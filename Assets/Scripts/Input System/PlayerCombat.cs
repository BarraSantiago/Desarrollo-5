using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] Camera _Camera;
    Vector2 _MousePos;
    PlayerInput _Input;
    Rigidbody2D _Rigidbody;

    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    public int attackDamage = 30;
    public float attackRate = 2f;
    float nextAttackTime = 0f;

    float angle = 0;
    Vector2 facingDirection;

    private void Awake()
    {
        _Input = new PlayerInput();
        _Rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if(Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _Rigidbody.MoveRotation(angle);
                Attack();
                nextAttackTime = Time.time + .1f / attackRate;
            }
        }
    }

    private void FixedUpdate()
    {
        facingDirection = _MousePos - _Rigidbody.position;
        angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg - 90;
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

    private void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach(Collider2D enemy in hitEnemies) 
        {
            enemy.GetComponent<Enemies>().TakeDamage(attackDamage);
        }
    }

    private void OnDrawGizmos()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
