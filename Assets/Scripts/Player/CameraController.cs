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

    private GameObject _cameraHolder;
    private GameObject _visualReference;

    private CinemachinePOV _cinemachinePOV;
    private Vector2 _lookAroundInput;

    private Rigidbody _rigidbody;
    private PlayerRenderer _playerRenderer;
    private AxisState _axisState;

    private bool _isEnabled = true;

    private bool _isFirstPerson;
    public bool IsFirstPerson
    {
        get => _isFirstPerson;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _cameraHolder = new GameObject("Camera Holder");
            _firstPersonCamera.transform.SetParent(_cameraHolder.transform);
            _thirdPersonCamera.transform.SetParent(_cameraHolder.transform);

            _cinemachinePOV = _firstPersonCamera.GetCinemachineComponent<CinemachinePOV>();
            _isFirstPerson = _firstPersonCamera.m_Priority > _thirdPersonCamera.m_Priority;

            _rigidbody = GetComponent<Rigidbody>();
            _playerRenderer = GetComponent<PlayerRenderer>();
            _axisState = _cinemachinePOV.m_HorizontalAxis;

            // 처음 시작시 카메라 위치를 초기화
            _cinemachinePOV.m_VerticalAxis.Value = 0f;

            _thirdPersonCamera.m_XAxis.Value = 0f;
            _thirdPersonCamera.m_YAxis.Value = 0f;

            InputHandler.Instance.OnLookAround += OnLookAroundInput;

            NetworkInterpolator networkInterpolator = GetComponent<NetworkInterpolator>();

            networkInterpolator.AddVisualReferenceDependantFunction(() =>
            {
                _visualReference = networkInterpolator.VisualReference;

                _thirdPersonCamera.Follow = networkInterpolator.VisualReference.transform;
                _thirdPersonCamera.LookAt = networkInterpolator.VisualReference.transform;
            });

            // Cursor.lockState = CursorLockMode.Locked;
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
        if (IsOwner && _isEnabled)
        {
            if (_isFirstPerson)
            {
                _cameraHolder.transform.position = _visualReference.transform.position;

                _cinemachinePOV.m_VerticalAxis.m_InputAxisValue = _lookAroundInput.y;

                _axisState.m_InputAxisValue = _lookAroundInput.x;
                _axisState.Update(Time.deltaTime);

                _cameraHolder.transform.rotation = Quaternion.Euler(Vector3.up * _axisState.Value);
                _rigidbody.MoveRotation(_cameraHolder.transform.rotation);

                _playerRenderer.SetHeadTarget(Camera.main.transform.forward);
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

        if (_cameraHolder)
        {
            Destroy(_cameraHolder);
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

            _axisState.Value = Camera.main.transform.rotation.eulerAngles.y;
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

    public void EnableInput()
    {
        _isEnabled = true;
    }

    public void DisableInput()
    {
        _isEnabled = false;
    }
}
