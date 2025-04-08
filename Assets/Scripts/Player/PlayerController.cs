using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어를 조작하는 Class
/// </summary>
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float _walkSpeed;    // 이동 속력
    [SerializeField] private float _jumpForce;     // 점프 속력

    private const float GROUND_DETECTION_THRESHOLD = 1f;        // 접지 판정 범위
    private const float JUMP_REMEMBER_TIME = 0.64f;             // 점프 키 입력 기억 시간
    private const float MAXIMUM_REACH_DISTANCE = 16f;           // 상호작용 가능 범위

    public static float INITIAL_CAPSULE_HEIGHT = 2f;             // 최초 Capsule Collider 높이
    public static float INITIAL_CAPSULE_RADIUS = 0.5f;           // 최초 Capsule Collider 반경 

    private Rigidbody _rigidbody;
    private Collider _collider;
    private PlayerRenderer _playerRenderer;
    private CameraController _cameraController;
    private NetworkPlatformFinder _networkPlatformFinder;

    private IInteractable _interactableOnPointer;  // 플레이어가 바라보고 있는 Interactable
    private IInteractable _interactableInHand;     // 플레이어가 들고 있는 Interactable

    // 입력 관련
    private bool _jumpInput;        // 점프 입력 여부
    private float _jumpRemember;    // 입력된 점프를 처리할 수 있는 쿨타임
    private bool _isEnabled = true; // 플레이어 조작 활성화 여부

    private Vector3 _lastPosition;
    private Quaternion _lastRotation;

    // 로컬 플레이어를 나타내는 static 변수
    private static PlayerController _localPlayer;
    public static PlayerController LocalPlayer
    {
        get => _localPlayer;
    }

    // 로컬 플레이어가 생성됐을 때 호출되는 delegate
    private static Action _localPlayerCreated;
    public static Action LocalPlayerCreated
    {
        get => _localPlayerCreated;
        set => _localPlayerCreated = value;
    }

    // 플레이어 색깔
    private ColorType _color;
    public ColorType Color
    {
        get => _color;
    }

    private Vector3 _moveInput;
    public Vector3 MoveInput
    {
        get => _moveInput;
    }

    private Vector3 _velocity;
    public Vector3 Velocity
    {
        get => _velocity;
    }

    private Vector3 _angularVelocity;
    public Vector3 AngularVelocity
    {
        get => _angularVelocity;
    }

    private bool _isGrounded;
    public bool IsGrounded
    {
        get => _isGrounded;
    }

    private bool _isJumping;
    public bool IsJumping
    {
        get => _isJumping;
    }

    public override void OnNetworkSpawn()
    {
        _collider = GetComponent<Collider>();
        _playerRenderer = GetComponent<PlayerRenderer>();

        if (IsOwner)
        {
            _rigidbody = GetComponent<Rigidbody>();
            _cameraController = GetComponent<CameraController>();
            _networkPlatformFinder = GetComponent<NetworkPlatformFinder>();

            InputHandler.Instance.OnMove += OnMoveInput;
            InputHandler.Instance.OnJump += OnJumpInput;
            InputHandler.Instance.OnInteraction += OnInteractionInput;

            // 색깔 배정
            _color = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerConfig>().IsBlue ? ColorType.Blue : ColorType.Red;
            _playerRenderer.Initialize();

            // 스폰 위치 배정
            Transform spawnPoint;

            if (_color == ColorType.Blue)
            {
                spawnPoint = GameObject.FindWithTag("Blue Spawn Point")?.transform;
            }
            else
            {
                spawnPoint = GameObject.FindWithTag("Red Spawn Point")?.transform;
            }

            if (spawnPoint != null)
            {
                _rigidbody.MovePosition(spawnPoint.position);
                _rigidbody.MoveRotation(spawnPoint.rotation);

                GetComponent<NetworkInterpolator>().SetInstantTransform(spawnPoint.position, spawnPoint.rotation);
            }

            _localPlayer = this;
            _localPlayerCreated?.Invoke();
            
            UIManager.Instance.CloseAllOpenUI();
        }
        else
        {
            // 상대 색깔 요청
            if (IsServer)
            {
                RequestPlayerColorClientRpc();
            }
            else
            {
                RequestPlayerColorServerRpc();
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.None;
            BaseUIData baseUIData = new BaseUIData();
            UIManager.Instance.OpenUI<LoadingUI>(baseUIData);
        }

        base.OnNetworkDespawn();
    }   

    private void Update()
    {
        if (IsOwner)
        {
            UpdatePlayerState();
            CheckGrounded();

            if (_isEnabled)
            {
                HandleMovement();
                HandleJump();
                SearchInteractables();
            }

            HandlePlatform();
            SendPlayerState();

            // !TEST
            if (Input.GetKeyDown(KeyCode.C))
            {
                _cameraController.ChangeCameraMode(!_cameraController.IsFirstPerson);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 30;
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                QualitySettings.vSyncCount = 1;
                Application.targetFrameRate = 0;
            }
        }
    }

    private new void OnDestroy()
    {
        if (IsOwner)
        {
            InputHandler.Instance.OnMove -= OnMoveInput;
            InputHandler.Instance.OnJump -= OnJumpInput;
            InputHandler.Instance.OnInteraction -= OnInteractionInput;

            _localPlayerCreated = null;
        }

        base.OnDestroy();
    }

    /// <summary>
    /// 플레이어의 상태를 갱신한다.
    /// </summary>
    private void UpdatePlayerState()
    {
        _velocity = (transform.position - _lastPosition) / Time.deltaTime;
        _lastPosition = transform.position;

        _angularVelocity = (transform.rotation * Quaternion.Inverse(_lastRotation)).eulerAngles;

        if (_angularVelocity.x > 180f)
        {
            _angularVelocity.x -= 360f;
        }

        if (_angularVelocity.y > 180f)
        {
            _angularVelocity.y -= 360f;
        }

        if (_angularVelocity.z > 180f)
        {
            _angularVelocity.z -= 360f;
        }

        _angularVelocity /= Time.deltaTime;
        _lastRotation = transform.rotation;
    }

    /// <summary>
    /// X, Z 축 입력을 처리한다.
    /// </summary>
    private void HandleMovement()
    {
        Quaternion rotation = Quaternion.Euler(Vector3.up * Camera.main.transform.rotation.eulerAngles.y);

        if (_cameraController.IsFirstPerson)
        {
            Vector3 newVelocity = rotation * _moveInput * _walkSpeed;
            newVelocity.y = _rigidbody.velocity.y;

            _rigidbody.velocity = newVelocity;
        }
        else
        {
            Vector3 newVelocity = rotation * _moveInput * _walkSpeed;
            newVelocity.y = _rigidbody.velocity.y;

            _rigidbody.velocity = newVelocity;

            if (_moveInput.magnitude > 0f)
            {
                _rigidbody.MoveRotation(Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 8));
            }
        }
    }

    /// <summary>
    /// Y 축 이동 및 점프 입력을 처리한다.
    /// </summary>
    private void HandleJump()
    {
        _jumpRemember -= Time.deltaTime;

        if (_isGrounded)
        {
            if (_jumpInput)
            {
                if (_jumpRemember > 0f)
                {
                    _isJumping = true;
                    _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
                }

                _jumpInput = false;
            }

            if (_isJumping && _velocity.y < 0f)
            {
                _isJumping = false;
            }
        }
    }

    /// <summary>
    /// 플레이어가 보고 있는 Interactable을 탐색한다.
    /// </summary>
    private void SearchInteractables()
    {
        if (_interactableInHand != null)
        {
            return;
        }

        int originalLayer = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, MAXIMUM_REACH_DISTANCE) &&
            hit.collider.gameObject.TryGetComponent<IInteractable>(out IInteractable interactable) &&
            interactable.IsInteractable(this))
        {
            if (_interactableOnPointer != interactable)
            {
                if (_interactableOnPointer != null)
                {
                    _interactableOnPointer.Outline.enabled = false;
                }

                _interactableOnPointer = interactable;

                if (_interactableOnPointer.Outline)
                {
                    _interactableOnPointer.Outline.enabled = true;
                }
            }
        }
        else if (_interactableOnPointer != null)
        {
            if (_interactableOnPointer.Outline)
            {
                _interactableOnPointer.Outline.enabled = false;
            }

            _interactableOnPointer = null;
        }

        gameObject.layer = originalLayer;
    }

    /// <summary>
    /// 플레이어가 올라가 있는 플랫폼을 처리한다.
    /// </summary>
    private void HandlePlatform()
    {
        if (_networkPlatformFinder.Platform)
        {
            Vector3 velocityDiff = _networkPlatformFinder.Velocity;
            velocityDiff.y = 0f;

            _rigidbody.velocity += velocityDiff;
        }
    }

    /// <summary>
    /// 접지 여부를 판단한다.
    /// </summary>
    private void CheckGrounded()
    {
        if (_collider is CapsuleCollider)
        {
            Vector3 offset = Vector3.up * (_collider.bounds.extents.y - _collider.bounds.extents.x) * 0.9f;
            _isGrounded = Physics.CapsuleCast(transform.position + offset, transform.position - offset, _collider.bounds.extents.x, Vector3.down, GROUND_DETECTION_THRESHOLD);
        }
        else
        {
            _isGrounded = Physics.BoxCast(transform.position, _collider.bounds.extents * 0.9f, Vector3.down, transform.rotation, GROUND_DETECTION_THRESHOLD);
        }
    }

    /// <summary>
    /// 플레이어의 현재 상태를 상대에게 보낸다.
    /// </summary>
    private void SendPlayerState()
    {
        if (IsServer)
        {
            SendPlayerStateClientRpc(_moveInput, _velocity, _angularVelocity, _isJumping, _isGrounded);
        }
        else
        {
            SendPlayerStateServerRpc(_moveInput, _velocity, _angularVelocity, _isJumping, _isGrounded);
        }
    }

    /// <summary>
    /// X, Z 축 입력을 받는 Callback.
    /// </summary>
    /// <param name="value">입력 값</param>
    void OnMoveInput(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();

        _moveInput = new Vector3(input.x, 0, input.y).normalized;
    }

    /// <summary>
    /// 점프 입력을 받는 Callback.
    /// </summary>
    void OnJumpInput()
    {
        _jumpInput = true;
        _jumpRemember = JUMP_REMEMBER_TIME;
    }

    /// <summary>
    /// 상호작용 입력을 받는 Callback.
    /// </summary>
    void OnInteractionInput()
    {
        if (_interactableInHand != null)
        {
            if (_interactableInHand.StopInteraction(this))
            {
                _interactableOnPointer = null;
                _interactableInHand = null;
            }
        }
        else if (_interactableOnPointer != null)
        {
            if (_interactableOnPointer.StartInteraction(this))
            {
                _interactableOnPointer.Outline.enabled = false;
                _interactableInHand = _interactableOnPointer;
                _interactableOnPointer = null;
            }
        }
    }

    /// <summary>
    /// 서버 측에서 클라이언트에게 상대의 색깔을 요청한다.
    /// </summary>
    [ClientRpc(RequireOwnership = false)]
    private void RequestPlayerColorClientRpc()
    {
        if (IsServer)
        {
            return;
        }

        SendPlayerColorServerRpc(_color);
    }

    /// <summary>
    /// 클라이언트 측에서 서버에게 상대의 색깔을 요청한다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void RequestPlayerColorServerRpc()
    {
        SendPlayerColorClientRpc(_color);
    }

    /// <summary>
    /// 서버에서 클라이언트에게 자신의 색깔을 전달한다.
    /// </summary>
    /// <param name="color">색깔</param>
    [ClientRpc]
    private void SendPlayerColorClientRpc(ColorType color)
    {
        _color = color;
        _playerRenderer.Initialize();
    }

    /// <summary>
    /// 클라이언트에서 서버에게 자신의 색깔을 전달한다.
    /// </summary>
    /// <param name="color">색깔</param>
    [ServerRpc(RequireOwnership = false)]
    private void SendPlayerColorServerRpc(ColorType color)
    {
        _color = color;
        _playerRenderer.Initialize();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendPlayerStateServerRpc(Vector3 moveInput, Vector3 velocity, Vector3 angularVelocity, bool isJumping, bool isGrounded)
    {
        _moveInput = moveInput;
        _velocity = velocity;
        _angularVelocity = angularVelocity;

        _isJumping = isJumping;
        _isGrounded = isGrounded;
    }

    [ClientRpc(RequireOwnership = false)]
    private void SendPlayerStateClientRpc(Vector3 moveInput, Vector3 velocity, Vector3 angularVelocity, bool isJumping, bool isGrounded)
    {
        if (IsServer)
        {
            return;
        }

        _moveInput = moveInput;
        _velocity = velocity;
        _angularVelocity = angularVelocity;

        _isJumping = isJumping;
        _isGrounded = isGrounded;
    }

    /// <summary>
    /// 플레이어의 Collider 정보를 갱신한다.
    /// </summary>
    /// <param name="newCollider">새로운 Collider</param>
    /// <param name="colliderScale">새로운 Collider의 Local Scale</param>
    public void UpdateCollider(Collider newCollider, Vector3 colliderScale)
    {
        Destroy(_collider);

        if (newCollider == null)
        {
            _collider = gameObject.AddComponent<CapsuleCollider>();
            (_collider as CapsuleCollider).height = INITIAL_CAPSULE_HEIGHT;
            (_collider as CapsuleCollider).radius = INITIAL_CAPSULE_RADIUS;
        }
        else if (newCollider is BoxCollider)
        {
            Vector3 newSize = (newCollider as BoxCollider).size;
            newSize.x = newSize.x * colliderScale.x / transform.localScale.x;
            newSize.y = newSize.y * colliderScale.y / transform.localScale.y;
            newSize.z = newSize.z * colliderScale.z / transform.localScale.z;

            _collider = gameObject.AddComponent<BoxCollider>();
            (_collider as BoxCollider).size = newSize;
        }
        else if (newCollider is MeshCollider)
        {
            Mesh mesh = new Mesh
            {
                vertices = (newCollider as MeshCollider).sharedMesh.vertices,
                normals = (newCollider as MeshCollider).sharedMesh.normals,
                triangles = (newCollider as MeshCollider).sharedMesh.triangles
            };

            Vector3[] vertices = mesh.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].x = vertices[i].x * colliderScale.x / transform.localScale.x;
                vertices[i].y = vertices[i].y * colliderScale.y / transform.localScale.y;
                vertices[i].z = vertices[i].z * colliderScale.z / transform.localScale.z;
            }

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            _collider = gameObject.AddComponent<MeshCollider>();
            (_collider as MeshCollider).sharedMesh = mesh;
        }
    }

    /// <summary>
    /// 현재 플레이어와 물체의 상호 작용을 강제 중단한다.
    /// </summary>
    public void ForceStopInteraction()
    {
        if (_interactableInHand != null)
        {
            if (_interactableInHand.StopInteraction(this))
            {
                _interactableOnPointer = null;
                _interactableInHand = null;
            }
        }
    }

    public void EnableInput()
    {
        _isEnabled = true;
        _cameraController.EnableInput();
    }

    public void DisableInput()
    {
        _isEnabled = false;
        _cameraController.DisableInput();
    }
}
