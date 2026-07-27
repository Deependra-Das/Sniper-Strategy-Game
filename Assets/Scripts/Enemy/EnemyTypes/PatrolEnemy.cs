using UnityEngine;

public class PatrolEnemy : BaseEnemy
{
    [SerializeField] private Path path;
    [SerializeField] private float waitTimeOnWayPoint = 2f;
    private float timer;

    protected override void Start()
    {
        base.Start();

        if (path != null)
            agent.destination = path.GetNextWayPoint();
    }

    protected override void ExecuteBehaviour()
    {
        if (agent.remainingDistance <= 0.1f)
        {
            timer += behaviourLoopInterval;

            if (timer >= waitTimeOnWayPoint)
            {
                timer = 0f;
                agent.destination = path.GetNextWayPoint();
            }
        }
        else
        {
            timer = 0f;
        }

        float normalizedSpeed = Mathf.InverseLerp( 0f, agent.speed, agent.velocity.magnitude);
        animator.SetFloat("Speed", normalizedSpeed);
    }
}
