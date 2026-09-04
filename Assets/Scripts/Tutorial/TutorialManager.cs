using UnityEngine;
using System.Collections.Generic;
using SniperStrategyGame.Enemy;
using SniperStrategyGame.Event;
using SniperStrategyGame.Main;
using SniperStrategyGame.Path;

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
        private readonly List<BaseEnemy> _currentStepEnemies = new();

        private int _currentTutorialGroupIndex;
        private int _currentTutorialStepIndex;

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
            _currentTutorialGroupIndex = 0;
            _currentTutorialStepIndex = 0;
            _aliveEnemies.Clear();
            _currentStepEnemies.Clear();

            TutorialGroupData currentGroup = GetCurrentTutorialGroup();

            if (currentGroup != null)
            {
                RaiseUpdateMissionInfoEvent(currentGroup);
                ExecuteCurrentTutorialStep();
            }
        }

        private void ExecuteCurrentTutorialStep()
        {
            TutorialGroupData currentGroup = GetCurrentTutorialGroup();

            if (currentGroup == null)
            {
                TutorialCompleted();
                return;
            }

            TutorialStepData currentStep = GetCurrentTutorialStep();

            if (currentStep == null)
            {
                AdvanceToNextTutorialGroup();
                return;
            }

            Debug.Log($"Starting Tutorial Group: {currentGroup.tutorialGroupName} | Step: {_currentTutorialStepIndex} | Action: {currentStep.tutorialAction}");

            _currentStepEnemies.Clear();

            if (currentStep.requiredEnemyTypeList.Count > 0)
            {
                foreach (EnemyTypeEnum enemyType in currentStep.requiredEnemyTypeList)
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

            if (spawnPointList == null || spawnPointList.Count <= 0)
            {
                Debug.LogWarning($"No spawn points configured for {enemyType}");
                return;
            }

            for (int j = 0; j < spawnPointList.Count; j++)
            {
                Transform spawnPoint = spawnPointList[j];

                if (spawnPoint == null) continue;

                BaseEnemy enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

                enemy.Initialize();

                if (enemy is PatrolEnemy patrolEnemy)
                {
                    patrolEnemy.SetPatrolPath(GetPatrolPath(j));
                }

                _aliveEnemies.Add(enemy);
                _currentStepEnemies.Add(enemy);

                RaiseEnemySpawnedEvent(enemy);
            }
        }

        private PatrolPath GetPatrolPath(int index)
        {
            if (index < 0 || index >= _patrolPathList.Count)
            {
                Debug.LogWarning($"Missing patrol path for index {index}");
                return null;
            }

            return _patrolPathList[index];
        }

        private List<Transform> GetSpawnPointTransformListByEnemyType(EnemyTypeEnum enemyType)
        {
            return enemyType switch
            {
                EnemyTypeEnum.Guard => _guardSpawnPointList,
                EnemyTypeEnum.Shield => _shieldSpawnPointList,
                EnemyTypeEnum.Patrol => _patrolSpawnPointList,

                _ => null
            };
        }

        private void AdvanceTutorial()
        {
            _currentTutorialStepIndex++;

            TutorialGroupData currentGroup = GetCurrentTutorialGroup();

            if (currentGroup != null && _currentTutorialStepIndex < currentGroup.tutorialStepsList.Count)
            {
                ExecuteCurrentTutorialStep();
                return;
            }

            AdvanceToNextTutorialGroup();
        }

        private void AdvanceToNextTutorialGroup()
        {
            _currentTutorialGroupIndex++;
            _currentTutorialStepIndex = 0;

            _currentStepEnemies.Clear();

            TutorialGroupData nextGroup = GetCurrentTutorialGroup();

            if (nextGroup == null)
            {
                TutorialCompleted();
                return;
            }

            Debug.Log($"Tutorial Group Completed. Starting: {nextGroup.tutorialGroupName}");

            RaiseUpdateMissionInfoEvent(nextGroup);

            ExecuteCurrentTutorialStep();
        }

        private void TutorialCompleted()
        {
            Debug.Log("Tutorial Completed");
        }

        private void OnScopeIn(PlayerScopeInEvent eventObj)
        {
            if (!IsCurrentAction(TutorialActionEnum.ScopeIn)) return;

            AdvanceTutorial();
        }

        private void OnScopeOut(PlayerScopeOutEvent eventObj)
        {
            if (!IsCurrentAction(TutorialActionEnum.ScopeOut)) return;

            AdvanceTutorial();
        }

        private void OnPlayerShot(PlayerShotEvent eventObj)
        {
            if (!IsCurrentAction(TutorialActionEnum.Shoot)) return;

            AdvanceTutorial();
        }

        private void OnEnemyDied(EnemyDiedEvent eventObj)
        {
            if (eventObj.Enemy == null) return;

            if (!_aliveEnemies.Remove(eventObj.Enemy)) return;

            _currentStepEnemies.Remove(eventObj.Enemy);

            Destroy(eventObj.Enemy.gameObject);

            TutorialStepData currentStep = GetCurrentTutorialStep();

            if (currentStep == null) return;

            if (!TryGetRequiredEnemyType(currentStep.tutorialAction, out EnemyTypeEnum requiredEnemyType))
            {
                return;
            }

            if (eventObj.Enemy.EnemyType != requiredEnemyType) return;

            if (_currentStepEnemies.Count > 0) return;

            AdvanceTutorial();
        }

        private TutorialGroupData GetCurrentTutorialGroup()
        {
            if (_tutorialSequenceSO == null)
            {
                Debug.LogError("Tutorial SO is not assigned.");
                return null;
            }

            if (_currentTutorialGroupIndex < 0 || _currentTutorialGroupIndex >= _tutorialSequenceSO.tutorialGroupsList.Count)
            {
                return null;
            }

            return _tutorialSequenceSO.tutorialGroupsList[_currentTutorialGroupIndex];
        }

        private TutorialStepData GetCurrentTutorialStep()
        {
            TutorialGroupData currentGroup = GetCurrentTutorialGroup();

            if (currentGroup == null) return null;

            if (_currentTutorialStepIndex < 0 || _currentTutorialStepIndex >= currentGroup.tutorialStepsList.Count)
            {
                return null;
            }

            return currentGroup.tutorialStepsList[_currentTutorialStepIndex];
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
            TutorialStepData currentStep = GetCurrentTutorialStep();

            if (currentStep == null)
                return false;

            return currentStep.tutorialAction == action;
        }

        private void RaiseEnemySpawnedEvent(BaseEnemy enemy)
        {
            _eventBusServiceObj.Publish(new EnemySpawnedEvent(enemy));
        }

        private void RaiseUpdateMissionInfoEvent(TutorialGroupData tutorialGroup)
        {
            _eventBusServiceObj.Publish(new UpdateMissionInfoEvent(tutorialGroup.tutorialGroupName, tutorialGroup.tutorialGoalInfo));
        }
    }
}