using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkSingletonBehaviour<LobbyManager>
{
    [SerializeField] private GameObject[] _playerPrefabs = new GameObject[2];
    [SerializeField] private Transform[] _spawnPoints = new Transform[2];
    public LobbyUIController LobbyUIController {  get; private set; }
    
    protected override void Init()
    {
        _isDestroyOnLoad = true;
        base.Init();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        LobbyUIController = FindObjectOfType<LobbyUIController>();

        if (!LobbyUIController)
        {
            Logger.LogError("LobbyUIManager does not exist");
            return;
        }

        LobbyUIController.SetPlayerColorData(IsHost);
        
        SpawnPlayerServerRpc();

        
        UIManager.Instance.CloseAllOpenUI();
    }

    /// <summary>
    /// Stage 문을 열때 호출하는 메소드
    /// </summary>
    /// <param name="stageName"></param>
    [ServerRpc(RequireOwnership = false)]
    public void RequestOpenDoorServerRpc(StageName stageName)
    {
        
    }

    /// <summary>
    /// Elevator 문을 열때 호출하는 메소드
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void RequestOpenElevatorServerRpc()
    {
        
    }

    /// <summary>
    /// 엘리베이터에서 층을 이동할 때 호출하는 메소드
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void RequestMoveFloorServerRpc(int floor)
    {
        
    }
    
    /// <summary>
    /// InGame으로 이동하는 메소드 
    /// </summary>
    /// <param name="stageName"></param>
    [ServerRpc(RequireOwnership = false)]
    public void RequestPlayStageServerRpc(StageName stageName)
    {
        // InGame Scene으로 이동하면 ConnectionManager에 있는 SelectStage로 Loader를 불러온다.
        SessionManager.Instance.SelectedStage = stageName;
        NetworkManager.SceneManager.LoadScene(SceneType.InGame.ToString(), UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    
    /// <summary>
    /// PlayerConfig를 참고하여 
    /// </summary>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        PlayerConfig playerConfig = NetworkManager.Singleton.SpawnManager
            .GetPlayerNetworkObject(serverRpcParams.Receive.SenderClientId).GetComponent<PlayerConfig>();
        int isBlue = playerConfig.IsBlue ? 1 : 0;
        GameObject player = Instantiate(_playerPrefabs[isBlue]);
    /*    player.transform.position = _spawnPoints[isBlue].position;
        player.transform.rotation = _spawnPoints[isBlue].rotation;*/
        
        player.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
        PlayerController playerController = player.GetComponent<PlayerController>();
        playerConfig.MyPlayer = playerController;
        //playerController.PlayerColor = playerConfig.IsBlue ? ColorType.Blue : ColorType.Red;
    }

    [ClientRpc]
    private void SetMapDataClientRpc()
    {
        
    }
}
