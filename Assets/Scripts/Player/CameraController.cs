using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 카메라를 조작하는 Class
/// </summary>
public class CameraController : NetworkBehaviour
{
    [SerializeField] CinemachineVirtualCamera _firstPersonCamera;
    [SerializeField] CinemachineFreeLook _thirdPersonCamera;

    private GameObject _firstPersonCameraHolder;

    private CinemachinePOV _cinemachinePOV;
    private Vector2 _lookAroundInput;

    private Rigidbody _rigidbody;
    private AxisState _axisState;
    private float _currentYaw;
    private float _lastYaw;

    private bool _isFirstPerson;
    public bool IsFirstPerson
    {
        get => _isFirstPerson;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _firstPersonCameraHolder = new GameObject("Camera Holder");
            _firstPersonCamera.transform.parent = _firstPersonCameraHolder.transform;

            _cinemachinePOV = _firstPersonCamera.GetCinemachineComponent<CinemachinePOV>();
            _isFirstPerson = _firstPersonCamera.m_Priority > _thirdPersonCamera.m_Priority;

            _rigidbody = GetComponent<Rigidbody>();
            _axisState = _cinemachinePOV.m_HorizontalAxis;

            // 처음 시작시 카메라 위치를 초기화
            _cinemachinePOV.m_VerticalAxis.Value = 0f;

            _thirdPersonCamera.m_XAxis.Value = 0f;
            _thirdPersonCamera.m_YAxis.Value = 0f;

            InputHandler.Instance.OnLookAround += OnLookAroundInput;

            NetworkInterpolator networkInterpolator = GetComponent<NetworkInterpolator>();

            networkInterpolator.AddVisualReferenceDependantFunction(() =>
            {
                _thirdPersonCamera.Follow = networkInterpolator.VisualReference.transform;
                _thirdPersonCamera.LookAt = networkInterpolator.VisualReference.transform;
            });

            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Destroy(_firstPersonCamera.gameObject);
            Destroy(_thirdPersonCamera.gameObject);
            Destroy(GetComponentInChildren<Camera>().gameObject);
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            if (_isFirstPerson)
            {
                _firstPersonCameraHolder.transform.position = transform.position;

                _cinemachinePOV.m_VerticalAxis.m_InputAxisValue = _lookAroundInput.y;

                _axisState.m_InputAxisValue = _lookAroundInput.x;
                _axisState.Update(Time.deltaTime);

                float newYaw = _axisState.Value;
                _currentYaw += newYaw - _lastYaw;

                if (_currentYaw > 180f)
                {
                    _currentYaw -= 360f;
                }
                else if (_currentYaw < -180f)
                {
                    _currentYaw += 360f;
                }

                _firstPersonCameraHolder.transform.rotation = Quaternion.Euler(Vector3.up * _currentYaw);
                _rigidbody.MoveRotation(_firstPersonCameraHolder.transform.rotation);

                _lastYaw = newYaw;
            }
            else
            {
                _thirdPersonCamera.m_YAxis.m_InputAxisValue = _lookAroundInput.y;
                _thirdPersonCamera.m_XAxis.m_InputAxisValue = _lookAroundInput.x;
            }
        }
    }

    private new void OnDestroy()
    {
        InputHandler.Instance.OnLookAround -= OnLookAroundInput;

        if (_firstPersonCameraHolder)
        {
            Destroy(_firstPersonCameraHolder);
        }

        base.OnDestroy();
    }

    public void ChangeCameraMode(bool toFirstPerson)
    {
        if (toFirstPerson && !_isFirstPerson)
        {
            _isFirstPerson = true;

            _firstPersonCamera.m_Priority = 10;
            _thirdPersonCamera.m_Priority = 0;

            _cinemachinePOV.m_HorizontalAxis.Value = Camera.main.transform.rotation.eulerAngles.y;
            _cinemachinePOV.m_VerticalAxis.Value = 0f;
        }
        else if (!toFirstPerson && _isFirstPerson)
        {
            _isFirstPerson = false;

            _firstPersonCamera.m_Priority = 0;
            _thirdPersonCamera.m_Priority = 10;
        }
    }

    public void OnLookAroundInput(InputValue value)
    {
        _lookAroundInput = value.Get<Vector2>() / Time.deltaTime / 2048f;
    }
}
