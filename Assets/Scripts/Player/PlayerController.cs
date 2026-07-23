using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset _inputActionObj;

    [Header("Camera")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private CinemachineCamera _playerCamera;
    [SerializeField] private float _scopedFOV;
    [SerializeField] private Transform _cameraPivot;
    [SerializeField] private float _sensitivityX;
    [SerializeField] private float _sensitivityY;
    [SerializeField] private float _minPitch;
    [SerializeField] private float _maxPitch;

    [Header("Bullet Camera")]
    [SerializeField] private CinemachineCamera _bulletCamera;

    [Header("Gun")]
    [SerializeField] private Animator _playerGunAnimator;
    [SerializeField] private GameObject _scopeOverlay;
    [SerializeField] private float _scopeDuration;

    [Header("Shooting")]
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private Transform _bulletSpawnPoint;
    [SerializeField] private float _range = 100f;
    [SerializeField] private float _bulletSpeed = 50f;
    [SerializeField] private LayerMask _hitMask;
    [SerializeField] private float _boltActionDuration = 1.2f;

    private InputAction m_lookAction;
    private InputAction m_scopeAction;
    private InputAction m_shootAction;
    private Vector2 m_lookAmt;
    private float _yaw;
    private float _pitch;
    private bool _isScoped = false;
    private bool _canShoot = false;
    private float _normalFOV;
    private int playerGunLayerMask;

    private void OnEnable()
    {
        _inputActionObj.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        _inputActionObj.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        m_lookAction = InputSystem.actions.FindAction("Look");
        m_scopeAction = InputSystem.actions.FindAction("Scope");
        m_shootAction = InputSystem.actions.FindAction("Shoot");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerGunLayerMask = 1 << LayerMask.NameToLayer("PlayerGun");
    }

    private void Update()
    {
        Look();
        HandleGunScopeInput();
        HandleShootingInput();
    }

    private void Look()
    {
        m_lookAmt = m_lookAction.ReadValue<Vector2>();

        _yaw += m_lookAmt.x * _sensitivityX * Time.deltaTime;
        _pitch -= m_lookAmt.y * _sensitivityY * Time.deltaTime;
        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

        transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
        _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    private void HandleGunScopeInput()
    {
        if (m_scopeAction.WasPressedThisFrame())
        {
            _isScoped = !_isScoped;
            _playerGunAnimator.SetBool("isScoped", _isScoped);

            if (_isScoped)
            {
                StartCoroutine(OnEnterScope());
            }
            else
            {
                OnExitScope();
            }
        }
    }

    private IEnumerator OnEnterScope()
    {
        yield return new WaitForSeconds(_scopeDuration);
        _scopeOverlay.SetActive(true);
        _mainCamera.cullingMask &= ~playerGunLayerMask;
        _normalFOV = _playerCamera.Lens.FieldOfView;
        _playerCamera.Lens.FieldOfView = _scopedFOV;
        _canShoot = true;
    }

    private void OnExitScope()
    {
        _scopeOverlay.SetActive(false);
        _mainCamera.cullingMask |= playerGunLayerMask;
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

        yield return new WaitForSeconds(0.05f);

        ShootBullet();

        yield return new WaitForSeconds(_boltActionDuration);
        _canShoot = true;
    }

    private void ShootBullet()
    {
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


        _bulletCamera.gameObject.transform.position = _bulletSpawnPoint.position;
        Vector3 direction = (targetPoint - _bulletSpawnPoint.position);
        _bulletCamera.transform.rotation = Quaternion.LookRotation(direction.normalized) * Quaternion.Euler(90f, 0f, 0f);
        Quaternion rotation = Quaternion.LookRotation(direction.normalized) * Quaternion.Euler(90f, 0f, 0f);

        Bullet bullet = Instantiate(_bulletPrefab, _bulletSpawnPoint.position, rotation);

        bullet.Initialize(direction, _bulletSpeed);

        _bulletCamera.Follow = bullet.transform;
        _bulletCamera.LookAt = bullet.transform;
        _playerCamera.Priority = 5;
        _bulletCamera.Priority = 20;

        bullet.SetController(this);
    }

    public void RestorePlayerCamera()
    {
        _bulletCamera.Follow = null;
        _bulletCamera.Priority = 5;
        _playerCamera.Priority = 20;
    }
}
