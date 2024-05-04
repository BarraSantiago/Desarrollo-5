using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemies : MonoBehaviour
{
    public int maxHealth = 100;
    int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log("Enemy Died");

        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        Destroy(gameObject);
    }
}
