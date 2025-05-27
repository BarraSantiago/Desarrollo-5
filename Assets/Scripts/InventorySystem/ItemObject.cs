using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory System/Items/New Item")]
    public class ItemObject : ScriptableObject
    {
        public Sprite uiDisplay;
        public GameObject characterDisplay;
        public bool stackable;
        public int maxStack;
        public ItemType type;
        [TextArea(15, 20)] public string description;
        public Item data = new Item();
        public List<string> boneNames = new List<string>();

        private int price;
        public int Price
        {
            get => price;
            set
            {
                price = value;
                PlayerPrefs.SetInt(data.id.ToString(), value);
            }
            
        }

        private void Awake()
        {
            if(data != null)
            {
                price = PlayerPrefs.GetInt(data.id.ToString(), 0);
            }
        }

        public Item CreateItem()
        {
            Item newItem = new Item(this);
            return newItem;
        }

        private void OnValidate()
        {
            boneNames.Clear();
            if (characterDisplay == null) 
                return;
            if(!characterDisplay.GetComponent<SkinnedMeshRenderer>())
                return;

            SkinnedMeshRenderer renderer = characterDisplay.GetComponent<SkinnedMeshRenderer>();
            Transform[] bones = renderer.bones;

            foreach (Transform t in bones)
            {
                boneNames.Add(t.name);
            }
        }
    }
}

