using System;
using System.Collections.Generic;
using System.Linq;
using InventorySystem;
using Store;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class ItemObjectCreatorWindow : EditorWindow
    {
        private Sprite _sprite;
        private ItemType _type;
        private ItemBuff[] _itemBuffs;
        private ItemObject[] _availableItems;
        private string _itemName;
        private string _description;
        private int _originalPrice;
        private bool _isStackable;
        private int _maxStack;
        private bool _isCraftable;
        private ItemRecipe.ItemEntry[] _recipeEntries;
        
        const string ItemPrefabsPath = "Assets/Prefabs/Items/";
        const string RecipiesPath = "Assets/ScriptableObjects/Recipes/";
        private const string ItemObjectPath = "Assets/ScriptableObjects/Items/";
        private const string DatabasePath = "Assets/ScriptableObjects/Databases/MainDatabase.asset";

        [MenuItem("Window/ItemObject Creator")]
        public static void ShowWindow()
        {
            GetWindow<ItemObjectCreatorWindow>("ItemObject Creator");
        }

        private void OnGUI()
        {
            _availableItems = AssetDatabase.FindAssets("t:ItemObject", new[] { ItemObjectPath })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ItemObject>)
                .ToArray();

            GenerateGuiFields();
            CreateBuffs();
            CreateRecipe();

            if (GUILayout.Button("Create ItemObject"))
            {
                CreateItemObject();
            }
        }

        private void GenerateGuiFields()
        {
            GUILayout.Label("Create a new ItemObject", EditorStyles.boldLabel);

            _sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", _sprite, typeof(Sprite), false);
            _itemName = EditorGUILayout.TextField("Name", _itemName);
            _description = EditorGUILayout.TextField("Description", _description);
            _type = (ItemType)EditorGUILayout.EnumPopup("Type", _type);
            _originalPrice = EditorGUILayout.IntField("Original Price", _originalPrice);
            _isStackable = EditorGUILayout.Toggle("Is Stackable", _isStackable);
            
            if (_isStackable)
            {
                _maxStack = EditorGUILayout.IntField("Max Stack", _maxStack);
            }
        }

        private void CreateRecipe()
        {
            _isCraftable = EditorGUILayout.Toggle("Is Craftable", _isCraftable);
            if (!_isCraftable) return;
            
            int entryCount = EditorGUILayout.IntField("Number of Recipe Entries",
                _recipeEntries?.Length ?? 0);
            
            if (entryCount != (_recipeEntries?.Length ?? 0))
            {
                _recipeEntries = new ItemRecipe.ItemEntry[Mathf.Min(entryCount, 3)];
                for (int i = 0; i < _recipeEntries.Length; i++)
                {
                    _recipeEntries[i] = new ItemRecipe.ItemEntry();
                }
            }

            if (_recipeEntries == null) return;
            {
                for (int i = 0; i < _recipeEntries.Length; i++)
                {
                    EditorGUILayout.LabelField($"Recipe Entry {i + 1}", EditorStyles.boldLabel);

                    int selectedIndex = Array.FindIndex(_availableItems,
                        item => item.data.id == _recipeEntries[i].itemID);
                    selectedIndex = EditorGUILayout.Popup("Item", selectedIndex,
                        _availableItems.Select(item => item.name).ToArray());

                    _recipeEntries[i].itemID = _availableItems[selectedIndex].data.id;

                    _recipeEntries[i].amount = EditorGUILayout.IntField("Amount", _recipeEntries[i].amount);
                }
            }
        }
        
        private void CreateBuffs()
        {
            int buffCount = EditorGUILayout.IntField("Number of Buffs", _itemBuffs?.Length ?? 0);
            
            if (buffCount != (_itemBuffs?.Length ?? 0))
            {
                _itemBuffs = new ItemBuff[buffCount];
                for (int i = 0; i < buffCount; i++)
                {
                    _itemBuffs[i] = new ItemBuff(0, 0);
                }
            }

            if (_itemBuffs == null) return;
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
        }

        private void CreateItemObject()
        {
            CheckForErrors();

            ItemObject itemObject = GenerateItemObject();

            var gameObject = GenerateGameObject(itemObject);
            
            itemObject.characterDisplay = AssetDatabase.LoadAssetAtPath<GameObject>(ItemPrefabsPath + _itemName + ".prefab");
            
            AssetDatabase.CreateAsset(itemObject, ItemObjectPath + _itemName + ".asset");
            AssetDatabase.SaveAssets();
            
            DestroyImmediate(gameObject);

            ItemDatabaseObject itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabaseObject>(DatabasePath);

            if (itemDatabase)
            {
                itemDatabase.AddItem(itemObject);
            }
            else
            {
                Debug.LogError("ItemDatabaseObject not found");
            }
        }

        private GameObject GenerateGameObject(ItemObject itemObject)
        {
            GameObject gameObject = new GameObject(_itemName);
            gameObject.AddComponent<SpriteRenderer>().sprite = _sprite;
            var collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            GroundItem groundItem = gameObject.AddComponent<GroundItem>();
            groundItem.amount = 1;
            groundItem.item = itemObject;
            PrefabUtility.SaveAsPrefabAsset(gameObject, ItemPrefabsPath + _itemName + ".prefab");
            return gameObject;
        }

        private ItemObject GenerateItemObject()
        {
            ItemObject itemObject = CreateInstance<ItemObject>();
            itemObject.uiDisplay = _sprite;
            itemObject.name = _itemName;
            itemObject.description = _description;
            itemObject.type = _type;
            itemObject.price = 0;
            itemObject.data = itemObject.CreateItem();
            itemObject.data.listPrice = new ListPrice(_originalPrice);

            itemObject.data.buffs = _itemBuffs;

            itemObject.data.craftable = _isCraftable;
            
            if (!_isCraftable) return itemObject;
            
            ItemRecipe itemRecipe = CreateInstance<ItemRecipe>();
            itemRecipe.items = _recipeEntries;
            AssetDatabase.CreateAsset(itemRecipe, RecipiesPath + _itemName + "Recipe.asset");
            AssetDatabase.SaveAssets();
            itemObject.data.recipe = itemRecipe;

            return itemObject;
        }

        private void CheckForErrors()
        {
            List<string> errors = new List<string>();

            if (!_sprite)
            {
                errors.Add("Sprite is not set");
            }
            if (string.IsNullOrEmpty(_itemName))
            {
                errors.Add("Item Name is not set");
            }
            if (string.IsNullOrEmpty(_description))
            {
                errors.Add("Description is not set");
            }
            if (_originalPrice <= 0)
            {
                errors.Add("Original Price must be greater than 0");
            }
            if (_isCraftable && (_recipeEntries == null || _recipeEntries.Length == 0))
            {
                errors.Add("Recipe Entries are not set");
            }
            if (string.IsNullOrEmpty(DatabasePath))
            {
                errors.Add("Missing Database Path");
            }

            if (errors.Any())
            {
                throw new ArgumentException("Errors detected: " + string.Join(", ", errors));
            }
        }
    }
}