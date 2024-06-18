using UnityEngine;

namespace player
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float zOffset = -10f;

        private void Start()
        {
            if (!playerTransform)
            {
                playerTransform = GameObject.FindWithTag("Player").transform;
            }
        }

        private void LateUpdate()
        {
            if (!playerTransform) return;

            transform.position = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z + zOffset);
        }
    }
}