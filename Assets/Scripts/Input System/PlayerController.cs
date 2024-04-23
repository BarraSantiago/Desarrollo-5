using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

//https://www.youtube.com/watch?v=ud1y5t0sOQc

public class PlayerController : MonoBehaviour
{
    [SerializeField] float _Speed = 3;
    PlayerInput _Input;
    Vector2 _Movement;
    Vector2 _LastMovement; // Para almacenar la última dirección del movimiento
    Rigidbody2D _Rigidbody;

    

    //DASH
    private bool canDash = true;
    private bool isDashing = false;
    private float dashingPower = 24f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;
    //DASH

    private void Awake()
    {
        _Input = new PlayerInput();
        _Rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        _Input.Enable();

        _Input.Gameplay.Movement.performed += OnMovement;
        _Input.Gameplay.Movement.canceled += OnMovement;

    }

    private void OnDisable()
    {
        _Input.Gameplay.Movement.performed -= OnMovement;
        _Input.Gameplay.Movement.canceled -= OnMovement;

        _Input.Disable();
    }

    private void OnMovement(InputAction.CallbackContext context)
    {
        _Movement = context.ReadValue<Vector2>();

        // Almacenar la última dirección del movimiento
        if (_Movement.magnitude > 0)
        {
            _LastMovement = _Movement.normalized;
        }
    }

    private void Update()
    {
        if (isDashing)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash(_LastMovement)); // Usar _LastMovement como dirección para el dash
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }

        _Rigidbody.AddForce(_Movement * _Speed);
    }

    private IEnumerator Dash(Vector2 dashDirection)
    {
        canDash = false;
        isDashing = true;
        float originalGravity = _Rigidbody.gravityScale;
        _Rigidbody.gravityScale = 0;
        _Rigidbody.velocity = dashDirection * dashingPower; // Usar dashDirection para la dirección del dash
        yield return new WaitForSeconds(dashingTime);
        _Rigidbody.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}
