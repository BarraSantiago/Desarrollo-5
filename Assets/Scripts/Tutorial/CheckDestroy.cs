using System;
using Interactable;
using UnityEngine;

namespace Tutorial
{
    public class CheckDestroy : MonoBehaviour
    {
        [SerializeField] private OpenMenu menu;

        private void Start()
        {
            if(menu.HasBeenAccessed)
            {
                Destroy(gameObject);
            }

            menu.OnInteract += Delete;
        }
        
        private void Delete()
        {
            menu.OnInteract -= Delete;
            Destroy(gameObject);
        }
    }
}
