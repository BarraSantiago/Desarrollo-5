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
        private ItemObject[] _availableItems;
        private string _itemName;
        private string _description;
        private int _originalPrice;
        private bool _isStackable;
        private int _maxStack;
        private bool _isCraftable;
        private ItemRecipe.ItemEntry[] _recipeEntries;
        private float craftChance;
        
        [MenuItem("Window/ItemObject Creator")]
        public static void ShowWindow()
        {
            GetWindow<ItemObjectCreatorWindow>("ItemObject Creator");
        }

        private void OnGUI()
        {
            _availableItems = AssetDatabase.FindAssets("t:ItemObject", new[] { "Assets/ScriptableObjects/Items" })
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<ItemObject>(path))
                .ToArray();

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
           
            CreateRecipe();

            if (GUILayout.Button("Create ItemObject"))
            {
                CreateItemObject();
            }
        }

        private void CreateRecipe()
        {
            _isCraftable = EditorGUILayout.Toggle("Is Craftable", _isCraftable);
            if (!_isCraftable) return;
            craftChance = EditorGUILayout.FloatField("Craft Chance", craftChance);

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

        private void CreateItemObject()
        {
            CheckForErrors();

            // Create a new ItemObject
            ItemObject itemObject = CreateInstance<ItemObject>();
            itemObject.uiDisplay = _sprite;
            itemObject.name = _itemName;
            itemObject.description = _description;
            itemObject.type = _type;
            itemObject.price = 0;
            itemObject.stackable = _isStackable;
            itemObject.maxStack = _maxStack;
            itemObject.data = itemObject.CreateItem();
            itemObject.data.listPrice = new ListPrice(_originalPrice);


            itemObject.data.craftable = _isCraftable;
            if (_isCraftable)
            {
                ItemRecipe itemRecipe = CreateInstance<ItemRecipe>();
                itemRecipe.items = _recipeEntries;
                itemRecipe.craftChance = craftChance;
                AssetDatabase.CreateAsset(itemRecipe, "Assets/ScriptableObjects/Recipes/" + _itemName + "Recipe.asset");
                AssetDatabase.SaveAssets();
                itemObject.data.recipe = itemRecipe;
            }

            GameObject gameObject = new GameObject(_itemName);
            gameObject.AddComponent<SpriteRenderer>().sprite = _sprite;
            var collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            GroundItem groundItem = gameObject.AddComponent<GroundItem>();
            groundItem.amount = 1;
            groundItem.item = itemObject;

            PrefabUtility.SaveAsPrefabAsset(gameObject, "Assets/Prefabs/Items/" + _itemName + ".prefab");
            
            itemObject.characterDisplay = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Items/" + _itemName + ".prefab");;
            
            AssetDatabase.CreateAsset(itemObject, "Assets/ScriptableObjects/Items/" + _itemName + ".asset");
            AssetDatabase.SaveAssets();
            DestroyImmediate(gameObject);

            ItemDatabaseObject itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabaseObject>("Assets/ScriptableObjects/Databases/MainDatabase.asset");

            if (itemDatabase)
            {
                itemDatabase.AddItem(itemObject);
            }
            else
            {
                Debug.LogError("ItemDatabaseObject not found");
            }
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

            if (errors.Any())
            {
                throw new ArgumentException("Errors detected: " + string.Join(", ", errors));
            }
        }
    }
}