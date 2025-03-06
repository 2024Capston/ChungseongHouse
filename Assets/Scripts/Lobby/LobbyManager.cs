using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class LobbyManager : NetworkSingletonBehaviour<LobbyManager>
{
    [SerializeField] private GameObject[] _playerPrefabs = new GameObject[2];
    [SerializeField] private Transform[] _spawnPoints = new Transform[2];
    [SerializeField] private AirlockController[] _airlockControllers = new AirlockController[6];
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
    }

    private void Start()
    {
        SetMapDataServerRpc();
        SpawnPlayerServerRpc();
        UIManager.Instance.CloseAllOpenUI();
    }

    /// <summary>
    /// 엘리베이터에서 층을 이동할 때 호출됩니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void RequestMoveFloorServerRpc(int floor)
    {
        
    }
    
    /// <summary>
    /// 현재 선택한 Stage를 저장하고 InGame Scene으로 이동합니다.
    /// </summary>
    /// <param name="stageName"></param>
    public void RequestTrasitionInGameScene(StageName stageName)
    {
        // InGame Scene으로 이동하면 ConnectionManager에 있는 SelectStage로 Loader를 불러온다.
        SessionManager.Instance.SelectedStage = stageName;
        
        // 현재 Player는 Despawn합니다.
        foreach (ulong playerId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            PlayerConfig playerConfig = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerId).GetComponent<PlayerConfig>();
            if (playerConfig.MyPlayer is not null)
            {
                playerConfig.MyPlayer.GetComponent<NetworkObject>().Despawn();
                playerConfig.MyPlayer = null;
            }
            
        }
        
        SceneLoader.Instance.LoadScene(SceneType.InGame.ToString(), true);
    }
    
    /// <summary>
    /// PlayerConfig를 참고하여 Player를 스폰합니다.
    /// </summary>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        PlayerConfig playerConfig = NetworkManager.Singleton.SpawnManager
            .GetPlayerNetworkObject(serverRpcParams.Receive.SenderClientId).GetComponent<PlayerConfig>();
        int isBlue = playerConfig.IsBlue ? 1 : 0;
        GameObject player = Instantiate(_playerPrefabs[isBlue]);
    /*  player.transform.position = _spawnPoints[isBlue].position;
        player.transform.rotation = _spawnPoints[isBlue].rotation;*/
        
        player.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
        PlayerController playerController = player.GetComponent<PlayerController>();
        playerConfig.MyPlayer = playerController;
        //playerController.PlayerColor = playerConfig.IsBlue ? ColorType.Blue : ColorType.Red;
    }
    
    /// <summary>
    /// 각 Airlock에 Stage를 할당하고, 해당 Stage Data에 맞게 Airlock 문을 개방합니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void SetMapDataServerRpc(ServerRpcParams serverRpcParams = default)
    {
        PlayData data = SessionManager.Instance.SelectedData;
        
        // 데이터 세팅을 요청한 ClientId를 저장합니다.
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { serverRpcParams.Receive.SenderClientId }
            }
        };
        
        // 데이터를 요청한 Client에서 뿌려서 Airlock를 세팅합니다.
        for (int i = 0; i < 6; i++)
        {
            int index = (SessionManager.Instance.CurrentFloor - 1) * 6 + i;
            
            SetAirlockDataClientRpc(i, (StageName)index, data.MapInfoList[index].ClearFlag != 0, clientRpcParams);
        }
    }

    /// <summary>
    /// AirLock의 StageName과 개방 여부를 세팅합니다.
    /// </summary>
    /// <param name="index">Airlock 인덱스</param>
    /// <param name="stageName">StageName</param>
    /// <param name="isAirlockOpened">Stage 개방 여부</param>
    /// <param name="clientRpcParams">세팅 할 Client</param>
    [ClientRpc]
    private void SetAirlockDataClientRpc(int index, StageName stageName, bool isAirlockOpened, ClientRpcParams clientRpcParams = default)
    {
        _airlockControllers[index].StageName = stageName;
        _airlockControllers[index].IsAirlockOpened = isAirlockOpened;
    }
}

