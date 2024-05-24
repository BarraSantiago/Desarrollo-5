using System.Collections;
using InventorySystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input_System
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float speed = 50f;
        [SerializeField] private GameObject inventoryUI;
        [SerializeField] private InventoryObject playerInventory;
        [SerializeField] private PlayerAttack dmg;

        public float dashSpeed;
        public float dashTime;

        private PlayerInput input;
        private Vector2 move;
        private Rigidbody rb;
        private Vector3 dashDirection;

        private void Awake()
        {
            input = new PlayerInput();
            rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            MovePlayer();
            RotatePlayer();
        }

        private void MovePlayer()
        {
            Vector3 movement = new Vector3(move.x, 0, move.y);
            rb.MovePosition(rb.position + movement * (speed * Time.deltaTime));
        }

        private void RotatePlayer()
        {
            if (move == Vector2.zero) return;
            
            Vector3 direction = new Vector3(move.x, 0, move.y);
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, speed * Time.deltaTime);
        }

        public void OnAttack(InputValue context)
        {
            dmg?.Attack();
        }
        
        public void OnDash(InputValue context)
        {
            StartCoroutine(Dash());
        }

        public void OnMovement(InputValue context)
        {
            move = context.Get<Vector2>();
        }

        public void OnInventoryOpen(InputValue context)
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
            if (inventoryUI.activeSelf)
            {
                playerInventory.UpdateInventory();
            }
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
            dashDirection = new Vector3(move.x, 0, move.y);
            float startTime = Time.time;

            while (Time.time < startTime + dashTime)
            {
                rb.MovePosition(rb.position + dashDirection.normalized * (dashSpeed * Time.deltaTime));

                yield return null;
            }
        }
    }
}