using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class BaseEnemy : MonoBehaviour
{
    [SerializeField] Path path;
    [SerializeField] float waitTimeOnWayPoint = 1f;
    NavMeshAgent agent;

    private float timer = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        agent.destination = path.GetCurrentWayPoint();
    }

    private void Update()
    {
        if(agent.remainingDistance <= 0.1f)
        {
            timer += Time.deltaTime;
            if(timer >= waitTimeOnWayPoint)
            {
                timer = 0f;
                agent.destination = path.GetNextWayPoint();
            }
        }
    }
}
