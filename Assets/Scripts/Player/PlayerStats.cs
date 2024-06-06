using UnityEngine;

namespace Player
{
    public class PlayerStats : MonoBehaviour
    {
        [SerializeField] private float maxHealth;
        private float currentHealth;

        //[SerializeField] private float dmg;

        [SerializeField] private HealthBar healthBar;

        private void Start()
        {
            currentHealth = maxHealth;
            healthBar?.SetMaxHealth(maxHealth);
        }

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            healthBar?.SetHealth(currentHealth);
        }
    }
}
