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
        [SerializeField] private float _freeLookSensitivityX = 100f;
        [SerializeField] private float _freeLookSensitivityY = 100f;
        [SerializeField] private float _scopedSensitivityX = 10f;
        [SerializeField] private float _scopedSensitivityY = 10f;
        [SerializeField] private float _minPitch = -40f;
        [SerializeField] private float _maxPitch = 40f;

        [Header("Gun")]
        [SerializeField] private Animator _playerGunAnimator;
        [SerializeField] private GameObject _scopeOverlay;
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
        private InputAction m_lookAction;
        private InputAction m_scopeAction;
        private InputAction m_shootAction;
        private Vector2 m_lookAmt;
        private float _yaw;
        private float _pitch;
        private bool _isScoped = false;
        private bool _canShoot = false;
        private bool _canLook = true;
        private bool _canTeleport = false;
        private float _normalFOV;
        private int playerGunLayerMask;
        private EventBusService _eventBusServiceObj;
        private BulletService _bulletServiceObj;

        private void OnEnable()
        {
            _inputActionObj.FindActionMap("Player").Enable();
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            _inputActionObj.FindActionMap("Player").Disable();
            UnsubscribeToEvents();
        }

        protected virtual void SubscribeToEvents()
        {
            _eventBusServiceObj.Subscribe<PlayerBulletHitEnemyEvent>(OnPlayerBulletHitEnemy);
            _eventBusServiceObj.Subscribe<PlayerBulletMissedEnemyEvent>(OnPlayerBulletMissedEnemyEvent);
            _eventBusServiceObj.Subscribe<ActivatePlayerTeleportAbilityEvent>(OnActivatePlayerTeleportAbilityEvent);
        }

        protected virtual void UnsubscribeToEvents()
        {
            _eventBusServiceObj.Unsubscribe<PlayerBulletHitEnemyEvent>(OnPlayerBulletHitEnemy);
            _eventBusServiceObj.Unsubscribe<PlayerBulletMissedEnemyEvent>(OnPlayerBulletMissedEnemyEvent);
            _eventBusServiceObj.Unsubscribe<ActivatePlayerTeleportAbilityEvent>(OnActivatePlayerTeleportAbilityEvent);
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _capsuleCollider = GetComponent<CapsuleCollider>();
            m_lookAction = InputSystem.actions.FindAction("Look");
            m_scopeAction = InputSystem.actions.FindAction("Scope");
            m_shootAction = InputSystem.actions.FindAction("Shoot");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            playerGunLayerMask = 1 << LayerMask.NameToLayer("PlayerGun");
            _eventBusServiceObj = GameManager.Instance.Services.Get<EventBusService>();
            _bulletServiceObj = GameManager.Instance.Services.Get<BulletService>();
        }

        private void Start()
        {
            SetPlayerCameraTarget();
            SetBulletCameraTarget();
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


        private void Update()
        {
            Look();
            HandleGunScopeInput();
            HandleShootingInput();
        }

        private void Look()
        {
            if (!_canLook) return;

            m_lookAmt = m_lookAction.ReadValue<Vector2>();

            float sensitivityX = _isScoped ? _scopedSensitivityX : _freeLookSensitivityX;
            float sensitivityY = _isScoped ? _scopedSensitivityY : _freeLookSensitivityY;

            _yaw += m_lookAmt.x * sensitivityX * Time.deltaTime;
            _pitch -= m_lookAmt.y * sensitivityY * Time.deltaTime;
            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void HandleGunScopeInput()
        {
            if (!m_scopeAction.WasPressedThisFrame())
                return;

            if (_isScoped)
                DisableScope();
            else
                EnableScope();
        }

        private void EnableScope()
        {
            _isScoped = true;
            _playerGunAnimator.SetBool("isScoped", _isScoped);
            StartCoroutine(ActivateScopeRoutine());
        }

        private void DisableScope()
        {
            _isScoped = false;
            _playerGunAnimator.SetBool("isScoped", _isScoped);
            HandleScopeDeactivation();
        }

        private IEnumerator ActivateScopeRoutine()
        {
            yield return new WaitForSeconds(_scopeDuration);
            _scopeOverlay.SetActive(true);
            StopRenderingGun();
            _normalFOV = _playerCamera.Lens.FieldOfView;
            _playerCamera.Lens.FieldOfView = _scopedFOV;
            _canShoot = true;
        }

        private void HandleScopeDeactivation()
        {
            _scopeOverlay.SetActive(false);
            RestoreRenderingGun();
            _playerCamera.Lens.FieldOfView = _normalFOV;
            _canShoot = false;
        }

        private void HandleShootingInput()
        {
            if (!_canShoot)
                return;

            if (!_isScoped)
                return;

            if (!m_shootAction.WasPressedThisFrame())
                return;

            StartCoroutine(ShootRoutine());
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
