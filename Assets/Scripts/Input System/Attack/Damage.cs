using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour
{
    public float attackRange = 2f; // Rango de ataque
    public int attackDamage = 10; // Daño de ataque
    public LayerMask enemyLayer; // Capa para identificar enemigos

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Detecta clic izquierdo
        {
            Attack();
        }
    }

    void Attack()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange, enemyLayer))
        {
            Health enemy = hit.transform.GetComponent<Health>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
                Debug.Log("Hit Enemy!!");
            }
        }
    }
}
