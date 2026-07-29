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
        protected Animator animator;
        protected NavMeshAgent agent;
        private Coroutine _behaviourLoopCoroutine;
        private bool _isActive;
        protected GameplayManager gameplayManagerObj;
        private EventBusService _eventBusServiceObj;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
        }

        public void Initialize(GameplayManager gameplayManagerObj)
        {
            this.gameplayManagerObj = gameplayManagerObj;
            _eventBusServiceObj = GameManager.Instance.Services.Get<EventBusService>();
            _eventBusServiceObj.Subscribe<ActivateEnemiesEvent>(OnActivateEnemies);

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

        protected abstract void ExecuteBehaviour();

        protected virtual void OnDestroy()
        {
            if (_behaviourLoopCoroutine != null)
            {
                StopCoroutine(_behaviourLoopCoroutine);
            }
        }
    }
}
