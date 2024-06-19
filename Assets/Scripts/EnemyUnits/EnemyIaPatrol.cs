using Input_System;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyUnits
{
    public class EnemyIaPatrol : MonoBehaviour
    {
        //Layer
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask playerLayer;

        //Patrol
        [SerializeField] float range;
        private Vector3 destPoint;
        private bool walkPointSet;


        //State Change
        [SerializeField] private float sightRange;
        [SerializeField] private float attackRange;
        private bool playerInSight;
        private bool playerInAttackRange;

        private GameObject player;
        private NavMeshAgent agent;
        private Animator animator;

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            player = GameObject.Find("Player");
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            playerInSight = Physics.CheckSphere(transform.position, sightRange, playerLayer);
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);

            if (!playerInSight && !playerInAttackRange) Patrol();
            if (playerInSight && !playerInAttackRange) Chase();
            if (playerInSight && playerInAttackRange) Attack();
        }

        private void Chase()
        {
            agent.SetDestination(player.transform.position);
        }

        private void Attack()
        {
            if(!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                animator.SetTrigger("attack");
                agent.SetDestination(transform.position);
            }
        }

        private void Patrol()
        {
            if (!walkPointSet)
                SearchForDest();

            if (walkPointSet)
                agent.SetDestination(destPoint);

            if (Vector3.Distance(transform.position, destPoint) < 10)
                walkPointSet = false;
        }

        private void SearchForDest()
        {
            float z = Random.Range(-range, range);
            float x = Random.Range(-range, range);

            destPoint = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);

            if (Physics.Raycast(destPoint, Vector3.down, groundLayer))
                walkPointSet = true;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}