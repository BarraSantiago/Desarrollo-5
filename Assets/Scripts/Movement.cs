using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Movement : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private int speed = 5;
    [FormerlySerializedAs("range")] [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private int damage = 5;
    [SerializeField] private Transform attackHitbox;
    [SerializeField] private LayerMask enemyLayers;

    private static readonly int AttackTrigger = Animator.StringToHash("Attack");

    private void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            var vector3 = gameObject.transform.position;
            vector3.x += speed * Time.deltaTime;
            gameObject.transform.position = vector3;
        }

        if (Input.GetKey(KeyCode.A))
        {
            var vector3 = gameObject.transform.position;
            vector3.x -= speed * Time.deltaTime;
            gameObject.transform.position = vector3;
        }

        if (Input.GetKey(KeyCode.W))
        {
            var vector3 = gameObject.transform.position;
            vector3.y += speed * Time.deltaTime;
            gameObject.transform.position = vector3;
        }

        if (Input.GetKey(KeyCode.S))
        {
            var vector3 = gameObject.transform.position;
            vector3.y -= speed * Time.deltaTime;
            gameObject.transform.position = vector3;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Attack();
        }
    }

    private void Attack()
    {
        _animator.SetTrigger(AttackTrigger);

        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(attackHitbox.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in enemiesHit)
        {
            enemy.gameObject.GetComponent<Enemy>().ModifyHealth(-damage);
            Debug.Log("Enemy Hit");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attackHitbox.position, attackRange);
    }
}