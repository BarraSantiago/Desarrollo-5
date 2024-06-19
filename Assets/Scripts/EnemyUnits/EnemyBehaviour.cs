using Input_System;
using InventorySystem;
using UnityEngine;
using PlayerStats = player.PlayerStats;
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

        private BoxCollider boxCollider;

        private void Start()
        {
            currentHealth = maxHealth;
            if (!playerStats)
            {
                playerStats = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
            }
            boxCollider = GetComponentInChildren<BoxCollider>();
        }

        private void DoRangedAction()
        {
            playerStats.RecibeDamageFromEnemy(damage);
        }

        private void DoMeleeAction()
        {   
            playerStats.RecibeDamageFromEnemy(damage);
        }

        public void RecibeDamageFromPlayer(float damage)
        {
            currentHealth -= damage;

            if (currentHealth > 0) return;

            currentHealth = 0;
            Die();
        }

        void EnableAttack()
        {
            boxCollider.enabled = true;
        }

        void DisableAttack()
        {
            boxCollider.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerController>();

            if (player != null)
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