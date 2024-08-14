using System;
using UnityEngine;

namespace Interactable
{
    public class InteractableUI : MonoBehaviour
    {
        private IInteractable interactable;

        private void Start()
        {
            interactable = GetComponent<IInteractable>();
            if (interactable == null)
            {
                throw new NullReferenceException("Interactable component not found");
            }
        }
    }
}