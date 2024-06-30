using System.Collections.Generic;
using UnityEngine;

namespace player
{
    public class PlayerAttack : MonoBehaviour
    {
        private Animator animator;                                  // Reference to the Animator component
        private Weapon weapon;                                      // Reference to the player s weapon
        private PlayerController playerController;

        // Attack Settings
        [SerializeField] private List<AttackSO> combo;              // List of combo attacks
        [SerializeField] private float comboCooldownTime = 0.5f;    // Time to wait before starting a new combo
        [SerializeField] private float attackInterval = 0.2f;       // Minimum time between attacks
        [SerializeField] private float exitAttackDelay = 1f;        // Delay before exiting attack state

        // Internal state
        private float lastClickedTime;                              // Last time the attack button was clicked
        private float lastComboEnd;                                 // Last time the combo ended
        private int comboCounter;                                   // Counter for current combo attack

        private void Start()
        {
            animator = GetComponent<Animator>();
            weapon = GetComponentInChildren<Weapon>();
            playerController = GetComponent<PlayerController>();
        }

        private void Update()
        {
            EnableMovementDuringCombo();
            ExitAttack();
        }

        // Method to handle player attack input
        public void Attack()
        {
            if (!(Time.time - lastComboEnd > comboCooldownTime) || comboCounter >= combo.Count) return;
            CancelInvoke(nameof(ResetCombo));

            if (Time.time - lastClickedTime >= attackInterval)
            {
                playerController.DisableMovement();
                PlayAttackAnimation(comboCounter);
                weapon.Damage *= combo[comboCounter].damage;
                comboCounter++;
                Debug.Log("Combo counter" + comboCounter);
                lastClickedTime = Time.time;

                if (comboCounter > combo.Count)
                {
                    Debug.Log("combo count" + combo.Count);
                    comboCounter = 0;
                }

                weapon.EnableTriggerBox();
            }
        }

        // Play the attack animation based on the current combo index
        private void PlayAttackAnimation(int index)
        {
            animator.runtimeAnimatorController = combo[index].animatorOV;
            animator.Play("Attack", 0, 0);
        }

        // Check if the player is exiting the attack state
        private void ExitAttack()
        {
            if (IsExitingAttack())
            {
                Invoke(nameof(ResetCombo), exitAttackDelay);
            }
        }

        // Determine if the current animation state is exiting an attack
        private bool IsExitingAttack()
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.normalizedTime > 0.9f && stateInfo.IsTag("Attack");
        }

        // Enable movement during combo attack
        private void EnableMovementDuringCombo()
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime > 0.9f && stateInfo.IsTag("Attack"))
            {
                playerController.EnableMovement();
            }
        }

        // Reset combo counter and disable the weapon s trigger box
        private void ResetCombo()
        {
            comboCounter = 0;
            lastComboEnd = Time.time;
            weapon.ResetDamage();

            weapon.DisableTriggerBox();
            playerController.EnableMovement();
        }
    }
}
