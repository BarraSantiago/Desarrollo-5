using UnityEngine;
using UnityEngine.InputSystem;

namespace Input_System
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float speed = 50f;
        [SerializeField] private GameObject inventoryUI;
        [SerializeField] private CharacterController characterController;
        private PlayerInput input;
        private Vector2 move;
        private Rigidbody rb;
    

        private void Awake()
        {
            input = new PlayerInput();
            rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            MovePlayer();
        }

        private void MovePlayer()
        {
            Vector3 movement = new Vector3(move.x, 0, move.y);
            characterController.Move(movement * (speed * Time.deltaTime));
        }

        public void OnMovement(InputValue context)
        {
            move = context.Get<Vector2>();
        }

        public void OnInventoryOpen(InputValue context)
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
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
    }
}