using Interactable;
using UnityEngine;
using UnityEngine.InputSystem;

namespace player
{
    public class PlayerController : MonoBehaviour
    {
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

        private void HighlightObject(GameObject obj)
        {
            if (lastHighlightedObject && lastHighlightedObject != obj)
            {
                ResetHighlight();
            }

            Renderer renderer = obj.GetComponent<Renderer>();
            
            if (renderer)
            {
                originalColor = renderer.material.color;
                renderer.material.color = Color.yellow; // Highlight color
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

        public void OnInteract(InputValue context)
        {
            RaycastFromMouse();
        }

        private void RaycastFromMouse()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (!Physics.Raycast(ray, out RaycastHit hit)) return;
            
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            interactable?.Interact();
        }
    }
}