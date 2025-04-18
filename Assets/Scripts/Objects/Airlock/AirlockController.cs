using Unity.Netcode;
using UnityEngine;

public class AirlockController : NetworkBehaviour
{
    [SerializeField] private bool _isEntrance;
    [SerializeField] private GameObject _stagePrefab;

    [SerializeField] private DoorController _doorIn;
    [SerializeField] private DoorController _doorOut;
    
    [Tooltip("0 : blue In Mesh, 1 : blue Out Mesh")]
    [SerializeField] private MeshRenderer[] _blueInOutMeshRenderers = new MeshRenderer[2];
    
    [Tooltip("0 : red In Mesh, 1 : red Out Mesh")]
    [SerializeField] private MeshRenderer[] _redInOutMeshRenderers = new MeshRenderer[2];

    private bool _isRedPressed = false;
    private bool _isBluePressed = false;

    private StageName _stageName;
    public StageName StageName
    {
        get => _stageName;
        set => _stageName = value;
    }

    private bool _isAirlockOpened;
    public bool IsAirlockOpened
    {
        get => _isAirlockOpened;
        set
        {
            _isAirlockOpened = value;
            _doorIn.IsOpened = value;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            RequestInitialStateServerRpc();
        }

        _blueInOutMeshRenderers[0].material.SetObjectColor(ColorType.None);
        _blueInOutMeshRenderers[1].material.SetObjectColor(ColorType.Blue);

        _redInOutMeshRenderers[0].material.SetObjectColor(ColorType.None);
        _redInOutMeshRenderers[1].material.SetObjectColor(ColorType.Red);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestInitialStateServerRpc()
    {
        _doorIn.IsOpened = _doorIn.IsOpened;
        _doorOut.IsOpened = _doorOut.IsOpened;
    }

    [ClientRpc(RequireOwnership = false)]

    private void UpdateInOutLightClientRpc(ColorType buttonColor, bool isIn)
    {
        if (buttonColor == ColorType.Blue)
        {
            _blueInOutMeshRenderers[0].material.SetObjectColor(isIn ? ColorType.Blue : ColorType.None);
            _blueInOutMeshRenderers[1].material.SetObjectColor(isIn ? ColorType.None : ColorType.Blue);
        }
        else
        {
            _redInOutMeshRenderers[0].material.SetObjectColor(isIn ? ColorType.Red : ColorType.None);
            _redInOutMeshRenderers[1].material.SetObjectColor(isIn ? ColorType.None : ColorType.Red);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnAirlockButtonPressedServerRpc(ColorType buttonColor, bool isIn)
    {
        if (buttonColor == ColorType.Blue)
        {
            _isBluePressed = isIn;
        }
        else
        {
            _isRedPressed = isIn;
        }

        UpdateInOutLightClientRpc(buttonColor, isIn);

        if (_isBluePressed & _isRedPressed)
        {
            if (_isEntrance)
            {
                _isBluePressed = false;
                _isRedPressed = false;

                UpdateInOutLightClientRpc(ColorType.Blue, false);
                UpdateInOutLightClientRpc(ColorType.Red, false);

                _doorIn.IsOpened = true;
                _doorOut.IsOpened = false;

                LobbyManager.Instance.SpawnStage(_stagePrefab, transform.position, transform.rotation);
            }
            else
            {
                LobbyManager.Instance.SpawnLobby(transform.position, transform.rotation);
            }
        }
        else if (_isBluePressed ^ _isRedPressed)
        {
            // 둘 다 잠근다
            _doorIn.IsOpened = false;
            _doorOut.IsOpened = false;
        }
        else
        {
            // 나간다
            _doorIn.IsOpened = true;
            _doorOut.IsOpened = false;
        }
    }
}
