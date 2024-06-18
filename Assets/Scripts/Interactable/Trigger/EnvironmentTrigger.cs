using UnityEngine;
using UnityEngine.Events;

public class EnvironmentTrigger : MonoBehaviour
{
    [SerializeField] private string targetTag = string.Empty;
    [SerializeField] private bool onlyOneTrigger = false;
    [SerializeField] private UnityEvent customEvent = null;

    private BoxCollider boxCollider = null;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(targetTag))
        {
            customEvent?.Invoke();

            if (onlyOneTrigger)
            {
                boxCollider.enabled = false;
            }
        }
    }
}
