using EnemyUnits;
using UnityEngine;

namespace Input_System
{
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private int attackDamage = 10;
        [SerializeField] private LayerMask enemyLayer;

        public void Attack()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit, Mathf.Infinity, enemyLayer)) return;
            if (!(Vector3.Distance(transform.position, hit.transform.position) <= attackRange)) return;
        
            EnemyBehaviour enemy = hit.transform.GetComponent<EnemyBehaviour>();
        
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
