using UnityEngine;

namespace InventorySystem
{
    public class GroundItem : MonoBehaviour
    {
        private static Camera _mainCamera;
        private void Awake()
        {
            _mainCamera ??= Camera.main;
            transform.LookAt(_mainCamera.transform);
            transform.Rotate(Vector3.up * 180f);
        }
    }
}
