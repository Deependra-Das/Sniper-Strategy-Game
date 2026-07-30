using SniperStrategyGame.Enemy;
using SniperStrategyGame.Event;
using SniperStrategyGame.Main;
using SniperStrategyGame.Player;
using UnityEngine;

namespace SniperStrategyGame.Bullet
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerBullet : MonoBehaviour
    {
        [SerializeField] private float _lifeTime = 100f;
        [SerializeField] private Rigidbody _rigidbodyObj;
        private Vector3 _shotDirection;
        private EventBusService _eventBusServiceObj;

        private void Awake()
        {
            _eventBusServiceObj = GameManager.Instance.Services.Get<EventBusService>();
        }

        public void Initialize(Vector3 direction, float speed)
        {
            _rigidbodyObj.linearVelocity = direction.normalized * speed;
            _shotDirection = direction.normalized;
            Destroy(gameObject, _lifeTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            BaseEnemy enemy = collision.collider.GetComponentInParent<BaseEnemy>();

            if (enemy != null)
            {
                Vector3 enemyosition = enemy.transform.position;
                bool hitEnemy = enemy.OnBulletHit(collision.collider);

                if (hitEnemy)
                {
                    _eventBusServiceObj.Publish(new PlayerBulletHitEnemyEvent(enemyosition, _shotDirection));
                }
                else
                {
                    _eventBusServiceObj.Publish(new PlayerBulletMissedEnemyEvent());
                }
            }
            else
            {
                _eventBusServiceObj.Publish(new PlayerBulletMissedEnemyEvent());
            }
            Destroy(gameObject);
        }
    }
}