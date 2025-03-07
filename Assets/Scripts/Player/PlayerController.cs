using System.Collections.Generic;
using System.Linq;
using Interactable;
using InventorySystem;
using Menu;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace player
{
    public enum CursorStates
    {
        Default,
        Hover,
        Interact
    }

    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private GameObject inventoryUI;
        [SerializeField] private GameObject debugUI;
        [SerializeField] private GameObject endDayStats;
        [SerializeField] private GameObject endDayInput;
        [SerializeField] private InventoryObject playerInventory;
        [SerializeField] private Texture2D[] cursors;

        public bool dayEnded;
        private GameObject lastHighlightedObject;
        private Color originalColor;
        private int _currentCursorState;
        private List<IInteractable> interactables = new List<IInteractable>();
        private PauseManager _pauseManager;
        
        private void Start()
        {
            Cursor.SetCursor(cursors[(int)CursorStates.Default], Vector2.zero, CursorMode.Auto);
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;

            interactables.AddRange(FindObjectsOfType<MonoBehaviour>().OfType<IInteractable>());
            _pauseManager = FindObjectOfType<PauseManager>();
        }

        private void Update()
        {
            UIHelper.UpdatePointerOverUIState();

            HighlightInteractable();
        }

        
        private void HighlightInteractable()
        {
            if (UIHelper.IsPointerOverUIElement())
            {
                SetCursor((int)CursorStates.Default);
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                RaycastHit[] hits = Physics.RaycastAll(ray);

                if (hits.Any(hit => hit.collider.gameObject.layer == LayerMask.NameToLayer("UI")))
                {
                    SetCursor((int)CursorStates.Default);
                    return;
                }

                IInteractable interactable = hit.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    SetCursor((int)CursorStates.Hover);
                }
                else
                {
                    SetCursor((int)CursorStates.Default);
                }
            }
            else
            {
                SetCursor((int)CursorStates.Default);
            }
        }


        public void OnInteract(InputValue context)
        {
            SetCursor((int)CursorStates.Interact);
            RaycastFromMouse();
        }

        public void OnInventoryOpen(InputValue context)
        {
            if (interactables.Any(interactable => interactable.State))
            {
                interactables.ForEach(interactable => interactable.Close());
                return;
            }

            inventoryUI.SetActive(!inventoryUI.activeSelf);

            if (!inventoryUI.activeSelf)
            {
                Destroy(MouseData.TempItemBeingDragged);
                return;
            }

            playerInventory.UpdateInventory();
        }

        public void OnPause(InputValue context)
        {
            _pauseManager.OnPause();
        }

        public void OnDebug(InputValue ctx)
        {
            debugUI?.SetActive(!debugUI.activeSelf);
        }

        /*
        private void HighlightObject(GameObject obj)
        {
            if (lastHighlightedObject && lastHighlightedObject != obj)
            {
                ResetHighlight();
            }

            Renderer renderer = obj.GetComponent<Renderer>();

            if (renderer)
            {
                if (renderer.material.color != highlightColor) originalColor = renderer.material.color;
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
        }*/


        private void RaycastFromMouse()
        {
            if (UIHelper.IsPointerOverUIElement())
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            if (hits.Any(hit => hit.collider.gameObject.layer == LayerMask.NameToLayer("UI")))
            {
                return;
            }

            foreach (RaycastHit hit in hits)
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable == null) continue;
                interactable.Interact();
                break; // Interact with the first interactable object hit
            }
        }

        private void OnEnter(InputValue context)
        {
            if (!dayEnded) return;
            endDayStats.SetActive(true);
            endDayInput.SetActive(false);
        }

        private void SetCursor(int cursorIndex)
        {
            if (cursorIndex < 0 || cursorIndex >= cursors.Length || cursorIndex == _currentCursorState) return;
            _currentCursorState = cursorIndex;

            Cursor.SetCursor(cursors[cursorIndex], Vector2.zero, CursorMode.Auto);
        }
    }
}