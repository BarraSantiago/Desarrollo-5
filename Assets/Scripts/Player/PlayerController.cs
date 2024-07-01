using System.Collections;
using Interactable;
using InventorySystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 50f;
        [SerializeField] private GameObject inventoryUI;
        [SerializeField] private GameObject debugUI;
        [SerializeField] private InventoryObject playerInventory;
        [SerializeField] private float interactionRadius = 5f;
        [SerializeField] private AudioSource audioSource;

        public float MovementSpeed
        {
            get => moveSpeed;
            set => moveSpeed = value;
        }

        public float dashSpeed;
        public float dashTime;
        public Animator animator;

        private CharacterController _characterController;
        private PlayerAttack _playerAttack;
        private PlayerInput _playerInput;
        private Vector2 _moveInput;
        private Vector3 _dashDirection;
        private bool _isDashing;
        private MouseIndicator _mouseIndicator;
        private bool _canMove = true;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            _playerInput = new PlayerInput();
            _playerAttack = GetComponent<PlayerAttack>();
            _mouseIndicator = GetComponentInChildren<MouseIndicator>();
        }

        private void Update()
        {
            if (!_isDashing && _canMove)
            {
                MovePlayer();
                float speed = _moveInput.magnitude * MovementSpeed;
                animator.SetFloat("speed", speed);
            }
            if (audioSource) audioSource.enabled = _moveInput != Vector2.zero && !_isDashing && _canMove;
        }

        private void MovePlayer()
        {
           
            if (_moveInput == Vector2.zero) return;

            Vector3 movement = new Vector3(_moveInput.x, 0, _moveInput.y).normalized * (MovementSpeed * Time.deltaTime);
            _characterController.Move(movement);

            float targetAngle = Mathf.Atan2(_moveInput.x, _moveInput.y) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * MovementSpeed);
        }

        private void Movement(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

        private void OnRotate()
        {
            if (_moveInput == Vector2.zero) return;

            float targetAngle = Mathf.Atan2(_moveInput.x, _moveInput.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }

        public void OnAttack(InputValue context)
        {
            if (!_playerAttack) return;

            RotateTowardsMouse();

            _playerAttack?.Attack();
        }

        private void RotateTowardsMouse()
        {
            if (_mouseIndicator == null) return;

            Vector3 mousePosition = _mouseIndicator.GetMouseWorldPosition();
            Vector3 playerPosition = transform.position;
            Vector3 direction = mousePosition - playerPosition;
            direction.y = 0f;

            float angleRadians = Mathf.Atan2(direction.z, direction.x);
            float angleDegrees = angleRadians * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0, -angleDegrees - _mouseIndicator.GetOffset(), 0);
        }

        public void OnDash(InputValue context)
        {
            if (!_isDashing)
            {
                StartCoroutine(Dash());
            }
        }

        public void OnInventoryOpen(InputValue context)
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);

            if (!inventoryUI.activeSelf) return;

            playerInventory.UpdateInventory();
        }

        public void OnPause(InputValue context)
        {
            Application.Quit();
        }

        public void OnDebug(InputValue ctx)
        {
            debugUI?.SetActive(!debugUI.activeSelf);
        }

        public void OnInteract(InputValue context)
        {
            InteractWithNearbyObjects();
        }

        private void InteractWithNearbyObjects()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRadius);

            foreach (var hitCollider in hitColliders)
            {
                IInteractable interactable = hitCollider.GetComponent<IInteractable>();
                if (interactable == null) continue;

                bool interactionSuccess = interactable.Interact();

                if (interactionSuccess) break;
            }
        }


        private void OnEnable()
        {
            _playerInput.Enable();
            _playerInput.Gameplay.Movement.performed += Movement;
            _playerInput.Gameplay.Movement.canceled += Movement;
        }

        private void OnDisable()
        {
            _playerInput.Disable();
            _playerInput.Gameplay.Movement.performed -= Movement;
            _playerInput.Gameplay.Movement.canceled -= Movement;
        }

        private IEnumerator Dash()
        {
            _isDashing = true;
            _dashDirection = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;
            float startTime = Time.time;

            while (Time.time < startTime + dashTime)
            {
                _characterController.Move(_dashDirection * (dashSpeed * Time.deltaTime));
                yield return null;
            }

            _isDashing = false;
        }

        public void EnableMovement()
        {
            _canMove = true;
        }

        public void DisableMovement()
        {
            _canMove = false;
        }
    }
}