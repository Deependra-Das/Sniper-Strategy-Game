using SniperStrategyGame.Enemy;
using SniperStrategyGame.Event;
using SniperStrategyGame.Main;
using SniperStrategyGame.Path;
using SniperStrategyGame.SceneLoader;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace SniperStrategyGame.Gameplay
{
    public class GameplayManager : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset _inputActionObj;

        [SerializeField] private List<Transform> _guardSpawnPointList;
        [SerializeField] private List<Transform> _patrolSpawnPointList;
        [SerializeField] private List<Transform> _shieldSpawnPointList;
        [SerializeField] private List<PatrolPath> _patrolPathList;


        private readonly List<BaseEnemy> _aliveEnemies = new();
        private EventBusService _eventBusServiceObj;
        private EnemyService _enemyServiceObj;

        private SceneLoaderService _sceneLoaderObj;

        private InputActionMap _uiActionMap;
        private InputAction _pauseAction;

        private bool _isPaused;
        private bool _isCleanedUp;
        private bool _isPlayerInputEnabled;
        private bool _playerInputBeforePause;
        private bool _isStageCompleted;

        public bool IsPaused => _isPaused;

        public bool CanPause => !_isPaused && !_isCleanedUp && !_isStageCompleted;

        private void SubscribeToEvents()
        {
            _eventBusServiceObj.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            _eventBusServiceObj.Subscribe<RequestPauseGameplayEvent>(OnPauseGameplayRequested);
            _eventBusServiceObj.Subscribe<RequestResumeGameplayEvent>(OnResumeGameplayRequested);
            _eventBusServiceObj.Subscribe<RequestRestartGameplayEvent>(OnRestartGameplayRequested);
            _eventBusServiceObj.Subscribe<RequestExitGameplayEvent>(OnExitGameplayRequested);
        }

        private void UnsubscribeFromEvents()
        {
            _eventBusServiceObj.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            _eventBusServiceObj.Unsubscribe<RequestPauseGameplayEvent>(OnPauseGameplayRequested);
            _eventBusServiceObj.Unsubscribe<RequestResumeGameplayEvent>(OnResumeGameplayRequested);
            _eventBusServiceObj.Unsubscribe<RequestRestartGameplayEvent>(OnRestartGameplayRequested);
            _eventBusServiceObj.Unsubscribe<RequestExitGameplayEvent>(OnExitGameplayRequested);
        }

        private void Awake()
        {
            var services = GameManager.Instance.Services;

            _eventBusServiceObj = services.Get<EventBusService>();
            _enemyServiceObj = services.Get<EnemyService>();
            _sceneLoaderObj = services.Get<SceneLoaderService>();

            _uiActionMap = _inputActionObj.FindActionMap("UI");
            _pauseAction = _uiActionMap.FindAction("Cancel");
        }

        private void OnEnable()
        {
            _pauseAction.performed += TogglePause;
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            _pauseAction.performed -= TogglePause;
            UnsubscribeFromEvents();
        }

        private void Start()
        {
            StartGameplay();
        }

        private void StartGameplay()
        {
            if (_isCleanedUp)
                return;

            _isStageCompleted = false;

            SpawnEnemies();
            RaiseActivateEnemiesEvent();
            RaiseActivatePlayerTeleportAbilityEvent();
            EnablePlayerInput();
            RaiseTogglePauseButtonVisibilityEvent(true);
        }

        private void SpawnEnemies()
        {
            SpawnEnemyGroup(EnemyTypeEnum.Guard, _guardSpawnPointList);
            SpawnEnemyGroup(EnemyTypeEnum.Shield, _shieldSpawnPointList);
            SpawnEnemyGroup(EnemyTypeEnum.Patrol, _patrolSpawnPointList);
        }

        private void SpawnEnemyGroup(EnemyTypeEnum enemyType, List<Transform> spawnPointList)
        {
            if (!_enemyServiceObj.TryGetEnemyPrefab(enemyType, out BaseEnemy enemyPrefab))
            {
                Debug.LogError($"Missing prefab for {enemyType}");
                return;
            }
            for (int j = 0; j < spawnPointList.Count; j++)
            {
                Transform spawnPoint = spawnPointList[j];

                if (spawnPoint == null)
                    continue;

                if (!NavMesh.SamplePosition(spawnPoint.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                {
                    Debug.LogWarning($"No NavMesh found near {spawnPoint.name}");
                    continue;
                }

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

        private void OnEnemyDied(EnemyDiedEvent eventObj)
        {
            if (_isCleanedUp || _isStageCompleted)
                return;

            if (eventObj.Enemy == null)
                return;

            if (!_aliveEnemies.Remove(eventObj.Enemy))
                return;

            Destroy(eventObj.Enemy.gameObject);

            if (_aliveEnemies.Count == 0)
            {
                StageCompleted();
            }
        }

        private void StageCompleted()
        {
            if (_isStageCompleted || _isCleanedUp)
                return;

            _isStageCompleted = true;

            DisablePlayerInput();
            RaiseTogglePauseButtonVisibilityEvent(false);
            EnableCursor();

            Debug.Log("Stage Complete");
        }

        private void EnablePlayerInput()
        {
            _isPlayerInputEnabled = true;
            RaiseTogglePlayerInputEvent(true);
        }

        private void DisablePlayerInput()
        {
            _isPlayerInputEnabled = false;
            RaiseTogglePlayerInputEvent(false);
        }

        private void EnableCursor()
        {
            RaiseToggleCursorEvent(true);
        }

        private void DisableCursor()
        {
            RaiseToggleCursorEvent(false);
        }

        private void TogglePause(InputAction.CallbackContext context)
        {
            if (!CanPause) return;

            PauseGameplay();
        }

        private void OnPauseGameplayRequested(RequestPauseGameplayEvent eventObj)
        {
            if (!CanPause) return;

            PauseGameplay();
        }

        private void OnResumeGameplayRequested(RequestResumeGameplayEvent eventObj)
        {
            if (!_isPaused || _isCleanedUp) return;

            ResumeGameplay();
        }

        private void PauseGameplay()
        {
            _playerInputBeforePause = _isPlayerInputEnabled;

            _isPaused = true;

            DisablePlayerInput();
            EnableCursor();
            Time.timeScale = 0f;
            RaiseGameplayPausedEvent();
        }

        private void ResumeGameplay()
        {
            _isPaused = false;

            Time.timeScale = 1f;

            if (_playerInputBeforePause)
            {
                EnablePlayerInput();
            }

            DisableCursor();
            RaiseTogglePlayerInputEvent(_playerInputBeforePause);
            RaiseGameplayResumedEvent();

            _playerInputBeforePause = false;
        }

        private void CleanUpGameplay()
        {
            if (_isCleanedUp) return;

            _isCleanedUp = true;
            DisablePlayerInput();
            Time.timeScale = 1f;
            _isPaused = false;

            foreach (BaseEnemy enemy in _aliveEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }

            _aliveEnemies.Clear();
            _isStageCompleted = false;
            _isPlayerInputEnabled = false;
            _playerInputBeforePause = false;

            RaiseResetPlayerEvent();
            RaiseTogglePauseButtonVisibilityEvent(false);
            EnableCursor();

            Debug.Log("Gameplay Cleaned Up");
        }

        private void OnRestartGameplayRequested(RequestRestartGameplayEvent eventObj)
        {
            if (_isCleanedUp) return;

            RestartGameplay();
        }

        private void RestartGameplay()
        {
            RaiseGameplayRestartedEvent();
            CleanUpGameplay();
            _isCleanedUp = false;
            StartGameplay();
        }

        private void OnExitGameplayRequested(RequestExitGameplayEvent eventObj)
        {
            if (_isCleanedUp) return;

            ExitGameplay();
        }

        private void ExitGameplay()
        {
            RaiseGameplayExitedEvent();
            CleanUpGameplay();
            _isCleanedUp = false;
            _sceneLoaderObj.LoadScene(SceneNameEnum.StageSelection);
        }

        private void RaiseEnemySpawnedEvent(BaseEnemy enemy)
        {
            _eventBusServiceObj.Publish(new EnemySpawnedEvent(enemy));
        }

        private void RaiseActivateEnemiesEvent()
        {
            _eventBusServiceObj.Publish(new ActivateEnemiesEvent());
        }

        private void RaiseActivatePlayerTeleportAbilityEvent()
        {
            _eventBusServiceObj.Publish(new ActivatePlayerTeleportAbilityEvent());
        }

        private void RaiseTogglePlayerInputEvent(bool value)
        {
            _eventBusServiceObj.Publish(new TogglePlayerInputEvent(value));
        }

        private void RaiseToggleCursorEvent(bool value)
        {
            _eventBusServiceObj.Publish(new ToggleCursorEvent(value));
        }

        private void RaiseTogglePauseButtonVisibilityEvent(bool value)
        {
            _eventBusServiceObj.Publish(new TogglePauseButtonVisibilityEvent(value));
        }

        private void RaiseGameplayPausedEvent()
        {
            _eventBusServiceObj.Publish(new GameplayPausedEvent());
        }

        private void RaiseGameplayResumedEvent()
        {
            _eventBusServiceObj.Publish(new GameplayResumedEvent());
        }

        private void RaiseGameplayRestartedEvent()
        {
            _eventBusServiceObj.Publish(new GameplayRestartedEvent());
        }

        private void RaiseGameplayExitedEvent()
        {
            _eventBusServiceObj.Publish(new GameplayExitedEvent());
        }

        private void RaiseResetPlayerEvent()
        {
            _eventBusServiceObj.Publish(new ResetPlayerEvent());
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();

            if (_pauseAction != null)
            {
                _pauseAction.performed -= TogglePause;
            }

            Time.timeScale = 1f;
        }
    }
}
