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
    private NetworkInterpolator _networkInterpolator;

    private GameObject _playerRender;
    public GameObject PlayerRender
    {
        get => _playerRender;
        set => _playerRender = value;
    }

    private PlayerRendererUtil _playerRendererUtil;
    public PlayerRendererUtil PlayerRendererUtil
    {
        get => _playerRendererUtil;
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
    private void SetHeadTargetServerRpc(Vector3 targetPosition)
    {
        _playerRendererUtil.SetHeadTarget(targetPosition);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SetHeadTargetClientRpc(Vector3 targetPosition)
    {
        if (IsServer)
        {
            return;
        }

        _playerRendererUtil.SetHeadTarget(targetPosition);
    }

    public void SetHeadTarget(Vector3 targetPosition)
    {
        _playerRendererUtil.SetHeadTarget(targetPosition);

        if (IsServer)
        {
            SetHeadTargetClientRpc(targetPosition);
        }
        else
        {
            SetHeadTargetServerRpc(targetPosition);
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
