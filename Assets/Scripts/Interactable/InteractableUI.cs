using System;
using UnityEngine;

namespace Interactable
{
    public class InteractableUI : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float distance = 2.2f;
        [SerializeField] private GameObject text;
        private IInteractable interactable;

        private void Start()
        {
            interactable = GetComponent<IInteractable>();
            if (interactable == null)
            {
                throw new NullReferenceException("Interactable component not found");
            }
        }

        private void Update()
        {
            if (CheckDistance())
            {
                text.SetActive(true);
            }
            else
            {
                if(interactable.State)
                    interactable.Close();
                text.SetActive(false);
            }
        }

        private bool CheckDistance()
        {
            return Vector3.Distance(playerTransform.position, transform.position) < distance;
        }
    }
}