using UnityEngine;
using UnityEngine.UI;

namespace Store.DayCycle
{
    public class RotationImageChanger : MonoBehaviour
    {
        [SerializeField]
        private enum RotationAxis
        {
            X,
            Y,
            Z
        }
        [SerializeField] private Sprite[] images;
        [SerializeField] private bool useLocalRotation = false;
        [SerializeField] private Animator animator;
        [SerializeField] private RotationAxis rotationAxis = RotationAxis.Z;
        
        private SpriteRenderer spriteRenderer;
        private Image uiImage;
        private int currentImageIndex = 0;
        private float thresholdAngle = 45f;

        private void Awake()
        {
            // Get the component that will display the image
            spriteRenderer = GetComponent<SpriteRenderer>();
            uiImage = GetComponent<Image>();

            if (spriteRenderer == null && uiImage == null)
            {
                Debug.LogError("RotationImageChanger requires either a SpriteRenderer or Image component!");
            }
        }

        private void Update()
        {
            float currentRotation = GetCurrentRotation();

            // If rotation has reached or passed threshold angle
            if (currentRotation >= thresholdAngle)
            {
                // Move to next image in array
                currentImageIndex = (currentImageIndex + 1) % images.Length;
                UpdateImage();

                // Reset rotation back to 0
                ResetRotation();
            }
        }

        private float GetCurrentRotation()
        {
            if (useLocalRotation)
            {
                return rotationAxis switch
                {
                    RotationAxis.X => transform.localEulerAngles.x % 360f,
                    RotationAxis.Y => transform.localEulerAngles.y % 360f,
                    _ => transform.localEulerAngles.z % 360f
                };
            }
            else
            {
                return rotationAxis switch
                {
                    RotationAxis.X => transform.eulerAngles.x % 360f,
                    RotationAxis.Y => transform.eulerAngles.y % 360f,
                    _ => transform.eulerAngles.z % 360f
                };
            }
        }

        private void ResetRotation()
        {
            Vector3 rotation = useLocalRotation ? transform.localEulerAngles : transform.eulerAngles;

            switch (rotationAxis)
            {
                case RotationAxis.X:
                    rotation.x = 0f;
                    break;
                case RotationAxis.Y:
                    rotation.y = 0f;
                    break;
                default:
                    rotation.z = 0f;
                    break;
            }

            if (useLocalRotation)
                transform.localEulerAngles = rotation;
            else
                transform.eulerAngles = rotation;
        }

        private void UpdateImage()
        {
            if (images == null || images.Length == 0) return;

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = images[currentImageIndex];
            }
            else if (uiImage != null)
            {
                uiImage.sprite = images[currentImageIndex];
            }
            animator.SetTrigger("Play");
        }
    }
}