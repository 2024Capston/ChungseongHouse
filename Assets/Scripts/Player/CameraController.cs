using System;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 카메라를 조작하는 Class
/// </summary>
public class CameraController : NetworkBehaviour
{
    private const string FIRST_PERSON_INVERT_Y = "FirstPersonInvertY";
    private const string THIRD_PERSON_INVERT_Y = "ThirdPersonInvertY";

    [SerializeField] CinemachineVirtualCamera _firstPersonCamera;
    [SerializeField] CinemachineFreeLook _thirdPersonCamera;

    private GameObject _cameraHolder;       // 카메라 오브젝트를 담을 부모 오브젝트
    private GameObject _visualReference;    // 플레이어에 대한 Visual Reference

    private CinemachinePOV _cinemachinePOV; // 1인칭 시점 컴포넌트
    private AxisState _axisState;           // 1인칭 시점의 위치를 계산하기 위한 구조체
    private Vector2 _lookAroundInput;       // 화면 회전 입력 값

    private Rigidbody _rigidbody;           // 플레이어의 Rigidbody
    private PlayerRenderer _playerRenderer; // 플레이어의 렌더러

    private bool _firstPersonInvertY;
    private bool _thirdPersonInvertY;

    // 카메라 조작 활성화 여부
    private static bool _isInputEnabled = true;
    public static bool IsInputEnabled
    {
        get => _isInputEnabled;
        set => _isInputEnabled = value;
    }

    // 1인칭 여부
    private bool _isFirstPerson;
    public bool IsFirstPerson
    {
        get => _isFirstPerson;
    }

    private static CameraController _localCamera;
    public static CameraController LocalCamera
    {
        get => _localCamera;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            if (!_localCamera)
            {
                Initialize();
            }
        }
        else
        {
            Destroy(_firstPersonCamera.gameObject);
            Destroy(_thirdPersonCamera.gameObject);
            Destroy(GetComponentInChildren<Camera>().gameObject);
        }
    }

    private void LateUpdate()
    {
        if (IsOwner)
        {
            if (_isFirstPerson)
            {
                // Camera Holder를 플레이어 위치로 이동
                _cameraHolder.transform.position = _visualReference.transform.position;

                // 화면 회전 입력 값을 AxisState 구조체에 적용
                if (_isInputEnabled)
                {
                    _cinemachinePOV.m_VerticalAxis.m_InputAxisValue = _lookAroundInput.y;

                    _axisState.m_InputAxisValue = _lookAroundInput.x;
                    _axisState.Update(Time.deltaTime);
                }

                // Camera Holder와 플레이어 Rigidbody를 갱신
                _cameraHolder.transform.rotation = Quaternion.Euler(Vector3.up * _axisState.Value);
                _rigidbody.MoveRotation(_cameraHolder.transform.rotation);

                // 플레이어가 바라보는 방향 갱신
                _playerRenderer.SetHeadTarget(Camera.main.transform.forward);
            }
            else if (_isInputEnabled)
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

    private void Initialize()
    {
        _cameraHolder = new GameObject("Camera Holder");
        _firstPersonCamera.transform.SetParent(_cameraHolder.transform);
        _thirdPersonCamera.transform.SetParent(_cameraHolder.transform);

        _cinemachinePOV = _firstPersonCamera.GetCinemachineComponent<CinemachinePOV>();
        _axisState = _cinemachinePOV.m_HorizontalAxis;

        _isFirstPerson = _firstPersonCamera.m_Priority > _thirdPersonCamera.m_Priority;

        ChangeInvertMode(PlayerPrefs.GetInt(FIRST_PERSON_INVERT_Y, 0) == 1, PlayerPrefs.GetInt(THIRD_PERSON_INVERT_Y, 0) == 1);

        _rigidbody = GetComponent<Rigidbody>();
        _playerRenderer = GetComponent<PlayerRenderer>();

        InputHandler.Instance.OnLookAround += OnLookAroundInput;

        NetworkInterpolator networkInterpolator = GetComponent<NetworkInterpolator>();

        networkInterpolator.AddVisualReferenceDependantFunction(() =>
        {
            _visualReference = networkInterpolator.VisualReference;

            _thirdPersonCamera.Follow = networkInterpolator.VisualReference.transform;
            _thirdPersonCamera.LookAt = networkInterpolator.VisualReference.transform;
        });

        _localCamera = this;
    }

    public void OnLookAroundInput(InputValue value)
    {
        _lookAroundInput = value.Get<Vector2>() / Time.deltaTime / 2048f;
    }

    /// <summary>
    /// 현재 카메라의 회전 값을 초기화
    /// </summary>
    /// <param name="initialAngle">1인칭 회전 초기 각도</param>
    public void ResetCamera(float initialAngle = 0f)
    {
        if (!_localCamera)
        {
            Initialize();
        }

        _cinemachinePOV.m_VerticalAxis.Value = 0f;
        _axisState.Value = initialAngle;

        _thirdPersonCamera.m_XAxis.Value = 0f;
        _thirdPersonCamera.m_YAxis.Value = 0f;
    }

    /// <summary>
    /// 현재 카메라 모드를 1인칭/3인칭으로 변경한다.
    /// </summary>
    /// <param name="toFirstPerson">1인칭으로 변경 여부</param>
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

    /// <summary>
    /// 카메라 상하반전 여부를 설정한다.
    /// </summary>
    /// <param name="firstPersonInvertY">1인칭 상하반전</param>
    /// <param name="thirdPersonInvertY">3인칭 상하반전</param>
    public void ChangeInvertMode(bool firstPersonInvertY, bool thirdPersonInvertY)
    {
        _cinemachinePOV.m_VerticalAxis.m_InvertInput = !firstPersonInvertY;
        _thirdPersonCamera.m_YAxis.m_InvertInput = !thirdPersonInvertY;
    }
}
