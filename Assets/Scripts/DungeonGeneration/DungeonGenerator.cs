using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonGeneration
{
    public class DungeonGenerator : MonoBehaviour
    {
        #region Attributes
        [Header("Dungeon configuration")]
        [SerializeField] private int maxRooms = 10;
        [SerializeField] private int minRooms = 7;
        private int _maxRooms;
        private int _nCurrentRooms;
        private int _nDeadEnds = 3;

        private Queue<DungeonRoom> _pendingRooms;
        private List<DungeonRoom> _dungeonRooms;
        private List<GameObject> _dungeonRoomInstances;
        private List<GameObject> _propInstances;

        private List<GameObject> roomPrefabs;
        //public List<Enemy> enemyPrefabs;
        //private List<Enemy> _enemyInstances;

        #endregion

        private enum RoomDirections
        {
            Up = 0,
            Right,
            Down,
            Left
        }

        private class DungeonRoom
        {
            public int xPosition;
            public int zPosition;

            public int NeighboursCount => _neighbours.Count;

            private List<Tuple<RoomDirections, DungeonRoom>> _neighbours;

            public List<Tuple<RoomDirections, DungeonRoom>> Neighbours => _neighbours;

            public RoomTypes type = RoomTypes.Invalid;

            public DungeonRoom(int x, int z)
            {
                xPosition = x;
                zPosition = z;
                _neighbours = new List<Tuple<RoomDirections, DungeonRoom>>();
            }

            public bool HasNeighbourInDirection(RoomDirections direction)
            {
                return _neighbours.Any(n => n.Item1 == direction);
            }

            public void AddNeighbourInDirection(DungeonRoom room, RoomDirections direction)
            {
                _neighbours.Add(new Tuple<RoomDirections, DungeonRoom>(direction, room));
            }
        }

        private void Awake()
        {
            LoadRoomPrefabs();
        }

        private void LoadRoomPrefabs()
        {
            string roomsPath = "Prefabs/Rooms/";
            string[] roomPrefabNames =
                { "Room_Door_1", "Room_Door_2_Close", "Room_Door_2_Opposite", "Room_Door_3", "Room_Door_4" };
            roomPrefabs = new List<GameObject>();

            var sb = new System.Text.StringBuilder();
            foreach (var t in roomPrefabNames)
            {
                sb.Append(roomsPath).Append(t);
                GameObject room = Resources.Load<GameObject>(sb.ToString());
                if (!ReferenceEquals(room, null))
                    roomPrefabs.Add(room);
                else
                    Debug.LogError("Room prefab " + sb.ToString() + " could not be found in " + roomsPath);
                sb.Clear();
            }
        }

        #region Dungeon Generation

        public void GenerateDungeon()
        {
            GenerateDungeonLayout();
            GenerateSpecialRooms();

            InstantiateDungeon();

            //SpawnEnemies();
            SpawnSpecialRooms();
        }

        private void GenerateDungeonLayout()
        {
            _dungeonRooms = new List<DungeonRoom>();
            _maxRooms = GetDungeonMaxRoomCount();
            _nCurrentRooms = 0;
            _pendingRooms = new Queue<DungeonRoom>();
            DungeonRoom startRoom = new DungeonRoom(0, 0);
            _pendingRooms.Enqueue(startRoom);
            _dungeonRooms.Add(startRoom);

            while (_pendingRooms.Count > 0)
            {
                _nCurrentRooms++;
                DungeonRoom currentRoom = _pendingRooms.Dequeue();

                int nNeighbours = (_nCurrentRooms + _pendingRooms.Count < _maxRooms) ? UnityEngine.Random.Range(1, 4) : 0;
                for (int i = 0; i < nNeighbours; ++i)
                {
                    if (currentRoom.NeighboursCount >= 4) continue;
                    
                    RoomDirections newNeighbourDirection = GetRandomNeighbourDirection(currentRoom);
                    (DungeonRoom, bool) newNeighbour = GenerateNeighbour(currentRoom, newNeighbourDirection);
                    DungeonRoom newNeighbourRoom = newNeighbour.Item1;
                    bool neighbourJustCreated = newNeighbour.Item2;
                    currentRoom.AddNeighbourInDirection(newNeighbourRoom, newNeighbourDirection);
                    
                    if (!neighbourJustCreated) continue;
                    
                    _pendingRooms.Enqueue(newNeighbourRoom);
                    _dungeonRooms.Add(newNeighbourRoom);
                }
            }

            Debug.Log(" === DUNGEON HAS BEEN GENERATED === ");
        }

        private bool IsThereRoomInPosition(int x, int z)
        {
            return _dungeonRooms.Any(t => t.xPosition == x && t.zPosition == z);
        }

        private DungeonRoom GetRoomInPosition(int x, int z)
        {
            return _dungeonRooms.FirstOrDefault(t => t.xPosition == x && t.zPosition == z);
        }

        private void InstantiateDungeon()
        {
            GameObject environmentParent = GameObject.Find("===== ENVIRONMENT =====");

            _dungeonRoomInstances = new List<GameObject>();
            foreach (DungeonRoom room in _dungeonRooms)
            {
                GameObject roomPrefab = null;
                Quaternion roomRotation = Quaternion.identity;
                switch (room.NeighboursCount)
                {
                    case 1:
                        roomPrefab = roomPrefabs[0];
                        roomRotation = Get1DoorRoomRotation(room);
                        break;
                    case 2:
                        roomPrefab = HasOppositeNeighbours(room) ? roomPrefabs[2] : roomPrefabs[1];
                        roomRotation = Get2DoorRoomRotation(room);
                        break;
                    case 3:
                        roomPrefab = roomPrefabs[3];
                        roomRotation = Get3DoorRoomRotation(room);
                        break;
                    case 4:
                        roomPrefab = roomPrefabs[4];
                        break;
                    default:
                        break;
                }

                GameObject roomInstance = Instantiate(roomPrefab, new Vector3(room.xPosition * 24, 0, room.zPosition * 24),
                    roomRotation);
                if (!ReferenceEquals(environmentParent, null))
                    roomInstance.transform.parent = environmentParent.transform;
                _dungeonRoomInstances.Add(roomInstance);
            }
        }

        private Quaternion Get1DoorRoomRotation(DungeonRoom room)
        {
            Quaternion result = Quaternion.identity;

            if (room.NeighboursCount != 1) return result;
        
            if (room.HasNeighbourInDirection(RoomDirections.Right))
            {
                result = Quaternion.Euler(0, 90, 0);
            }
            else if (room.HasNeighbourInDirection(RoomDirections.Down))
            {
                result = Quaternion.Euler(0, 180, 0);
            }
            else if (room.HasNeighbourInDirection(RoomDirections.Left))
            {
                result = Quaternion.Euler(0, 270, 0);
            }

            return result;
        }

        private Quaternion Get2DoorRoomRotation(DungeonRoom room)
        {
            Quaternion result = Quaternion.identity;

            if (room.NeighboursCount != 2) return result;
        
            if (HasOppositeNeighbours(room))
            {
                if (room.HasNeighbourInDirection(RoomDirections.Up))
                    result = Quaternion.Euler(0, 90, 0);
            }
            else
            {
                if (room.HasNeighbourInDirection(RoomDirections.Up) &&
                    room.HasNeighbourInDirection(RoomDirections.Right))
                {
                    result = Quaternion.Euler(0, 90, 0);
                }
                else if (room.HasNeighbourInDirection(RoomDirections.Right) &&
                         room.HasNeighbourInDirection(RoomDirections.Down))
                {
                    result = Quaternion.Euler(0, 180, 0);
                }
                else if (room.HasNeighbourInDirection(RoomDirections.Left) &&
                         room.HasNeighbourInDirection(RoomDirections.Down))
                {
                    result = Quaternion.Euler(0, 270, 0);
                }
            }

            return result;
        }

        private Quaternion Get3DoorRoomRotation(DungeonRoom room)
        {
            Quaternion result = Quaternion.identity;

            if (room.NeighboursCount != 3) return result;
        
            if (room.HasNeighbourInDirection(RoomDirections.Right) &&
                !room.HasNeighbourInDirection(RoomDirections.Left))
            {
                result = Quaternion.Euler(0, 90, 0);
            }
            else if (room.HasNeighbourInDirection(RoomDirections.Down) &&
                     !room.HasNeighbourInDirection(RoomDirections.Up))
            {
                result = Quaternion.Euler(0, 180, 0);
            }
            else if (room.HasNeighbourInDirection(RoomDirections.Left) &&
                     !room.HasNeighbourInDirection(RoomDirections.Right))
            {
                result = Quaternion.Euler(0, 270, 0);
            }

            return result;
        }

        private bool HasOppositeNeighbours(DungeonRoom room)
        {
            if (room.NeighboursCount != 2) return false;
        
            switch (room.Neighbours[0].Item1)
            {
                case RoomDirections.Up: 
                    return room.Neighbours[1].Item1 == RoomDirections.Down;
                case RoomDirections.Right: 
                    return room.Neighbours[1].Item1 == RoomDirections.Left;
                case RoomDirections.Down: 
                    return room.Neighbours[1].Item1 == RoomDirections.Up;
                case RoomDirections.Left: 
                    return room.Neighbours[1].Item1 == RoomDirections.Right;
                default: 
                    return false;
            }
        }

        private RoomDirections GetRandomNeighbourDirection(DungeonRoom currentRoom)
        {
            bool found = false;
            RoomDirections direction = RoomDirections.Up;
        
            while (!found)
            {
                direction = GetRandomDirection();
                if (!currentRoom.HasNeighbourInDirection(direction))
                    found = true;
            }

            return direction;
        }

        private RoomDirections GetRandomDirection()
        {
            return (RoomDirections)UnityEngine.Random.Range(0, 4);
        }

        private (DungeonRoom, bool) GenerateNeighbour(DungeonRoom currentRoom, RoomDirections direction)
        {
            (DungeonRoom, bool) resultTuple;
            DungeonRoom result;
            bool roomCreated = false;
            (int, int)[] newRoomPositions =
            {
                (currentRoom.xPosition, currentRoom.zPosition + 1),
                (currentRoom.xPosition + 1, currentRoom.zPosition),
                (currentRoom.xPosition, currentRoom.zPosition - 1),
                (currentRoom.xPosition - 1, currentRoom.zPosition)
            };

            (int, int) newPosition = newRoomPositions[(int)direction];
            if (IsThereRoomInPosition(newPosition.Item1, newPosition.Item2))
                result = GetRoomInPosition(newPosition.Item1, newPosition.Item2);
            else
            {
                result = new DungeonRoom(newPosition.Item1, newPosition.Item2);
                roomCreated = true;
            }

            RoomDirections oppositeDirection = (RoomDirections)(((int)direction + 2) % 4);

            result.AddNeighbourInDirection(currentRoom, oppositeDirection);

            resultTuple.Item1 = result;
            resultTuple.Item2 = roomCreated;
            return resultTuple;
        }

        private int GetDungeonMaxRoomCount()
        {
            return Mathf.RoundToInt(1 + UnityEngine.Random.Range(minRooms, maxRooms));
        }

        private void GenerateSpecialRooms()
        {
            bool bossGenerated = false;
            _dungeonRooms[0].type = RoomTypes.Start;

            for (int i = _dungeonRooms.Count - 1; i >= 0; --i)
            {
                DungeonRoom room = _dungeonRooms[i];
                if (room.NeighboursCount == 1)
                {
                    if (!bossGenerated)
                    {
                        room.type = RoomTypes.Boss;
                        bossGenerated = true;
                    }
                    else
                    {
                        RoomTypes roomType = GetRandomSpecialRoomType();
                        room.type = roomType;
                    }
                }
            }
        }

        private RoomTypes GetRandomSpecialRoomType()
        {
            float rng = UnityEngine.Random.Range(0f, 1f);
            if (rng < 0.5f)
                return RoomTypes.Treasure;
            else if (rng < 0.9f)
                return RoomTypes.Enemies;
            else
                return RoomTypes.Empty;
        }

        private void SpawnSpecialRooms()
        {
            _propInstances = new List<GameObject>();

            for (int i = 0; i < _dungeonRooms.Count; ++i)
            {
                DungeonRoom room = _dungeonRooms[i];
                switch (room.type)
                {
                    case RoomTypes.Treasure:
                        //_propInstances.Add(SpawnProp(PropsID.Treasurechest, _dungeonRoomInstances[i].transform.position));
                        break;
                    case RoomTypes.Start:
                    
                        //_propInstances.Add(SpawnProp(PropsID.Bonfire,
                        //    _dungeonRoomInstances[0].transform.position + Vector3.up * 0.1f));
                        //if (_dungeonRooms[0].NeighboursCount != 2)
                        //    _propInstances.Add(SpawnProp(PropsID.Startroomprops, _dungeonRoomInstances[0].transform.position));
                        break;
                    
                
                    case RoomTypes.Boss:
                    /*
                    // Spawn Boss Door
                    Transform roomTransform = _dungeonRoomInstances[i].transform;
                    Vector3 doorPosition = roomTransform.position + roomTransform.forward * 12;
                    float doorRotationY = roomTransform.eulerAngles.y;

                    GameObject doorGo = SpawnProp(PROPS_ID.BOSSDOOR, doorPosition, Quaternion.Euler(0, doorRotationY, 0));
                    _propInstances.Add(doorGo);
                    // Spawn Boss Enemy
                    GameObject bossGo =
                        SpawnEnemy(BOSS_ID.BOSS_BARBARIANGIANT, _dungeonRoomInstances[i].transform.position);
                    bossGo.GetComponent<Enemy>()?.SetType(ENEMY_TYPE.BOSS);
                    // Link door to enemy
                    doorGo.GetComponent<BossDoor>().LinkToEnemy(bossGo.GetComponent<Enemy>());
                    break;*/
                
                    case RoomTypes.Empty:
                    case RoomTypes.Enemies:
                    case RoomTypes.Invalid:
                    default:
                        break;
                }
            }
        }

        #endregion

        #region Enemies

        /*
    private void SpawnEnemies()
    {
        _enemyInstances = new List<Enemy>();
        for (int i = 1; i < _dungeonRoomInstances.Count; ++i)
        {
            if (_dungeonRooms[i].NeighboursCount <= 1) continue;
            
            GameObject room = _dungeonRoomInstances[i];
            GameObject enemiesParentObject = new GameObject("Enemy Instances");
            enemiesParentObject.transform.parent = room.transform;
            enemiesParentObject.transform.localPosition = Vector3.zero;
            Transform enemySpawnsParent = room.transform.Find("EnemySpawnPoints");
            if (!ReferenceEquals(enemySpawnsParent, null))
            {
                List<Transform> enemySpawns =
                    new List<Transform>(enemySpawnsParent.GetComponentsInChildren<Transform>());
                enemySpawns.RemoveAt(0);

                foreach (var spawn in enemySpawns.Where(spawn => UnityEngine.Random.Range(0f, 1f) <= 0.75f))
                {
                    Enemy e = Instantiate(GetRandomEnemyPrefab(), spawn.position, Quaternion.identity,
                        enemiesParentObject.transform);
                    e.SetType(ENEMY_TYPE.NORMAL);
                    _enemyInstances.Add(e);
                }
            }
        }
    }

    private Enemy GetRandomEnemyPrefab()
    {
        int enemyCount = enemyPrefabs.Count;
        return enemyPrefabs[UnityEngine.Random.Range(0, enemyCount)];
    }

    private GameObject SpawnEnemy(BOSS_ID bossId, Vector3 position)
    {
        // TODO: Spawn Enemies
        string bossPath = "Prefabs/Enemies/Bosses/";

        var sb = new System.Text.StringBuilder();
        sb.Append(bossPath).Append(bossId.ToString());

        GameObject boss = Resources.Load<GameObject>(sb.ToString());
        if (!ReferenceEquals(boss, null))
            return Instantiate(boss, position, Quaternion.identity);
        else
            Debug.LogError("Boss prefab " + sb.ToString() + " could not be found in " + bossPath);
        sb.Clear();
        return null;
    }

    
    public Enemy GetClosestEnemyToPlayer()
    {
        Vector3 playerPosition = ThirdPersonControllerMovement.s.transform.position;
        Enemy result = _enemyInstances[0];
        float minDistance = Vector3.Distance(playerPosition, result.transform.position);

        for (int i = 1; i < _enemyInstances.Count; ++i)
        {
            Enemy currentEnemy = _enemyInstances[i];
            float distance = Vector3.Distance(playerPosition, currentEnemy.transform.position);
            if (distance < minDistance)
            {
                result = currentEnemy;
                minDistance = distance;
            }
        }

        return result;
    }*/

        #endregion

        #region Special Rooms

        private GameObject SpawnProp(PropsID propId, Vector3 position)
        {
            return SpawnProp(propId, position, Quaternion.identity);
        }

        private GameObject SpawnProp(PropsID propId, Vector3 position, Quaternion rotation)
        {
            string propsPath = "Prefabs/Props/";

            var sb = new System.Text.StringBuilder();
            sb.Append(propsPath).Append(propId.ToString());

            GameObject prop = Resources.Load<GameObject>(sb.ToString());
            if (!ReferenceEquals(prop, null))
            {
                return Instantiate(prop, position, rotation);
            }
            else
                Debug.LogError("Prop prefab " + sb.ToString() + " could not be found in " + propsPath);

            sb.Clear();

            return null;
        }

        #endregion

        public void DeleteDungeon()
        {
            try
            {
                foreach (GameObject go in _propInstances)
                    Destroy(go);

                foreach (GameObject room in _dungeonRoomInstances)
                    Destroy(room);

                /*foreach (Enemy enemy in _enemyInstances)
                Destroy(enemy.gameObject);*/
            }
            catch (NullReferenceException e)
            {
                Debug.LogWarning("There is no dungeon to delete.");
            }
        }
    }
}