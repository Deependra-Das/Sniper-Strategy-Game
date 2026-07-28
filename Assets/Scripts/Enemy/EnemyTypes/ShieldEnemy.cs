using UnityEngine;

namespace SniperStrategyGame.Enemy
{
    public class ShieldEnemy : BaseEnemy
    {
        protected override void ExecuteBehaviour()
        {
            agent.ResetPath();
        }
    }
}
