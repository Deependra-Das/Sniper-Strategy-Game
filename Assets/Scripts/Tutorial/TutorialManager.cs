using SniperStrategyGame.Enemy;
using SniperStrategyGame.Event;
using SniperStrategyGame.Main;
using SniperStrategyGame.Path;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SniperStrategyGame.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] private Tutorial_SO _tutorialSequenceSO;
        [SerializeField] private List<Transform> _guardSpawnPointList;
        [SerializeField] private List<Transform> _shieldSpawnPointList;
        [SerializeField] private List<Transform> _patrolSpawnPointList;
        [SerializeField] private List<PatrolPath> _patrolPathList;

        private readonly List<BaseEnemy> _aliveEnemies = new();
        private int _currentTutorialStepIndex = 0;
        private EventBusService _eventBusServiceObj;
        private EnemyService _enemyServiceObj;

        private void Awake()
        {
            var services = GameManager.Instance.Services;

            _eventBusServiceObj = services.Get<EventBusService>();
            _enemyServiceObj = services.Get<EnemyService>();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            _eventBusServiceObj.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            _eventBusServiceObj.Subscribe<PlayerScopeInEvent>(OnScopeIn);
            _eventBusServiceObj.Subscribe<PlayerScopeOutEvent>(OnScopeOut);
            _eventBusServiceObj.Subscribe<PlayerShotEvent>(OnPlayerShot);
        }

        private void UnsubscribeFromEvents()
        {
            _eventBusServiceObj.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            _eventBusServiceObj.Unsubscribe<PlayerScopeInEvent>(OnScopeIn);
            _eventBusServiceObj.Unsubscribe<PlayerScopeOutEvent>(OnScopeOut);
            _eventBusServiceObj.Unsubscribe<PlayerShotEvent>(OnPlayerShot);
        }

        private void Start()
        {
            StartTutorial();
        }

        private void StartTutorial()
        {
            _currentTutorialStepIndex = 0;
            ExecuteCurrentTutorialStep();
        }

        private void ExecuteCurrentTutorialStep()
        {
            TutorialStepData step = GetCurrentTutorialStep();

            if (step == null)
            {
                TutorialCompleted();
                return;
            }

            Debug.Log($"Starting tutorial step: {step.tutorialAction}");

            if (step.requiredEnemyTypeList.Count > 0)
            {
                foreach (EnemyTypeEnum enemyType in step.requiredEnemyTypeList)
                {
                    SpawnEnemyGroup(enemyType);
                }
            }
        }

        private void SpawnEnemyGroup(EnemyTypeEnum enemyType)
        {
            if (!_enemyServiceObj.TryGetEnemyPrefab(enemyType, out BaseEnemy enemyPrefab))
            {
                Debug.LogError($"Missing prefab for {enemyType}");
                return;
            }

            List<Transform> spawnPointList = GetSpawnPointTransformListByEnemyType(enemyType);

            if (spawnPointList.Count <= 0) return;

            for (int j = 0; j < spawnPointList.Count; j++)
            {
                Transform spawnPoint = spawnPointList[j];

                BaseEnemy enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                enemy.Initialize();

                if (enemy is PatrolEnemy patrolEnemy)
                {
                    patrolEnemy.SetPatrolPath(GetPatrolPath(j));
                }

                _aliveEnemies.Add(enemy);
                RaiseEnemySpawnedEvent(enemy);
            }
        }

        private PatrolPath GetPatrolPath(int index)
        {
            if (index >= _patrolPathList.Count)
            {
                Debug.LogWarning("Missing patrol path");
                return null;
            }

            return _patrolPathList[index];
        }

        private List<Transform> GetSpawnPointTransformListByEnemyType(EnemyTypeEnum enemyType)
        {
            List <Transform> spawnPointList = new(); 
            switch (enemyType)
            {
                case EnemyTypeEnum.Guard:
                    spawnPointList = _guardSpawnPointList;
                    break;
                case EnemyTypeEnum.Shield:
                    spawnPointList = _shieldSpawnPointList;
                    break;
                case EnemyTypeEnum.Patrol:
                    spawnPointList = _patrolSpawnPointList;
                    break;
            }

            return spawnPointList;
        }

        private void AdvanceTutorial()
        {
            _currentTutorialStepIndex++;
            ExecuteCurrentTutorialStep();
        }

        private void TutorialCompleted()
        {
            Debug.Log("Tutorial Completed");
        }

        private void OnScopeIn(PlayerScopeInEvent eventObj)
        {
            if (!IsCurrentAction(TutorialActionEnum.ScopeIn))
                return;

            AdvanceTutorial();
        }

        private void OnScopeOut(PlayerScopeOutEvent eventObj)
        {
            if (!IsCurrentAction(TutorialActionEnum.ScopeOut))
                return;

            AdvanceTutorial();
        }

        private void OnPlayerShot(PlayerShotEvent eventObj)
        {
            if (!IsCurrentAction(TutorialActionEnum.Shoot))
                return;

            AdvanceTutorial();
        }

        private void OnEnemyDied(EnemyDiedEvent eventObj)
        {
            if (!_aliveEnemies.Remove(eventObj.Enemy))
                return;

            Destroy(eventObj.Enemy.gameObject);

            TutorialStepData currentStep = GetCurrentTutorialStep();

            if (currentStep == null)
                return;

            if (!TryGetRequiredEnemyType(currentStep.tutorialAction, out EnemyTypeEnum requiredEnemyType))
            {
                return;
            }

            if (eventObj.Enemy.EnemyType != requiredEnemyType)
                return;

            AdvanceTutorial();
        }

        private TutorialStepData GetCurrentTutorialStep()
        {
            if (_currentTutorialStepIndex >=
                _tutorialSequenceSO.tutorialStepsList.Count)
            {
                return null;
            }

            return _tutorialSequenceSO
                .tutorialStepsList[_currentTutorialStepIndex];
        }

        private bool TryGetRequiredEnemyType( TutorialActionEnum action, out EnemyTypeEnum enemyType)
        {
            switch (action)
            {
                case TutorialActionEnum.GuardEnemy:
                    enemyType = EnemyTypeEnum.Guard;
                    return true;

                case TutorialActionEnum.PatrolEnemy:
                    enemyType = EnemyTypeEnum.Patrol;
                    return true;

                case TutorialActionEnum.ShieldEnemy:
                    enemyType = EnemyTypeEnum.Shield;
                    return true;

                default:
                    enemyType = default;
                    return false;
            }
        }

        private bool IsCurrentAction(TutorialActionEnum action)
        {
            if (_currentTutorialStepIndex >= _tutorialSequenceSO.tutorialStepsList.Count)
                return false;

            TutorialStepData currentStep = _tutorialSequenceSO.tutorialStepsList[_currentTutorialStepIndex];

            return currentStep.tutorialAction == action;
        }

        private void RaiseEnemySpawnedEvent(BaseEnemy enemy)
        {
            _eventBusServiceObj.Publish(new EnemySpawnedEvent(enemy));
        }
    }
}