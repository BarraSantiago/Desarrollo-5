using UnityEngine;

namespace InventorySystem
{
    public class GroundItem : MonoBehaviour
    {
        private Camera _mainCamera;
        
        private void Awake()
        {
            _mainCamera = Camera.main;
            
            if (_mainCamera != null)
            {
                transform.LookAt(_mainCamera.transform);
                transform.Rotate(Vector3.up * 180f);
            }
        }
    }
}
