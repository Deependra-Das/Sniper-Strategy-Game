using UnityEngine;
using SniperStrategyGame.Path;

namespace SniperStrategyGame.Enemy
{
    public class PatrolEnemy : BaseEnemy
    {
        [SerializeField] private float waitTimeOnWayPoint = 2f;
        private PatrolPath _patrolPath;
        private float _timer;

        public void SetPatrolPath(PatrolPath path)
        {
            _patrolPath = path;
        }

        protected override void ActivateEnemy()
        {
            base.ActivateEnemy();

            if (_patrolPath != null)
                agent.destination = _patrolPath.GetNextWayPoint();
        }

        protected override void ExecuteBehaviour()
        {
            if (agent.remainingDistance <= 0.1f)
            {
                _timer += behaviourLoopInterval;

                if (_timer >= waitTimeOnWayPoint)
                {
                    _timer = 0f;
                    agent.destination = _patrolPath.GetNextWayPoint();
                }
            }
            else
            {
                _timer = 0f;
            }

            float normalizedSpeed = Mathf.InverseLerp(0f, agent.speed, agent.velocity.magnitude);
            animator.SetFloat("Speed", normalizedSpeed);
        }
    }
}
