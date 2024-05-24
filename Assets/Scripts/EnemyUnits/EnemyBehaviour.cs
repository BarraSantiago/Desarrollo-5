using InventorySystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EnemyUnits
{
    public class EnemyBehaviour : MonoBehaviour
    {
        public enum EnemyType
        {
            Melee,
            Ranged
        };

        [SerializeField] private ItemDatabaseObject itemDatabase;
        [SerializeField] private EnemyType enemyType;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private float damage;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float maxHealth = 50;

        private float currentHealth;
        private float lastDamageTime;

        private void Start()
        {
            currentHealth = maxHealth;
            lastDamageTime = Time.time;
        }

        public void PerformAction()
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
        }

        private void DoRangedAction()
        {
            Debug.Log("Ataque Range");
        }

        private void DoMeleeAction()
        {
            if (Time.time - lastDamageTime < attackCooldown) return;
            
            
            lastDamageTime = Time.time;
            
            playerStats.TakeDamage(damage);
        }

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;

            if (currentHealth > 0) return;

            currentHealth = 0;
            Die();
        }

        private void Die()
        {
            Debug.Log($"{enemyType} enemy died.");
            int rand = Random.Range(0, itemDatabase.ItemObjects.Length);

            Instantiate(itemDatabase.ItemObjects[rand].characterDisplay, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}