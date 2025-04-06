using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 플레이어를 렌더링하는 Class
/// </summary>
public class PlayerRenderer : NetworkBehaviour
{
    /// <summary>
    /// 렌더링에 사용할 플레이어 프리팹. (파랑, 빨강 순)
    /// </summary>
    [SerializeField] private GameObject[] _playerRenderPrefab;

    private PlayerController _playerController;
    private PlayerRendererUtil _playerRendererUtil;
    private NetworkInterpolator _networkInterpolator;

    private GameObject _playerRender;
    public GameObject PlayerRender
    {
        get => _playerRender;
        set => _playerRender = value;
    }

    private MeshFilter _meshFilter;
    public MeshFilter MeshFilter
    {
        get => _meshFilter;
        set => _meshFilter = value;
    }

    private MeshRenderer _meshRenderer;
    public MeshRenderer MeshRenderer
    {
        get => _meshRenderer;
        set => _meshRenderer = value;
    }

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _networkInterpolator = GetComponent<NetworkInterpolator>();
    }

    public void Initialize()
    {
        _networkInterpolator.AddVisualReferenceDependantFunction(() =>
        {
            _meshFilter = _networkInterpolator.VisualReference.AddComponent<MeshFilter>();
            _meshRenderer = _networkInterpolator.VisualReference.AddComponent<MeshRenderer>();

            int colorIndex = (int)_playerController.Color - 1;

            _playerRender = Instantiate(_playerRenderPrefab[colorIndex]);
            _playerRender.transform.SetParent(_networkInterpolator.VisualReference.transform);

            _playerRender.transform.localPosition = new Vector3(0f, -0.95f, -0.125f);
            _playerRender.transform.localRotation = Quaternion.identity;
            _playerRender.transform.localScale = Vector3.one * 0.075f;

            _playerRendererUtil = _playerRender.GetComponent<PlayerRendererUtil>();
            _playerRendererUtil.SetPlayerController(_playerController);

            if (IsOwner)
            {
                _playerRendererUtil.HideFirstPersonPlayerRender();
            }
        });
    }

    /// <summary>
    /// 플레이어 모습을 표시한다.
    /// </summary>
    public void ShowPlayerRender()
    {
        _playerRendererUtil.ShowPlayerRender();
    }

    /// <summary>
    /// 플레이어 모습을 숨긴다.
    /// </summary>
    public void HidePlayerRender()
    {
        _playerRendererUtil.HidePlayerRender();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetHeadTargetServerRpc(Vector3 forward)
    {
        _playerRendererUtil.SetHeadTarget(forward);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SetHeadTargetClientRpc(Vector3 forward)
    {
        if (IsServer)
        {
            return;
        }

        _playerRendererUtil.SetHeadTarget(forward);
    }

    /// <summary>
    /// 플레이어 머리의 방향을 지정한다.
    /// </summary>
    /// <param name="targetPosition">방향 벡터</param>
    public void SetHeadTarget(Vector3 forward)
    {
        _playerRendererUtil.SetHeadTarget(forward);

        if (IsServer)
        {
            SetHeadTargetClientRpc(forward);
        }
        else
        {
            SetHeadTargetServerRpc(forward);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetArmTargetServerRpc(ArmType armType, Vector3 targetPosition)
    {
        _playerRendererUtil.SetArmTarget(armType, targetPosition);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SetArmTargetClientRpc(ArmType armType, Vector3 targetPosition)
    {
        if (IsServer)
        {
            return;
        }

        _playerRendererUtil.SetArmTarget(armType, targetPosition);
    }

    /// <summary>
    /// 플레이어 팔의 방향을 지정한다.
    /// </summary>
    /// <param name="armType">팔 종류</param>
    /// <param name="targetPosition">팔 위치</param>
    public void SetArmTarget(ArmType armType, Vector3 targetPosition)
    {
        _playerRendererUtil.SetArmTarget(armType, targetPosition);

        if (IsServer)
        {
            SetArmTargetClientRpc(armType, targetPosition);
        }
        else
        {
            SetArmTargetServerRpc(armType, targetPosition);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetArmWeightServerRpc(ArmType armType, float weight)
    {
        _playerRendererUtil.SetArmWeight(armType, weight);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SetArmWeightClientRpc(ArmType armType, float weight)
    {
        if (IsServer)
        {
            return;
        }

        _playerRendererUtil.SetArmWeight(armType, weight);
    }

    /// <summary>
    /// 플레이어 팔의 Weight를 지정한다. [0, 1]
    /// </summary>
    /// <param name="armType">팔 위치</param>
    /// <param name="weight">Weight</param>
    public void SetArmWeight(ArmType armType, float weight)
    {
        _playerRendererUtil.SetArmWeight(armType, weight);

        if (IsServer)
        {
            SetArmWeightClientRpc(armType, weight);
        }
        else
        {
            SetArmWeightServerRpc(armType, weight);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayTouchAnimationServerRpc(Vector3 touchPosition)
    {
        _playerRendererUtil.PlayTouchAnimation(touchPosition);
    }

    [ClientRpc(RequireOwnership = false)]
    private void PlayTouchAnimationClientRpc(Vector3 touchPosition)
    {
        if (IsServer)
        {
            return;
        }

        _playerRendererUtil.PlayTouchAnimation(touchPosition);
    }

    /// <summary>
    /// 플레이어가 오른팔로 특정 위치를 누르는 애니메이션을 재생한다.
    /// </summary>
    /// <param name="touchPosition">목표 위치</param>
    public void PlayTouchAnimation(Vector3 touchPosition)
    {
        _playerRendererUtil.PlayTouchAnimation(touchPosition);

        if (IsServer)
        {
            PlayTouchAnimationClientRpc(touchPosition);
        }
        else
        {
            PlayTouchAnimationServerRpc(touchPosition);
        }
    }
}
