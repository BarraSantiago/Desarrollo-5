using System.Collections;
using InventorySystem;
using Player;
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

        private PlayerInput input;
        private Vector2 moveInput;
        private Rigidbody rb;
        private Vector3 dashDirection;
        private bool isDashing;

        private void Awake()
        {
            input = new PlayerInput();
            rb = GetComponent<Rigidbody>();
            playerAttack = GetComponent<PlayerAttack>();
            input.Gameplay.Attack.performed += ctx => playerAttack.Attack();
        }

        private void Update()
        {
            if (!isDashing)
            {
                RotatePlayer();
            }
            float speed = moveInput.magnitude * moveSpeed;
            animator.SetFloat("speed", speed);

        }

        private void FixedUpdate()
        {
            if (!isDashing)
            {
                MovePlayer();
            }
        }

        private void MovePlayer()
        {
            Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);
            rb.MovePosition(rb.position + movement * (moveSpeed * Time.deltaTime));
        }

        private void RotatePlayer()
        {
            if (moveInput == Vector2.zero) return;
            
            Vector3 direction = new Vector3(moveInput.x, 0, moveInput.y);
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, moveSpeed * Time.deltaTime);
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

        public void OnMovement(InputValue context)
        {
            moveInput = context.Get<Vector2>();
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
            input.Enable();
        }

        private void OnDisable()
        {
            input.Disable();
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