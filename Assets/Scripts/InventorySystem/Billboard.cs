using UnityEngine;

namespace InventorySystem
{
    public class Billboard : MonoBehaviour
    {
        private void LateUpdate()
        {
            transform.forward = Camera.main.transform.forward;
        }
    }
}
