using UnityEngine;

namespace SniperStrategyGame.Enemy
{
    public class PatrolEnemy : BaseEnemy
    {
        [SerializeField] private float waitTimeOnWayPoint = 2f;
        private Path _path;
        private float _timer;

        public void SetPatrolPath(Path path)
        {
            _path = path;
        }

        protected override void ActivateEnemy()
        {
            base.ActivateEnemy();

            if (_path != null)
                agent.destination = _path.GetNextWayPoint();
        }

        protected override void ExecuteBehaviour()
        {
            if (agent.remainingDistance <= 0.1f)
            {
                _timer += behaviourLoopInterval;

                if (_timer >= waitTimeOnWayPoint)
                {
                    _timer = 0f;
                    agent.destination = _path.GetNextWayPoint();
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
