using UnityEngine;
using UnityEngine.Events;

namespace Utils
{
    public class CheckDisable : MonoBehaviour
    {
        public UnityEvent OnDisableEvent;

        private void OnDisable()
        {
            OnDisableEvent?.Invoke();
        }
    }
}