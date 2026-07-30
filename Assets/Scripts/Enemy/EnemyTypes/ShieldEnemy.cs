using UnityEngine;

namespace SniperStrategyGame.Enemy
{
    public class ShieldEnemy : BaseEnemy
    {
        [SerializeField] private Collider shieldCollider;

        protected override void ExecuteBehaviour()
        {
            agent.ResetPath();
        }
        public override void OnBulletHit(Collider hitCollider)
        {
            if (hitCollider == shieldCollider)
            {
                Debug.Log("Shield blocked bullet");
                return;
            }

            HandleDeath();
        }
    }
}
