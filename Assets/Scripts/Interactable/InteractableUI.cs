using UnityEngine;

namespace Interactable
{
    public class InteractableUI : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float distance = 2.5f;
        [SerializeField] private GameObject text;
        
        private void Update()
        {
            text.SetActive(CheckDistance());
        }

        private bool CheckDistance()
        {
            return Vector3.Distance(playerTransform.position, transform.position) < distance;
        }
    }
}