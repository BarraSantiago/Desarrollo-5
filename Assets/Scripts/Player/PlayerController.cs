using System.Collections;
using InventorySystem;
using player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input_System
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 50f;
        [SerializeField] private GameObject inventoryUI;
        [SerializeField] private GameObject storeInventoryUI;
        [SerializeField] private InventoryObject playerInventory;
        
        private PlayerAttack playerAttack;

        public float dashSpeed;
        public float dashTime;
        public Animator animator;

        private PlayerInput playerInput;
        private Vector2 moveInput;
        private Rigidbody rb;
        private Vector3 dashDirection;
        private bool isDashing;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            playerInput = new PlayerInput();
            playerAttack = GetComponent<PlayerAttack>();
            playerInput.Gameplay.Attack.performed += ctx => playerAttack.Attack();
        }

        private void Update()
        {
            if (!isDashing)
            {
                MovePlayer();
            }

            float speed = moveInput.magnitude * moveSpeed;
            animator.SetFloat("speed", speed);

        }

        private void MovePlayer()
        {
            Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed * Time.deltaTime;
            rb.MovePosition(transform.position + movement);

            OnRotate();
        }

        private void Movement(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        private void OnRotate()
        {
            if (moveInput != Vector2.zero)
            {
                float targetAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
            }
        }

        public void OnAttack(InputValue context)
        {
            playerAttack?.Attack();
        }
        
        public void OnDash(InputValue context)
        {
            if (!isDashing)
            {
                StartCoroutine(Dash());
            }
        }

        public void OnInventoryOpen(InputValue context)
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
            if(storeInventoryUI) storeInventoryUI?.SetActive(!storeInventoryUI.activeSelf);

            if (!inventoryUI.activeSelf) return;
            
            playerInventory.UpdateInventory();
        }

        public void OnPause(InputValue context)
        {
            Application.Quit();
        }

        private void OnEnable()
        {
            playerInput.Enable();
            playerInput.Gameplay.Movement.performed += Movement;
            playerInput.Gameplay.Movement.canceled += Movement;
        }

        private void OnDisable()
        {
            playerInput.Disable();
            playerInput.Gameplay.Movement.performed -= Movement;
            playerInput.Gameplay.Movement.canceled -= Movement;
        }

        private IEnumerator Dash()
        {
            isDashing = true;
            dashDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
            float startTime = Time.time;

            while (Time.time < startTime + dashTime)
            {
                rb.MovePosition(rb.position + dashDirection * (dashSpeed * Time.deltaTime));
                yield return null;
            }

            isDashing = false;
        }
    }
}