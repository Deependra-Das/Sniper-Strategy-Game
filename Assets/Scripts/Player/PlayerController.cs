using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] InputActionAsset _inputActionObj;

    [Header("Camera")]
    [SerializeField] private Transform _cameraPivot;
    [SerializeField] private float _sensitivityX;
    [SerializeField] private float _sensitivityY;
    [SerializeField] private float _minPitch;
    [SerializeField] private float _maxPitch;

    private InputAction m_lookAction;
    private Vector2 m_lookAmt;
    private float _yaw;
    private float _pitch;

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
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Look();
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

}
