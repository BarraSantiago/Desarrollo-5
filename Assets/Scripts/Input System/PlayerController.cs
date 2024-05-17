using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input_System
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float speed = 50f;
        //[SerializeField] private GameObject inventoryUI;
        private PlayerInput input;
        private Vector2 move;
        private Rigidbody rb;

        public float dashSpeed;
        public float dashTime;
        private Vector3 dashDirection;

        private void Awake()
        {
            input = new PlayerInput();
            rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            MovePlayer();

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                // Obtener dirección del movimiento
                
                StartCoroutine(Dash());
            }
        }

        private void MovePlayer()
        {
            Vector3 movement = new Vector3(move.x, 0, move.y);
            rb.MovePosition(rb.position + movement * (speed * Time.deltaTime));
        }

        public void OnMovement(InputValue context)
        {
            move = context.Get<Vector2>();
        }

        public void OnInventoryOpen(InputValue context)
        {
            //inventoryUI.SetActive(!inventoryUI.activeSelf);
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

        IEnumerator Dash()
        {
            dashDirection = new Vector3(move.x, 0, move.y);
            float startTime = Time.time;

            while (Time.time < startTime + dashTime)
            {
                rb.MovePosition(rb.position + dashDirection.normalized * dashSpeed * Time.deltaTime);

                yield return null;
            }
        }
    }
}