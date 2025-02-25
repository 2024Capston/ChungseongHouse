using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 큐브를 조작하는 Class.
/// </summary>
public class CubeController : PlayerDependantBehaviour, IInteractable
{
    /// <summary>
    /// 큐브 색깔
    /// </summary>
    [SerializeField] private ColorType _color;
    public ColorType Color
    {
        get => _color;
        set => _color = value;
    }

    private const float DISTANCE_FROM_PLAYER = 32f;         // 플레이어와 큐브 사이의 거리
    private const float MAXIMUM_DISTANCE_FROM_PLAYER = 64f; // 플레이어와 큐브가 멀어질 수 있는 최대 거리
    private const float CUBE_SPEED = 64f;                   // 큐브의 이동 속력

    private Rigidbody _rigidbody;
    private CubeRenderer _cubeRenderer;
    private NetworkInterpolator _networkInterpolator;

    private PlayerController _interactingPlayer;    // 큐브를 들고 있는 플레이어
    private bool _isActive = true;

    /// <summary>
    /// 상호작용 여부
    /// </summary>
    private bool _isTaken;
    public bool IsTaken
    {
        get => _isTaken;
    }

    private Outline _outline;
    public Outline Outline
    {
        get => _outline;
        set => _outline = value;
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _cubeRenderer = GetComponent<CubeRenderer>();
        _networkInterpolator = GetComponent<NetworkInterpolator>();

        _networkInterpolator.AddVisualReferenceDependantFunction(() =>
        {
            _outline = _networkInterpolator.VisualReference.GetComponent<Outline>();
            _outline.enabled = false;
        });
    }

    public override void OnPlayerInitialized()
    {
        if (_color == PlayerController.LocalPlayer.Color && IsClient)
        {
            // 플레이어와 색깔이 같으면 Ownership 요청
            RequestOwnershipServerRpc(NetworkManager.LocalClientId);
        }
        else
        {
            _rigidbody.isKinematic = true;
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (_interactingPlayer)
        {
            Vector3 target = Camera.main.transform.position + Camera.main.transform.forward * DISTANCE_FROM_PLAYER;
            target = target - transform.position;

            _rigidbody.velocity = target.normalized * Mathf.Sqrt(target.magnitude) * CUBE_SPEED;

            if (Vector3.Distance(transform.position, _interactingPlayer.transform.position) > MAXIMUM_DISTANCE_FROM_PLAYER)
            {
                ForceStopInteraction();
            }
        }
    }

    public bool IsInteractable(PlayerController player)
    {
        return _color == player.Color && _isActive;
    }

    public bool StartInteraction(PlayerController player)
    {
        _interactingPlayer = player;
        _rigidbody.useGravity = false;

        SetTakenServerRpc(true);

        return true;
    }

    public bool StopInteraction(PlayerController player)
    {
        _interactingPlayer = null;
        _rigidbody.useGravity = true;

        SetTakenServerRpc(false);

        return true;
    }

    /// <summary>
    /// 색깔이 같은 큐브에 대해 서버에 Ownership을 요청한다.
    /// </summary>
    /// <param name="clientId">요청하는 플레이어 ID</param>
    [ServerRpc(RequireOwnership = false)]
    private void RequestOwnershipServerRpc(ulong clientId)
    {
        GetComponent<NetworkObject>().ChangeOwnership(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetTakenServerRpc(bool isTaken)
    {
        SetTakenClientRpc(isTaken);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SetTakenClientRpc(bool isTaken)
    {
        _isTaken = isTaken;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeColorServerRpc(ColorType color)
    {
        ChangeColorClientRpc(color);
    }

    [ClientRpc(RequireOwnership = false)]
    private void ChangeColorClientRpc(ColorType color)
    {
        _color = color;

        if (IsOwner && _interactingPlayer)
        {
            _interactingPlayer.ForceStopInteraction();
        }

        if (_color == PlayerController.LocalPlayer.Color)
        {
            RequestOwnershipServerRpc(NetworkManager.LocalClientId);
            _rigidbody.isKinematic = false;
        }
        else
        {
            _rigidbody.isKinematic = true;
        }

        _rigidbody.useGravity = true;

        _cubeRenderer.UpdateColor();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetActiveServerRpc(bool isActive)
    {
        SetActiveClientRpc(isActive);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SetActiveClientRpc(bool isActive)
    {
        _isActive = isActive;
    }

    [ClientRpc(RequireOwnership = false)]
    private void ForceStopInteractionClientRpc()
    {
        _isTaken = false;

        if (_interactingPlayer)
        {
            _interactingPlayer.ForceStopInteraction();
        }
    }

    /// <summary>
    /// 큐브의 색깔 정보를 서버와 클라이언트에서 변경한다.
    /// </summary>
    /// <param name="color">새 색깔</param>
    public void ChangeColor(ColorType color)
    {
        ChangeColorServerRpc(color);
    }

    /// <summary>
    /// 큐브의 활성화 여부를 서버와 클라이언트에서 변경한다.
    /// </summary>
    /// <param name="isActive">활성화 여부</param>
    public void SetActive(bool isActive)
    {
        SetActiveServerRpc(isActive);
    }

    /// <summary>
    /// 큐브와 상호작용 중인 플레이어가 있다면 강제 중단한다.
    /// </summary>
    public void ForceStopInteraction()
    {
        ForceStopInteractionClientRpc();
    }
}
