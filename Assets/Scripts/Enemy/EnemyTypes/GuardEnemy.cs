using UnityEngine;

namespace SniperStrategyGame.Enemy
{
    public class GuardEnemy : BaseEnemy
    {
        protected override void ExecuteBehaviour()
        {
            agent.ResetPath();
        }
    }
}
