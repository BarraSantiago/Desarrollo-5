using System;
using Interactable;
using player;
using UnityEngine;

namespace InventorySystem
{
    public class GroundItem : MonoBehaviour, IInteractable
    {
        public ItemObject item;
        public int amount = 1;
        public bool droppedByPlayer = false;
        public float droppedTime;
        public bool State { get; }
        private static Camera _mainCamera;
        private void Awake()
        {
            _mainCamera ??= Camera.main;
            
            transform.LookAt(_mainCamera.transform);
        }

        public bool Interact()
        {
            Player player = FindObjectOfType<Player>();
            
            if (player is null || !player.inventory.AddItem(new Item(item), amount)) return false;
            
            Destroy(gameObject);
            return true;
        }

        public void Close()
        {
        }
    }
}
