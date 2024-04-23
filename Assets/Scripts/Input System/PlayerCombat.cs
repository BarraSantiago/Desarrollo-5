using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] Camera _Camera;
    Vector2 _MousePos;
    PlayerInput _Input;
    Rigidbody2D _Rigidbody;

    private void Awake()
    {
        _Input = new PlayerInput();
        _Rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 facingDirection = _MousePos - _Rigidbody.position;
        float angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg - 90;

        if (Input.GetMouseButton(0))
        {
            _Rigidbody.MoveRotation(angle);
        }
    }

    private void OnEnable()
    {
        _Input.Enable();
        _Input.Gameplay.MousePos.performed += OnMousePos;
    }

    private void OnDisable()
    {
        _Input.Gameplay.MousePos.performed -= OnMousePos;

        _Input.Disable();
    }

    private void OnMousePos(InputAction.CallbackContext context)
    {
        _MousePos = _Camera.ScreenToWorldPoint(context.ReadValue<Vector2>());
    }
}
