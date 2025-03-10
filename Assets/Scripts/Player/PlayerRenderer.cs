using Unity.Netcode;
using UnityEngine;

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

            _playerRender.transform.localPosition = Vector3.zero;
            _playerRender.transform.localRotation = Quaternion.identity;
            _playerRender.transform.localScale = Vector3.one;
        });
    }

    /// <summary>
    /// 플레이어 모습을 표시한다.
    /// </summary>
    public void ShowPlayerRender()
    {
        _playerRender.SetActive(true);
    }

    /// <summary>
    /// 플레이어 모습을 숨긴다.
    /// </summary>
    public void HidePlayerRender()
    {
        _playerRender.SetActive(false);
    }
}
