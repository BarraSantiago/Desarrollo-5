using Store;
using UnityEditor;
using UnityEngine;

namespace InventorySystem
{
    public class ItemObjectCreatorWindow : EditorWindow
    {
        private Sprite _sprite;
        private string _itemName;
        private string _description;
        private ItemType _type;
        private int _originalPrice;
        private ItemBuff[] _itemBuffs;

        [MenuItem("Window/ItemObject Creator")]
        public static void ShowWindow()
        {
            GetWindow<ItemObjectCreatorWindow>("ItemObject Creator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Create a new ItemObject", EditorStyles.boldLabel);

            _sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", _sprite, typeof(Sprite), false);
            _itemName = EditorGUILayout.TextField("Name", _itemName);
            _description = EditorGUILayout.TextField("Description", _description);
            _type = (ItemType)EditorGUILayout.EnumPopup("Type", _type);
            _originalPrice = EditorGUILayout.IntField("Original Price", _originalPrice);

            // Add fields for ItemBuff properties
            int buffCount = EditorGUILayout.IntField("Number of Buffs", _itemBuffs != null ? _itemBuffs.Length : 0);
            if (buffCount != (_itemBuffs != null ? _itemBuffs.Length : 0))
            {
                _itemBuffs = new ItemBuff[buffCount];
                for (int i = 0; i < buffCount; i++)
                {
                    _itemBuffs[i] = new ItemBuff(0, 0);
                }
            }
            if (_itemBuffs != null)
            {
                for (int i = 0; i < _itemBuffs.Length; i++)
                {
                    EditorGUILayout.LabelField($"Buff {i + 1}", EditorStyles.boldLabel);
                    _itemBuffs[i].Min = EditorGUILayout.IntField("Min", _itemBuffs[i].Min);
                    _itemBuffs[i].Max = EditorGUILayout.IntField("Max", _itemBuffs[i].Max);
                    _itemBuffs[i].Duration = EditorGUILayout.FloatField("Duration", _itemBuffs[i].Duration);
                    _itemBuffs[i].stat = (Attributes)EditorGUILayout.EnumPopup("Stat", _itemBuffs[i].stat);
                    _itemBuffs[i].value = EditorGUILayout.IntField("Value", _itemBuffs[i].value);
                }
            }

            if (GUILayout.Button("Create ItemObject"))
            {
                CreateItemObject();
            }
        }

        private void CreateItemObject()
        {
            if (!_sprite)
            {
                Debug.LogError("Sprite is not set");
                return;
            }

            // Create a new ItemObject
            ItemObject itemObject = ScriptableObject.CreateInstance<ItemObject>();
            itemObject.uiDisplay = _sprite;
            itemObject.name = _itemName;
            itemObject.description = _description;
            itemObject.type = _type;
            itemObject.price = 0;
            itemObject.data = itemObject.CreateItem();
            itemObject.data.listPrice = new ListPrice(_originalPrice);

            // Add the ItemBuffs to the ItemObject
            itemObject.data.buffs = _itemBuffs;

            // Save the ItemObject as an asset
            

            // Create a new GameObject with the specified components
            GameObject gameObject = new GameObject(_itemName);
            gameObject.AddComponent<SpriteRenderer>().sprite = _sprite;
            gameObject.AddComponent<BoxCollider>();
            GroundItem groundItem = gameObject.AddComponent<GroundItem>();
            groundItem.amount = 1;
            groundItem.item = itemObject;

            // Save the GameObject as a prefab
            PrefabUtility.SaveAsPrefabAsset(gameObject, "Assets/Prefabs/Items/" + _itemName + ".prefab");
            
            itemObject.characterDisplay = gameObject;
            
            AssetDatabase.CreateAsset(itemObject, "Assets/ScriptableObjects/Items/" + _itemName + ".asset");
            AssetDatabase.SaveAssets();
            // Destroy the GameObject from the scene
            DestroyImmediate(gameObject);

            // Get a reference to the main ItemDatabaseObject
            ItemDatabaseObject itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabaseObject>("Assets/ScriptableObjects/Databases/MainDatabase.asset");

            // Add the new ItemObject to the ItemDatabaseObject
            if (itemDatabase)
            {
                itemDatabase.AddItem(itemObject);
            }
            else
            {
                Debug.LogError("ItemDatabaseObject not found");
            }
        }
    }
}