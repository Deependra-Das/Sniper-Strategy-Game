using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SniperStrategyGame.Enemy;
using SniperStrategyGame.Event;
using SniperStrategyGame.Main;
using SniperStrategyGame.Path;

namespace SniperStrategyGame.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] private float _stepTransitionDelay = 1f;
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

        private bool _isTransitioning;

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
            _eventBusServiceObj.Subscribe<PlayerBulletMissedEnemyEvent>(OnPlayerBulletMissedEnemy);
            _eventBusServiceObj.Subscribe<ContinueTutorialEvent>(OnContinueTutorial);
        }

        private void UnsubscribeFromEvents()
        {
            _eventBusServiceObj.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            _eventBusServiceObj.Unsubscribe<PlayerScopeInEvent>(OnScopeIn);
            _eventBusServiceObj.Unsubscribe<PlayerScopeOutEvent>(OnScopeOut);
            _eventBusServiceObj.Unsubscribe<PlayerBulletMissedEnemyEvent>(OnPlayerBulletMissedEnemy);
            _eventBusServiceObj.Unsubscribe<ContinueTutorialEvent>(OnContinueTutorial);
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

            if (currentGroup == null)
            {
                TutorialCompleted();
                return;
            }

            RaiseUpdateMissionInfoEvent(currentGroup);
            StartCoroutine(StartTutorialRoutine());
        }

        private IEnumerator StartTutorialRoutine()
        {
            DisablePlayerInput();

            yield return new WaitForSeconds(_stepTransitionDelay);

            ExecuteCurrentTutorialStep();

            EnablePlayerInput();
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
            RaiseTutorialStepStartedEvent(currentStep);

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

        private void AdvanceToNextTutorialGroup()
        {
            if (_isTransitioning) return;

            _isTransitioning = true;
            DisablePlayerInput();

            _currentTutorialGroupIndex++;
            _currentTutorialStepIndex = 0;

            _currentStepEnemies.Clear();

            TutorialGroupData nextGroup = GetCurrentTutorialGroup();

            if (nextGroup == null)
            {
                _isTransitioning = false;
                TutorialCompleted();
                return;
            }

            StartCoroutine(StartNextTutorialGroupRoutine(nextGroup));
        }

        private IEnumerator StartNextTutorialGroupRoutine(TutorialGroupData nextGroup)
        {
            yield return new WaitForSeconds(_stepTransitionDelay);

            Debug.Log($"Starting Tutorial Group: {nextGroup.tutorialGroupName}");
            RaiseUpdateMissionInfoEvent(nextGroup);
            ExecuteCurrentTutorialStep();
            _isTransitioning = false;
            EnablePlayerInput();
        }

        private void AdvanceToNextTutorialStep()
        {
            if (_isTransitioning) return;

            _isTransitioning = true;
            DisablePlayerInput();

            TutorialStepData completedStep = GetCurrentTutorialStep();

            if (completedStep != null)
            {
                RaiseTutorialStepCompletedEvent(completedStep);
            }

            StartCoroutine(StartNextTutorialStepRoutine());
        }

        private IEnumerator StartNextTutorialStepRoutine()
        {
            yield return new WaitForSeconds(_stepTransitionDelay);

            _currentTutorialStepIndex++;
            TutorialGroupData currentGroup = GetCurrentTutorialGroup();

            if (currentGroup != null && _currentTutorialStepIndex < currentGroup.tutorialStepsList.Count)
            {
                ExecuteCurrentTutorialStep();
                _isTransitioning = false;
                EnablePlayerInput();

                yield break;
            }

            _isTransitioning = false;
            CompleteCurrentTutorialGroup();
        }

        private void CompleteCurrentTutorialGroup()
        {
            TutorialGroupData completedGroup = GetCurrentTutorialGroup();

            if (completedGroup == null)
            {
                TutorialCompleted();
                return;
            }

            TutorialGroupData nextGroup = GetTutorialGroup(_currentTutorialGroupIndex + 1);

            DisablePlayerInput();

            if (nextGroup == null)
            {
                TutorialCompleted();
                return;
            }

            Debug.Log($"Tutorial Group Completed: {completedGroup.tutorialGroupName}");
            EnableCursor();
            RaiseTutorialGroupCompletedEvent(completedGroup.tutorialGroupName, nextGroup.tutorialGroupName);
       
        }

        private void TutorialCompleted()
        {
            Debug.Log("Tutorial Completed");
        }

        private TutorialGroupData GetTutorialGroup(int index)
        {
            if (_tutorialSequenceSO == null)
                return null;

            if (index < 0 ||
                index >= _tutorialSequenceSO.tutorialGroupsList.Count)
            {
                return null;
            }

            return _tutorialSequenceSO.tutorialGroupsList[index];
        }

        private void OnContinueTutorial(ContinueTutorialEvent eventObj)
        {
            DisableCursor();
            AdvanceToNextTutorialGroup();
        }

        private void OnScopeIn(PlayerScopeInEvent eventObj)
        {
            if (!IsCurrentAction(TutorialActionEnum.ScopeIn)) return;

            AdvanceToNextTutorialStep();
        }

        private void OnScopeOut(PlayerScopeOutEvent eventObj)
        {
            if (!IsCurrentAction(TutorialActionEnum.ScopeOut)) return;

            AdvanceToNextTutorialStep();
        }

        private void OnPlayerBulletMissedEnemy(PlayerBulletMissedEnemyEvent eventObj)
        {
            if (!IsCurrentAction(TutorialActionEnum.Shoot)) return;

            AdvanceToNextTutorialStep();
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

            AdvanceToNextTutorialStep();
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

        private void EnablePlayerInput()
        {
            RaiseTogglePlayerInputEventEvent(true);
        }

        private void DisablePlayerInput()
        {
            RaiseTogglePlayerInputEventEvent(false);
        }

        private void EnableCursor()
        {
            RaiseToggleCursorEventEvent(true);
        }

        private void DisableCursor()
        {
            RaiseToggleCursorEventEvent(false);
        }

        private void RaiseTogglePlayerInputEventEvent(bool value)
        {
            _eventBusServiceObj.Publish(new TogglePlayerInputEvent(value));
        }

        private void RaiseToggleCursorEventEvent(bool value)
        {
            _eventBusServiceObj.Publish(new ToggleCursorEvent(value));
        }

        private void RaiseTutorialStepCompletedEvent(TutorialStepData tutorialStep)
        {
            _eventBusServiceObj.Publish(new TutorialStepCompletedEvent(tutorialStep.tutorialAction));
        }

        private void RaiseTutorialGroupCompletedEvent(string completedTutorialGroupName,string nextTutorialGroupName)
        {
            _eventBusServiceObj.Publish(new TutorialGroupCompletedEvent(completedTutorialGroupName, nextTutorialGroupName));
        }

        private void RaiseEnemySpawnedEvent(BaseEnemy enemy)
        {
            _eventBusServiceObj.Publish(new EnemySpawnedEvent(enemy));
        }

        private void RaiseUpdateMissionInfoEvent(TutorialGroupData tutorialGroup)
        {
            _eventBusServiceObj.Publish(new UpdateMissionInfoEvent(tutorialGroup.tutorialGroupName, tutorialGroup.tutorialGoalInfo));
        }

        private void RaiseTutorialStepStartedEvent(TutorialStepData tutorialStep)
        {
            _eventBusServiceObj.Publish(new TutorialStepStartedEvent(tutorialStep));
        }
    }
}