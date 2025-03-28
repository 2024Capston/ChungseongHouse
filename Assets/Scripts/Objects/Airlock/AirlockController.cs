using Unity.Netcode;
using UnityEngine;

public class AirlockController : NetworkBehaviour
{
    private enum AirlockState
    {
        Idle, SceneTransition
    }

    private AirlockState _airlockState;
    
    [SerializeField] private DoorController _doorIn;
    [SerializeField] private DoorController _doorOut;
    
    [Tooltip("0 : blue In Mesh, 1 : blue Out Mesh")]
    [SerializeField] private MeshRenderer[] _blueInOutMeshRenderers = new MeshRenderer[2];
    
    [Tooltip("0 : red In Mesh, 1 : red Out Mesh")]
    [SerializeField] private MeshRenderer[] _redInOutMeshRenderers = new MeshRenderer[2];
    
    // 두 값이 true일 땐 DoorIn이 개방, 두 값이 false일 땐 DoorOut이 개방
    private bool _isBlueOpened = true;
    private bool IsBlueOpened
    {
        get => _isBlueOpened;
        set
        {
            _isBlueOpened = value;
            SetButtonMaterialClientRpc(ColorType.Blue, _isBlueOpened);
        }
        
    }
    private bool _isRedOpened = true;

    private bool IsRedOpened
    {
        get => _isRedOpened;
        set
        {
            _isRedOpened = value;
            SetButtonMaterialClientRpc(ColorType.Red, _isRedOpened);
        }
    }
    
    
    private bool _isInOpened;
    /// <summary>
    /// true일 때 Lobby쪽 문이 열려있다.
    /// </summary>
    private bool IsInOpened
    {
        get => _isInOpened;
        set
        {
            _isInOpened = value;
            _doorIn.IsOpened = _isInOpened && IsAirlockOpened;
            _doorOut.IsOpened = !_isInOpened && IsAirlockOpened;
        }
    }

    private bool _isAirlockOpened;
    /// <summary>
    /// true일 때 스테이지가 해금 되어 해당 문에 접근할 수 있다.
    /// </summary>
    public bool IsAirlockOpened
    {
        get => _isAirlockOpened;
        set
        {
            _isAirlockOpened = value;
            _doorIn.IsOpened = _isAirlockOpened && _isInOpened;
            _doorOut.IsOpened = _isAirlockOpened && !_isInOpened;
        }
    }
    [field: SerializeField]
    public StageName StageName { get; set; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        StageName = StageName.Size;
        IsAirlockOpened = true;
        IsInOpened = true;
        _airlockState = AirlockState.Idle;
        
        // Init
        OnClickAirlockButtonServerRpc(ColorType.Blue, true);
        OnClickAirlockButtonServerRpc(ColorType.Red, true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnClickAirlockButtonServerRpc(ColorType colorType, bool isInButton)
    {
        if (colorType == ColorType.Blue)
        {
            IsBlueOpened = isInButton;
        }
        else if (colorType == ColorType.Red)
        {
            IsRedOpened = isInButton;
        }

        // 일치하면 문 개방 방향이 변경된다.
        if (IsBlueOpened == IsRedOpened)
        {
            IsInOpened = IsBlueOpened;
        }
    }

    [ClientRpc]
    private void SetButtonMaterialClientRpc(ColorType colorType, bool isInButton)
    {
        if (colorType == ColorType.Blue)
        {
            _blueInOutMeshRenderers[isInButton ? 0 : 1].material.SetObjectColor(ColorType.Blue);
            _blueInOutMeshRenderers[isInButton ? 1 : 0].material.SetObjectColor(ColorType.None);
        }
        else
        {
            _redInOutMeshRenderers[isInButton ? 0 : 1].material.SetObjectColor(ColorType.Red);
            _redInOutMeshRenderers[isInButton ? 1 : 0].material.SetObjectColor(ColorType.None);
        }
    }

    /// <summary>
    /// InGame Scene으로 넘어갈 수 있는 경우 LobbyManager에 InGame Scene으로 전환을 요청합니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void RequestTransitionInGameSceneServerRpc()
    {
        // 아직 개방되지 않은 스테이지이거나, 아직 문이 열리지 않았거나, 이미 전환 중일때는 처리하지 않는다.
        if (!IsAirlockOpened || IsInOpened || _airlockState == AirlockState.SceneTransition)
        {
            return;
        }

        _airlockState = AirlockState.SceneTransition;
        LobbyManager.Instance.RequestTrasitionInGameScene(StageName);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        RequestTransitionInGameSceneServerRpc();
    }
}
