using UnityEngine;

namespace Utils
{
    public class LookAtCamera : MonoBehaviour
    {
        private static Camera _mainCamera;
        
        private void Start()
        {
            if(!_mainCamera) _mainCamera = Camera.main;
        }
        
        private void Update()
        {
            transform.LookAt(_mainCamera.transform);
        }
    }
}