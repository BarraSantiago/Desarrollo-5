using System.Linq;
using Interactable;
using InventorySystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private GameObject inventoryUI;
        [SerializeField] private GameObject debugUI;
        [SerializeField] private InventoryObject playerInventory;
        [SerializeField] private Color highlightColor = Color.yellow;
        private GameObject lastHighlightedObject;
        private Color originalColor;

        private void Update()
        {
            HighlightInteractable();
        }

        private void HighlightInteractable()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                RaycastHit[] hits = Physics.RaycastAll(ray);

                if (hits.Any(hit => hit.collider.gameObject.layer == LayerMask.NameToLayer("UI")))
                {
                    return;
                }
                
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    HighlightObject(hit.collider.gameObject);
                }
                else
                {
                    ResetHighlight();
                }
            }
            else
            {
                ResetHighlight();
            }
        }

      
        public void OnInteract(InputValue context)
        {
            RaycastFromMouse();
        }
        
        public void OnInventoryOpen(InputValue context)
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);

            if (!inventoryUI.activeSelf) return;

            playerInventory.UpdateInventory();
        }

        public void OnPause(InputValue context)
        {
            Application.Quit();
        }
        
        public void OnDebug(InputValue ctx)
        {
            debugUI?.SetActive(!debugUI.activeSelf);
        }
        
        private void HighlightObject(GameObject obj)
        {
            if (lastHighlightedObject && lastHighlightedObject != obj)
            {
                ResetHighlight();
            }

            Renderer renderer = obj.GetComponent<Renderer>();
            
            if (renderer)
            {
                if(renderer.material.color != highlightColor) originalColor = renderer.material.color;
                renderer.material.color = highlightColor;
            }

            lastHighlightedObject = obj;
        }

        private void ResetHighlight()
        {
            if (!lastHighlightedObject) return;
            
            Renderer renderer = lastHighlightedObject.GetComponent<Renderer>();
            
            if (renderer)
            {
                renderer.material.color = originalColor;
            }

            lastHighlightedObject = null;
        }


        private void RaycastFromMouse()
        {
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            if (hits.Any(hit => hit.collider.gameObject.layer == LayerMask.NameToLayer("UI")))
            {
                return;
            }

            foreach (var hit in hits)
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable == null) continue;
                interactable.Interact();
                break; // Interact with the first interactable object hit
            }
        }
    }
}