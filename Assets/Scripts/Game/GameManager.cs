using UnityEngine;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Player _player;
        [SerializeField] private GameObject groundItemPrefab;
        [SerializeField] private ItemDatabaseObject itemDatabase;

        private Inventory _inventory;

        private void Awake()
        {
            Initialize();
        }
        
        public void SpawnItem()
        {
            int randX = Random.Range(-6, 0);
            int randY = Random.Range(-3, 3);
            Vector3 position = new Vector3(randX, randY, 0);
            GameObject gameObject = Instantiate(groundItemPrefab, position, Quaternion.identity);
            int rand = Random.Range(0, itemDatabase.ItemObjects.Length);
            gameObject.GetComponent<GroundItem>().item = itemDatabase.ItemObjects[rand];
            gameObject.GetComponentInChildren<SpriteRenderer>().sprite = itemDatabase.ItemObjects[rand].uiDisplay;
        }

        private void Initialize()
        {
            LoadInventory();
        }

        private void LoadInventory()
        {
            //TODO load inventory here
        }
    }
}