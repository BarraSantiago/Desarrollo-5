using player;
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

        private HealthBar _healthBar;
        private float _currentHealth;
        private bool hasAttacked = false;

        private BoxCollider boxCollider;

        private void Start()
        {
            _currentHealth = maxHealth;
            if (!playerStats)
            {
                playerStats = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
            }
            boxCollider = GetComponentInChildren<BoxCollider>();
            _healthBar = GetComponentInChildren<HealthBar>();

            _currentHealth = maxHealth;
            _healthBar?.SetMaxHealth(maxHealth);
        }

        private void DoRangedAction()
        {
            playerStats.ReceiveDamage(damage);
        }

        private void DoMeleeAction()
        {
            if (!hasAttacked)
            {
                playerStats.ReceiveDamage(damage);
                hasAttacked = true;
                AudioManager.instance.Play("EnemyHit");
            }
        }

        public void RecibeDamageFromPlayer(float damage)
        {
            _currentHealth -= damage;
            _healthBar?.SetHealth(_currentHealth);

            if (_currentHealth > 0) return;

            _currentHealth = 0;
            Die();
        }

        void EnableAttack()
        {
            boxCollider.enabled = true;
        }

        void DisableAttack()
        {
            boxCollider.enabled = false;
            hasAttacked = false;
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

            GameObject drop = Instantiate(itemDatabase.ItemObjects[rand].characterDisplay, transform.position, Quaternion.identity);
            drop.transform.position = new Vector3(drop.transform.position.x, 1f, drop.transform.position.z);
            Destroy(gameObject);
        }
    }
}