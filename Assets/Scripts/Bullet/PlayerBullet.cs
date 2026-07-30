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
        private PlayerController controller;
        private EventBusService _eventBusServiceObj;

        private void Awake()
        {
            _eventBusServiceObj = GameManager.Instance.Services.Get<EventBusService>();
        }

        public void Initialize(Vector3 direction, float speed)
        {
            _rigidbodyObj.linearVelocity = direction.normalized * speed;

            Destroy(gameObject, _lifeTime);
        }

        public void SetController(PlayerController player)
        {
            controller = player;
        }

        private void OnCollisionEnter(Collision collision)
        {
            BaseEnemy enemy = collision.collider.GetComponentInParent<BaseEnemy>();

            if (enemy != null)
            {
                enemy.OnBulletHit(collision.collider);
            }

            _eventBusServiceObj.Publish(new PlayerBulletImpactEvent());
            controller.RestorePlayerCamera();
            Destroy(gameObject);
        }
    }
}