using SniperStrategyGame.Bullet;
using SniperStrategyGame.Event;
using SniperStrategyGame.Main;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SniperStrategyGame.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset _inputActionObj;

        [Header("Camera")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private CinemachineCamera _playerCamera;
        [SerializeField] private CinemachineCamera _bulletCamera;
        [SerializeField] private float _scopedFOV;
        [SerializeField] private Transform _cameraPivot;
        [SerializeField] private float _freeLookSensitivityX = 0.1f;
        [SerializeField] private float _freeLookSensitivityY = 0.1f;
        [SerializeField] private float _scopedSensitivityX = 0.025f;
        [SerializeField] private float _scopedSensitivityY = 0.025f;
        [SerializeField] private float _minPitch = -20f;
        [SerializeField] private float _maxPitch = 10f;

        [Header("Gun")]
        [SerializeField] private Animator _playerGunAnimator;
        [SerializeField] private float _scopeDuration;

        [Header("Shooting")]
        [SerializeField] private Transform _bulletSpawnPoint;
        [SerializeField] private float _range = 100f;
        [SerializeField] private float _bulletSpeed = 50f;
        [SerializeField] private LayerMask _hitMask;
        [SerializeField] private float _boltActionDuration = 1.2f;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private float _groundSphereCastRadius = 0.5f;
        private const float _groundRaycastHeight = 10f;
        private const float _groundRaycastMaxDistance = 100f;

        private Rigidbody _rigidbody;
        private CapsuleCollider _capsuleCollider;

        private InputActionMap _playerActionMap;
        private InputAction _lookAction;
        private InputAction _scopeAction;
        private InputAction _shootAction;
        private float _yaw;
        private float _pitch;
        private bool _isScoped = false;
        private bool _canShoot = false;
        private bool _canLook = true;
        private bool _canTeleport = false;
        private Coroutine _scopeCoroutine;
        private float _normalFOV;
        private int playerGunLayerMask;
        private EventBusService _eventBusServiceObj;
        private BulletService _bulletServiceObj;

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

        protected virtual void SubscribeToEvents()
        {
            _eventBusServiceObj.Subscribe<PlayerBulletHitEnemyEvent>(OnPlayerBulletHitEnemy);
            _eventBusServiceObj.Subscribe<PlayerBulletMissedEnemyEvent>(OnPlayerBulletMissedEnemyEvent);
            _eventBusServiceObj.Subscribe<ActivatePlayerTeleportAbilityEvent>(OnActivatePlayerTeleportAbilityEvent);
        }

        protected virtual void UnsubscribeFromEvents()
        {
            _eventBusServiceObj.Unsubscribe<PlayerBulletHitEnemyEvent>(OnPlayerBulletHitEnemy);
            _eventBusServiceObj.Unsubscribe<PlayerBulletMissedEnemyEvent>(OnPlayerBulletMissedEnemyEvent);
            _eventBusServiceObj.Unsubscribe<ActivatePlayerTeleportAbilityEvent>(OnActivatePlayerTeleportAbilityEvent);
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _capsuleCollider = GetComponent<CapsuleCollider>();

            InitializeInput();
            _eventBusServiceObj = GameManager.Instance.Services.Get<EventBusService>();
            _bulletServiceObj = GameManager.Instance.Services.Get<BulletService>();

            playerGunLayerMask = 1 << LayerMask.NameToLayer("PlayerGun");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }

        private void Start()
        {
            SetPlayerCameraTarget();
            SetBulletCameraTarget();
            _normalFOV = _playerCamera.Lens.FieldOfView;
        }

        private void SetPlayerCameraTarget()
        {
            _playerCamera.Follow = _cameraPivot;
            _playerCamera.LookAt = _cameraPivot;
        }

        private void SetBulletCameraTarget()
        {
            _bulletCamera.Follow = _bulletSpawnPoint;
            _bulletCamera.LookAt = _bulletSpawnPoint;
            _bulletCamera.transform.position = _bulletSpawnPoint.position;
            _bulletCamera.transform.rotation = _bulletSpawnPoint.rotation;
        }

        private void InitializeInput()
        {
            _playerActionMap =
                _inputActionObj.FindActionMap("Player");

            _lookAction =
                _playerActionMap.FindAction("Look");

            _scopeAction =
                _playerActionMap.FindAction("Scope");

            _shootAction =
                _playerActionMap.FindAction("Shoot");
        }

        private void EnableInput()
        {
            _playerActionMap.Enable();

            _lookAction.performed += OnLookPerformed;
            _scopeAction.performed += OnScopePerformed;
            _shootAction.performed += OnShootPerformed;
        }


        private void DisableInput()
        {
            _lookAction.performed -= OnLookPerformed;
            _scopeAction.performed -= OnScopePerformed;
            _shootAction.performed -= OnShootPerformed;

            _playerActionMap.Disable();
        }

        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            if (!_canLook) return;

            Vector2 lookAmount = context.ReadValue<Vector2>();
            float sensitivityX = _isScoped ? _scopedSensitivityX : _freeLookSensitivityX;
            float sensitivityY = _isScoped ? _scopedSensitivityY : _freeLookSensitivityY;
            _yaw += lookAmount.x * sensitivityX;
            _pitch -= lookAmount.y * sensitivityY;
            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void OnScopePerformed(InputAction.CallbackContext context)
        {
            if (_isScoped)
                TryScopeOut();
            else
                TryScopeIn();
        }

        private void OnShootPerformed(InputAction.CallbackContext context)
        {
            TryShoot();
        }

        public void TryScopeIn()
        {
            if (_isScoped) return;

            EnableScope();
        }

        public void TryScopeOut()
        {
            if (!_isScoped) return;

            DisableScope();
        }

        public void TryShoot()
        {
            if (!_canShoot) return;

            if (!_isScoped) return;

            StartCoroutine(ShootRoutine());
            _eventBusServiceObj.Publish(new PlayerShotEvent());
        }

        private void EnableScope()
        {
            _isScoped = true;
            _playerGunAnimator.SetBool("isScoped", _isScoped);
            _scopeCoroutine = StartCoroutine(ActivateScopeRoutine());
        }

        private void DisableScope()
        {
            _isScoped = false;

            if (_scopeCoroutine != null)
            {
                StopCoroutine(_scopeCoroutine);
                _scopeCoroutine = null;
            }

            _playerGunAnimator.SetBool("isScoped", _isScoped);
            HandleScopeDeactivation();
        }

        private IEnumerator ActivateScopeRoutine()
        {
            yield return new WaitForSeconds(_scopeDuration);

            if (!_isScoped) yield break;

            StopRenderingGun();
            _playerCamera.Lens.FieldOfView = _scopedFOV;
            _canShoot = true;
            _eventBusServiceObj.Publish(new PlayerScopeInEvent());
            _scopeCoroutine = null;
        }

        private void HandleScopeDeactivation()
        {
            RestoreRenderingGun();
            _playerCamera.Lens.FieldOfView = _normalFOV;
            _canShoot = false;
            _eventBusServiceObj.Publish(new PlayerScopeOutEvent());
        }

        private IEnumerator ShootRoutine()
        {
            _canShoot = false;
            _canLook = false;
            yield return new WaitForSeconds(0.05f);
            DisableScope();
            ShootBullet();
            yield return new WaitForSeconds(_boltActionDuration);
            _canShoot = true;
        }

        private void ShootBullet()
        {
            _eventBusServiceObj.Publish(new PlayerBulletFiredEvent());

            Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);

            Vector3 targetPoint;

            if (Physics.Raycast(ray, out RaycastHit hit, _range, _hitMask))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.GetPoint(_range);
            }

            Vector3 direction = (targetPoint - _bulletSpawnPoint.position);
            Quaternion rotation = Quaternion.LookRotation(direction.normalized) * Quaternion.Euler(0f, 0f, 90f);

            PlayerBullet bullet = _bulletServiceObj.SpawnPlayerBullet(_bulletSpawnPoint.position, rotation);

            bullet.Initialize(direction, _bulletSpeed);
            ActivateBulletCamera(bullet.transform);
        }

        private void ActivateBulletCamera(Transform bullet)
        {
            if (!_canTeleport)
            {
                SetBulletCameraTarget();
            }

            StopRenderingGun();
            _bulletCamera.Follow = bullet;
            _bulletCamera.LookAt = bullet;

            _playerCamera.Priority = 5;
            _bulletCamera.Priority = 20;
        }

        private void OnPlayerBulletHitEnemy(PlayerBulletHitEnemyEvent eventObj)
        {
            if (_canTeleport)
            {
               TeleportToShotEnemy(eventObj.enemyPosition, eventObj.shotDirection);
            }

            SwitchToPlayerCamera();
            RestoreRenderingGun();
            _canLook = true;
        }

        private void OnPlayerBulletMissedEnemyEvent(PlayerBulletMissedEnemyEvent eventObj)
        {
            SwitchToPlayerCamera();
            RestoreRenderingGun();
            _canLook = true;
        }

        private void OnActivatePlayerTeleportAbilityEvent(ActivatePlayerTeleportAbilityEvent eventObj)
        {
            _canTeleport = true;
        }

        private bool TeleportToShotEnemy(Vector3 enemyPosition, Vector3 rotation)
        {
            if (!TryGetGroundPosition(enemyPosition, out Vector3 groundPosition))
            {
                Debug.LogWarning($"Could not find ground below enemy at {enemyPosition}");
                return false;
            }

            float groundOffset = GetGroundOffset();

            Vector3 newPosition = new Vector3( enemyPosition.x, groundPosition.y + groundOffset, enemyPosition.z);

            _rigidbody.position = newPosition;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            Physics.SyncTransforms();

            return true;
        }

        private void StopRenderingGun()
        {
            _mainCamera.cullingMask &= ~playerGunLayerMask;
        }

        private void RestoreRenderingGun()
        {
            _mainCamera.cullingMask |= playerGunLayerMask;
        }

        private void SwitchToPlayerCamera()
        {
            _bulletCamera.Follow = null;
            _bulletCamera.LookAt = null;

            _bulletCamera.Priority = 5;
            _playerCamera.Priority = 20;

            if (_canTeleport)
            {
                StartCoroutine(ResetBulletCameraNextFrame());
            }
        }

        private IEnumerator ResetBulletCameraNextFrame()
        {
            yield return null;
            SetBulletCameraTarget();
        }

        private float GetGroundOffset()
        {
            float halfHeight = _capsuleCollider.height * 0.5f;
            return halfHeight - _capsuleCollider.radius - _capsuleCollider.center.y;
        }

        private bool TryGetGroundPosition(Vector3 worldPosition, out Vector3 groundPosition)
        {
            Vector3 sphereOrigin = worldPosition + Vector3.up * _groundRaycastHeight;

            if (Physics.SphereCast( sphereOrigin, _groundSphereCastRadius, Vector3.down, out RaycastHit hit, _groundRaycastMaxDistance, _groundMask, QueryTriggerInteraction.Ignore))
            {
                groundPosition = hit.point;
                return true;
            }

            groundPosition = default;
            return false;
        }
    }
}
