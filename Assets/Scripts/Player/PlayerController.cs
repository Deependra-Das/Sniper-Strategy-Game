using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
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

    [Header("Gun")]
    [SerializeField] private Animator _playerGunAnimator;
    [SerializeField] private GameObject _scopeOverlay;
    [SerializeField] private float _scopeDuration;

    private InputAction m_lookAction;
    private InputAction m_scopeAction;
    private Vector2 m_lookAmt;
    private float _yaw;
    private float _pitch;
    private bool isScoped = false;
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
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerGunLayerMask = 1 << LayerMask.NameToLayer("PlayerGun");
    }

    private void Update()
    {
        Look();
        HandleGunInput();
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

    private void HandleGunInput()
    {
        if (m_scopeAction.WasPressedThisFrame())
        {
            isScoped = !isScoped;
            _playerGunAnimator.SetBool("isScoped", isScoped);

            if(isScoped)
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
    }

    private void OnExitScope()
    {
        _scopeOverlay.SetActive(false);
        _mainCamera.cullingMask |= playerGunLayerMask;
        _playerCamera.Lens.FieldOfView = _normalFOV;
    }


}
