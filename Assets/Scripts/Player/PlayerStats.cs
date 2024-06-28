using System;
using System.Collections;
using InventorySystem;
using UnityEngine;

namespace player
{
    public class PlayerStats : MonoBehaviour
    {
        [SerializeField] private float maxHealth;
        [SerializeField] private HealthBar healthBar;
        [SerializeField] private PlayerController playerController;
        public static Action<ItemBuff> OnBuffReceived;
        private float _currentHealth;
        

        private void Start()
        {
            _currentHealth = maxHealth;
            healthBar?.SetMaxHealth(maxHealth);
            OnBuffReceived += ReceiveBuff;
        }

        private void OnDestroy()
        {
            OnBuffReceived -= ReceiveBuff;
        }

        private void ReceiveBuff(ItemBuff buff)
        {
            switch (buff.stat)
            {
                case Attributes.Health:
                    Heal(buff.value);
                    break;
                case Attributes.Agility:
                    StartCoroutine(IncreaseAgility(buff.value, buff.Duration));
                    break;
                case Attributes.Strength:
                    break;
                case Attributes.Stamina:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void ReceiveDamage(float damage)
        {
            _currentHealth -= damage;
            healthBar?.SetHealth(_currentHealth);
        }

        private void Heal(float healAmount)
        {
            _currentHealth += healAmount;
            if (_currentHealth > maxHealth)
            {
                _currentHealth = maxHealth;
            }
            healthBar?.SetHealth(_currentHealth);
        }
        
        private IEnumerator IncreaseAgility(float value, float duration)
        {
            playerController.MovementSpeed += value;
            yield return new WaitForSeconds(duration);
            playerController.MovementSpeed -= value;
        }
    }
}
