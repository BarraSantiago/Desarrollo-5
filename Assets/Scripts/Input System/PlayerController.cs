using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerInput input;
    [SerializeField] private float speed = 50f;
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
        move = input.Gameplay.Movement.ReadValue<Vector2>();

        Vector3 movement = new Vector3(move.x, 0, move.y);
        rb.MovePosition(rb.position + movement * speed * Time.deltaTime);
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