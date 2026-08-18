using SniperStrategyGame.Event;
using SniperStrategyGame.Gameplay;
using SniperStrategyGame.Main;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace SniperStrategyGame.Enemy
{
    public abstract class BaseEnemy : MonoBehaviour
    {
        [SerializeField] protected float behaviourLoopInterval = 0.1f;
        [SerializeField] protected EnemyTypeEnum enemyType;

        public EnemyTypeEnum EnemyType => enemyType;
        protected Animator animator;
        protected NavMeshAgent agent;
        private Coroutine _behaviourLoopCoroutine;
        private bool _isActive;
        protected EventBusService eventBusServiceObj;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
        }

        protected virtual void SubscribeToEvents()
        {
            eventBusServiceObj.Subscribe<ActivateEnemiesEvent>(OnActivateEnemies);
            eventBusServiceObj.Subscribe<PlayerBulletFiredEvent>(OnPlayerBulletFired);
            eventBusServiceObj.Subscribe<PlayerBulletHitEnemyEvent>(OnPlayerBulletImpact);
        }

        protected virtual void UnsubscribeToEvents()
        {
            eventBusServiceObj.Unsubscribe<ActivateEnemiesEvent>(OnActivateEnemies);
            eventBusServiceObj.Unsubscribe<PlayerBulletFiredEvent>(OnPlayerBulletFired);
            eventBusServiceObj.Unsubscribe<PlayerBulletHitEnemyEvent>(OnPlayerBulletImpact);
        }

        public void Initialize()
        {
            eventBusServiceObj = GameManager.Instance.Services.Get<EventBusService>();
            SubscribeToEvents();
            SetActive(false);
        }

        private void OnActivateEnemies(ActivateEnemiesEvent eventObj)
        {
            ActivateEnemy();
        }

        protected virtual void ActivateEnemy()
        {
            SetActive(true);

            if (_behaviourLoopCoroutine == null)
            {
                _behaviourLoopCoroutine = StartCoroutine(BehaviourLoop());
            }
        }

        private IEnumerator BehaviourLoop()
        {
            while (true)
            {
                if (_isActive)
                    ExecuteBehaviour();

                yield return new WaitForSeconds(behaviourLoopInterval);
            }
        }

        private void SetActive(bool isActive)
        {
            _isActive = isActive;

            agent.isStopped = !isActive;
            animator.speed = isActive ? 1f : 0f;
        }

        private void OnPlayerBulletFired(PlayerBulletFiredEvent eventObj)
        {
            Freeze();
        }

        private void OnPlayerBulletImpact(PlayerBulletHitEnemyEvent eventObj)
        {
            UnFreeze();
        }

        private void Freeze()
        {
            agent.isStopped = true;
            animator.speed = 0f;
        }

        private void UnFreeze()
        {
            agent.isStopped = false;
            animator.speed = 1f;
        }

        protected abstract void ExecuteBehaviour();

        public virtual bool OnBulletHit(Collider hitCollider)
        {
            HandleDeath();
            return true;
        }

        protected virtual void HandleDeath()
        {
            Cleanup();
            eventBusServiceObj.Publish(new EnemyDiedEvent(this));
        }

        protected virtual void Cleanup()
        {
            if (_behaviourLoopCoroutine != null)
            {
                StopCoroutine(_behaviourLoopCoroutine);
                _behaviourLoopCoroutine = null;
            }

            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }

            if (animator != null)
            {
                animator.enabled = false;
            }

            if (eventBusServiceObj != null)
            {
                UnsubscribeToEvents();
            }
        }

        protected virtual void OnDestroy()
        {
            Cleanup();
        }
    }
}
