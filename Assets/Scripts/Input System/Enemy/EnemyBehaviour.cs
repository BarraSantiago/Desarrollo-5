using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public enum EnemyType { Melee, Ranged };

    [SerializeField] private EnemyType enemyType;

    [SerializeField] private float maxHealth = 50;
    private float currentHealth;

    [SerializeField] private float damage;

    [SerializeField] private PlayerStats playerStats;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public EnemyType PerformAction()
    {
        switch (enemyType)
        {
            case EnemyType.Melee:
                DoMeleeAction();
                break;
            case EnemyType.Ranged:
                DoRangedAction();
                break;
            default:
                break;
        }

        return enemyType;
    }

    private void DoRangedAction()
    {
        Debug.Log("Ataque Range");
    }

    private void DoMeleeAction()
    {
        playerStats.TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("Enemy health " + currentHealth);
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{enemyType} enemy died.");
        Destroy(gameObject);
    }
}
