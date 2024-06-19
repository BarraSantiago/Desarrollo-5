using UnityEngine;

namespace player
{
    public class PlayerStats : MonoBehaviour
    {
        [SerializeField] private float maxHealth;
        private float currentHealth;

        [SerializeField] private HealthBar healthBar;

        private void Start()
        {
            currentHealth = maxHealth;
            healthBar?.SetMaxHealth(maxHealth);
        }

        public void RecibeDamageFromEnemy(float damage)
        {
            currentHealth -= damage;
            healthBar?.SetHealth(currentHealth);
        }
    }
}
