using UnityEngine;
using UnityEngine.AI;

public class PathfindMovement : MonoBehaviour
{
    [SerializeField] private Transform target;
    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Update()
    {
        agent.SetDestination(target.position);
    }
}