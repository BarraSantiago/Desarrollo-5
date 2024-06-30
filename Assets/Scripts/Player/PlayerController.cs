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
        [SerializeField] private InventoryObject playerInventory;
        [SerializeField] private float interactionRadius = 5f;
        
        public float MovementSpeed
        {
            get => moveSpeed;
            set => moveSpeed = value;
        }

        public float dashSpeed;
        public float dashTime;
        public Animator animator;

        private PlayerAttack _playerAttack;
        private PlayerInput _playerInput;
        private Vector2 _moveInput;
        private Rigidbody _rb;
        private Vector3 _dashDirection;
        private bool _isDashing;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _playerInput = new PlayerInput();
            _playerAttack = GetComponent<PlayerAttack>();
        }

        private void Update()
        {
            if (!_isDashing)
            {
                MovePlayer();
            }

            float speed = _moveInput.magnitude * MovementSpeed;
            animator.SetFloat("speed", speed);
        }

        private void MovePlayer()
        {
            Vector3 movement = new Vector3(_moveInput.x, 0, _moveInput.y) * (MovementSpeed * Time.deltaTime);
            _rb.MovePosition(transform.position + movement);

            OnRotate();
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
            _playerAttack?.Attack();
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
                _rb.MovePosition(_rb.position + _dashDirection * (dashSpeed * Time.deltaTime));
                yield return null;
            }

            _isDashing = false;
        }
    }
}