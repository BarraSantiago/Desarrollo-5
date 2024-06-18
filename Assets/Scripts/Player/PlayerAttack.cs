using System.Collections.Generic;
using UnityEngine;

namespace player
{
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private Weapon weapon;
        public List<AttackSO> combo;
        public float comboCooldownTime = 0.5f;
        public float attackInterval = 0.2f;
        public float exitAttackDelay = 1f;

        private float lastClickedTime;
        private float lastComboEnd;
        private int comboCounter;
        private Animator animator;

        private void Start()
        {
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            ExitAttack();
        }

        public void Attack()
        {
            if (!(Time.time - lastComboEnd > comboCooldownTime) || comboCounter >= combo.Count) return;
            CancelInvoke(nameof(EndCombo));

            if (Time.time - lastClickedTime >= attackInterval)
            {
                PlayAttackAnimation(comboCounter);
                weapon.damage = combo[comboCounter].damage;
                comboCounter++;
                lastClickedTime = Time.time;

                if (comboCounter > combo.Count)
                {
                    comboCounter = 0;
                }

                weapon.EnableTriggerBox();
            }
        }

        private void PlayAttackAnimation(int index)
        {
            animator.runtimeAnimatorController = combo[index].animatorOV;
            animator.Play("Attack", 0, 0);
        }

        void ExitAttack()
        {
            if (IsExitingAttack())
            {
                Invoke(nameof(EndCombo), exitAttackDelay);
            }
        }

        bool IsExitingAttack()
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.normalizedTime > 0.9f && stateInfo.IsTag("Attack");
        }

        void EndCombo()
        {
            comboCounter = 0;
            lastComboEnd = Time.time;

            weapon.DisableTriggerBox();
        }
    }
}
