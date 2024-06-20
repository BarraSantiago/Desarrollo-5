using UnityEngine;

namespace player
{
    public class MouseIndicator : MonoBehaviour
    {
        [SerializeField] private float offset = 40;
        private Camera mainCamera;
        
        private void Start()
        {
            mainCamera = Camera.main;
        }
        
        void Update()
        {
            RotateCombatMouseCircle();
        }

        public void RotateCombatMouseCircle() 
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            Vector3 playerPosition = transform.position;

            Vector3 direction = mousePosition - playerPosition;
            direction.y = 0f;

            float angleRadians = Mathf.Atan2(direction.z, direction.x);
            float angleDegrees = angleRadians * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(90, 0, angleDegrees + offset);
        }

        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            Ray ray = mainCamera.ScreenPointToRay(mouseScreenPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) return hit.point;

            return Vector3.zero;
        }
    }
}
