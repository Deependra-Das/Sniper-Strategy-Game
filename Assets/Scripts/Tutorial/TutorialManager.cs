using SniperStrategyGame.Enemy;
using SniperStrategyGame.Event;
using SniperStrategyGame.Main;
using SniperStrategyGame.Path;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SniperStrategyGame.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset _inputActionObj;

        [SerializeField] private float _stepTransitionDelay = 1f;
        [SerializeField] private Tutorial_SO _tutorialSequenceSO;
        [SerializeField] private List<Transform> _enemySpawnPointList;
        [SerializeField] private List<PatrolPath> _patrolPathList;

        private readonly List<BaseEnemy> _currentStepEnemies = new();

        private int _currentTutorialGroupIndex;
        private int _currentTutorialStepIndex;

        private EventBusService _eventBusServiceObj;
        private EnemyService _enemyServiceObj;

        private InputActionMap _uiActionMap;
        private InputAction _pauseAction;

        private bool _isTransitioning;
        private bool _isPaused;
        private bool _isTutorialStarted;
        private bool _isCleanedUp;
        private bool _isPlayerInputEnabled;
        private bool _playerInputBeforePause;
        private bool _canPauseTutorial;

        private void Awake()
        {
            var services = GameManager.Instance.Services;

            _eventBusServiceObj = services.Get<EventBusService>();
            _enemyServiceObj = services.Get<EnemyService>();

            _uiActionMap = _inputActionObj.FindActionMap("UI");
            _pauseAction = _uiActionMap.FindAction("Cancel");       
        }

        private void EnableInput()
        {
            _pauseAction.performed += TogglePause;
        }

        private void DisableInput()
        {
            _pauseAction.performed -= TogglePause;
        }

        private void OnEnable()
        {
            EnableInput();
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            DisableInput();
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            _eventBusServiceObj.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            _eventBusServiceObj.Subscribe<PlayerScopeInEvent>(OnScopeIn);
            _eventBusServiceObj.Subscribe<PlayerScopeOutEvent>(OnScopeOut);
            _eventBusServiceObj.Subscribe<PlayerBulletMissedEnemyEvent>(OnPlayerBulletMissedEnemy);
            _eventBusServiceObj.Subscribe<ContinueTutorialEvent>(OnContinueTutorial);
            _eventBusServiceObj.Subscribe<TutorialResumeEvent>(OnResumeTutorial);
            _eventBusServiceObj.Subscribe<TogglePauseButtonVisibilityEvent>(OnTogglePauseButtonVisibility);
        }

        private void UnsubscribeFromEvents()
        {
            _eventBusServiceObj.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            _eventBusServiceObj.Unsubscribe<PlayerScopeInEvent>(OnScopeIn);
            _eventBusServiceObj.Unsubscribe<PlayerScopeOutEvent>(OnScopeOut);
            _eventBusServiceObj.Unsubscribe<PlayerBulletMissedEnemyEvent>(OnPlayerBulletMissedEnemy);
            _eventBusServiceObj.Unsubscribe<ContinueTutorialEvent>(OnContinueTutorial);
            _eventBusServiceObj.Unsubscribe<TogglePauseButtonVisibilityEvent>(OnTogglePauseButtonVisibility);
        }

        private void Start()
        {
            StartTutorial();
        }

        private void StartTutorial()
        {
            _currentTutorialGroupIndex = 0;
            _currentTutorialStepIndex = 0;
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

            if (currentStep.requiredEnemySpawnDataList.Count > 0)
            {
                foreach (TutorialEnemySpawnData enemyData in currentStep.requiredEnemySpawnDataList)
                {
                    SpawnEnemy(enemyData);
                }
            }

            RaiseActivateEnemiesEvent();
            RaiseTogglePlyerShootAbilityEvent(currentStep.allowShooting);

            if(currentStep.tutorialAction == TutorialActionEnum.TeleportAbility)
            {
                RaiseActivatePlayerTeleportAbilityEvent();
            }

            _canPauseTutorial = true;
        }

        private void SpawnEnemy(TutorialEnemySpawnData enemyData)
        {
            if (!_enemyServiceObj.TryGetEnemyPrefab(enemyData.enemyType, out BaseEnemy enemyPrefab))
            {
                Debug.LogError($"Missing prefab for {enemyData}");
                return;
            }

            Transform spawnPointTransform = GetSpawnPointTransformByIndex(enemyData.spawnPointIndex);

            if (spawnPointTransform == null)
            {
                Debug.LogWarning($"No spawn point Transform configured for Spawn Point Index : {enemyData.spawnPointIndex}");
                return;
            }
           
            BaseEnemy enemy = Instantiate(enemyPrefab, spawnPointTransform.position, spawnPointTransform.rotation);
            enemy.Initialize();

            if (enemy is PatrolEnemy patrolEnemy)
            {
                patrolEnemy.SetPatrolPath(GetPatrolPath(0));
            }

            _currentStepEnemies.Add(enemy);

            RaiseEnemySpawnedEvent(enemy);            
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

        private Transform GetSpawnPointTransformByIndex(int index)
        {
            return _enemySpawnPointList[index];
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

            RaiseTogglePauseButtonVisibilityEvent(true);
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
            DisablePlayerInput();
            EnableCursor();

            TutorialGroupData completedGroup = GetCurrentTutorialGroup();

            if (completedGroup == null)
            {
                TutorialCompleted();
                return;
            }

            TutorialGroupData nextGroup = GetTutorialGroup(_currentTutorialGroupIndex + 1);

            if (nextGroup == null)
            {
                TutorialCompleted();
                return;
            }

            Debug.Log($"Tutorial Group Completed: {completedGroup.tutorialGroupName}");
            RaiseTogglePauseButtonVisibilityEvent(false);
            RaiseTutorialGroupCompletedEvent(completedGroup.tutorialGroupName, nextGroup.tutorialGroupName);       
        }

        private void TutorialCompleted()
        {
            Debug.Log("Tutorial Completed");
            RaiseAllTutorialsCompletedEvent();
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
            if (!IsValidEnemyDeath(eventObj))
                return;

            RemoveDeadEnemy(eventObj.Enemy);

            TutorialStepData currentStep = GetCurrentTutorialStep();

            if (currentStep == null)
                return;

            if (currentStep.tutorialAction == TutorialActionEnum.TeleportAbility)
            {
                HandleTeleportStepEnemyDeath();
                return;
            }

            HandleRequiredEnemyStepDeath(currentStep, eventObj.Enemy);
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

        private bool IsValidEnemyDeath(EnemyDiedEvent eventObj)
        {
            if (eventObj.Enemy == null)
                return false;

            if (!_currentStepEnemies.Contains(eventObj.Enemy))
                return false;

            return true;
        }

        private void RemoveDeadEnemy(BaseEnemy enemy)
        {
            _currentStepEnemies.Remove(enemy);

            Destroy(enemy.gameObject);
        }

        private void HandleTeleportStepEnemyDeath()
        {
            if (_currentStepEnemies.Count == 0)
            {
                AdvanceToNextTutorialStep();
            }
        }

        private void HandleRequiredEnemyStepDeath(TutorialStepData currentStep, BaseEnemy deadEnemy)
        {
            if (!TryGetRequiredEnemyType(currentStep.tutorialAction, out EnemyTypeEnum requiredEnemyType))            
            {
                return;
            }

            if (deadEnemy.EnemyType != requiredEnemyType) return;

            if (_currentStepEnemies.Count == 0)
            {
                AdvanceToNextTutorialStep();
            }
        }

        private void EnablePlayerInput()
        {
            _isPlayerInputEnabled = true;
            RaiseTogglePlayerInputEventEvent(true);
        }

        private void DisablePlayerInput()
        {
            _isPlayerInputEnabled = false;
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

        private void TogglePause(InputAction.CallbackContext context)
        {
            PauseTutorial();
        }

        private void PauseTutorial()
        {
            if (_isPaused || !_canPauseTutorial || _isTransitioning || _isCleanedUp)
                return;

            _playerInputBeforePause = _isPlayerInputEnabled;

            _isPaused = true;
            DisablePlayerInput();
            EnableCursor();
            RaiseTutorialPauseEventEvent();
            Time.timeScale = 0f;
        }

        private void OnResumeTutorial(TutorialResumeEvent eventObj)
        {
            ResumeTutorial();
        }

        private void ResumeTutorial()
        {
            if (!_isPaused || _isCleanedUp)
                return;

            if (_playerInputBeforePause)
            {
                EnablePlayerInput();
            }

            _isPaused = false;
            Time.timeScale = 1f;
            DisableCursor();
            RaiseTogglePlayerInputEventEvent(_playerInputBeforePause);
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

        private void RaiseAllTutorialsCompletedEvent()
        {
            _eventBusServiceObj.Publish(new AllTutorialsCompletedEvent());
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

        private void RaiseActivateEnemiesEvent()
        {
            _eventBusServiceObj.Publish(new ActivateEnemiesEvent());
        }

        private void RaiseTogglePlyerShootAbilityEvent(bool canShoot)
        {
            _eventBusServiceObj.Publish(new TogglePlyerShootAbilityEvent(canShoot));
        }

        private void RaiseActivatePlayerTeleportAbilityEvent()
        {
            _eventBusServiceObj.Publish(new ActivatePlayerTeleportAbilityEvent());
        }

        private void RaiseTutorialPauseEventEvent()
        {
            _eventBusServiceObj.Publish(new TutorialPauseEvent());
        }

        private void RaiseTogglePauseButtonVisibilityEvent(bool value)
        {
            _eventBusServiceObj.Publish(new TogglePauseButtonVisibilityEvent(value));
        }

        private void OnTogglePauseButtonVisibility(TogglePauseButtonVisibilityEvent eventObj)
        {
            _canPauseTutorial = eventObj.IsVisible;
        }

        private void OnDestroy()
        {
            _pauseAction.Dispose();
        }

    }
}