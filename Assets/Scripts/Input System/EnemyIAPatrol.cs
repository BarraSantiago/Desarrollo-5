using Input_System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyIAPatrol : MonoBehaviour
{
    GameObject player;

    NavMeshAgent agent;

    [SerializeField] private LayerMask groundLayer, playerLayer;

    BoxCollider boxCollider;

    //Patrol
    Vector3 destPoint;
    bool walkPointSet;
    [SerializeField] float range;

    //State Change
    [SerializeField] private float sightRange, attackRange;
    bool playerInSight, playerInAttackRange;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player");
        boxCollider = player.GetComponentInChildren<BoxCollider>();
    }

    private void Update()
    {
        playerInSight = Physics.CheckSphere(transform.position, sightRange, playerLayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);

        if (!playerInSight && !playerInAttackRange) Patrol();
        if (playerInSight && !playerInAttackRange) Chase();
        if (playerInSight && playerInAttackRange) Attack();
    }

    void Chase()
    {
        agent.SetDestination(player.transform.position);
    }

    void Attack()
    {
        agent.SetDestination(transform.position);
    }

    void Patrol()
    {
        if (!walkPointSet)
            SearchForDest();

        if (walkPointSet)
            agent.SetDestination(destPoint);

        if (Vector3.Distance(transform.position, destPoint) < 10)
            walkPointSet = false;
    }

    void SearchForDest()
    {
        float z = Random.Range(-range, range);
        float x = Random.Range(-range, range);

        destPoint = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);

        if (Physics.Raycast(destPoint, Vector3.down, groundLayer))
            walkPointSet = true;
    }

    //void EnableAttack()
    //{
    //    boxCollider.enabled = true;
    //}

    //void DisableAttack()
    //{
    //    boxCollider.enabled = false;
    //}

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerController>();

        if(player != null)
        {
            print("HIT!");
        }
    }
}