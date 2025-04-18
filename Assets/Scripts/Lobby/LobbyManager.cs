using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif
using UnityEngine;
    
public class LobbyManager : NetworkSingletonBehaviour<LobbyManager>
{
    [SerializeField] private GameObject[] _playerPrefabs = new GameObject[2];
    [SerializeField] private Transform[] _spawnPoints = new Transform[2];
    [SerializeField] private AirlockController[] _airlockControllers = new AirlockController[6];
    public LobbyUIController LobbyUIController {  get; private set; }

    [SerializeField] private NetworkObject _lobby;
    public NetworkObject Lobby
    {
        get => _lobby;
    }

    [SerializeField] private NetworkObject _stage;
    
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
        
        SceneLoadManager.Instance.LoadScene(SceneType.InGame.ToString(), true);
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

    [ClientRpc(RequireOwnership = false)]
    public void SpawnStageClientRpc(NetworkObjectReference stageObject, Vector3 targetPosition, Quaternion targetRotation)
    {
        if (IsServer)
        {
            return;
        }

        if (stageObject.TryGet(out NetworkObject networkObject))
        {
            networkObject.transform.position = targetPosition;
            networkObject.transform.rotation = targetRotation;

            _lobby.gameObject.SetActive(false);
            _stage = networkObject;
        }
    }

    public void SpawnStage(GameObject stagePrefab, Vector3 targetPosition, Quaternion targetRotation)
    {
        EventBus.Instance.ClearEventBus();

        GameObject stage = Instantiate(stagePrefab);
        _stage = stage.GetComponent<NetworkObject>();
        _stage.Spawn();

        Transform stageTransform = stage.transform;
        Transform stageChildTransform = GameObject.FindGameObjectWithTag("Airlock Enter Target").transform;

        stageTransform.rotation = targetRotation * Quaternion.Inverse(stageChildTransform.localRotation);
        stageTransform.position = targetPosition - (stageChildTransform.position - stageTransform.position);

        _lobby.gameObject.SetActive(false);

        stage.GetComponent<StageManager>().StartGame();

        SpawnStageClientRpc(_stage, stageTransform.position, stageTransform.rotation);
    }

    public void SpawnLobby(Vector3 targetPosition, Quaternion targetRotation)
    {
        _lobby.gameObject.SetActive(true);

        Transform lobbyChildTransform = GameObject.FindGameObjectWithTag("Airlock Exit Target").transform;

        _lobby.transform.rotation = targetRotation * Quaternion.Inverse(lobbyChildTransform.localRotation);
        _lobby.transform.position = targetPosition - (lobbyChildTransform.position - _lobby.transform.position);

        SpawnLobbyClientRpc(_lobby.transform.position, _lobby.transform.rotation);
    }

    [ClientRpc(RequireOwnership = false)]
    public void SpawnLobbyClientRpc(Vector3 targetPosition, Quaternion targetRotation)
    {
        if (IsServer && NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            return;
        }

        _lobby.gameObject.SetActive(true);

        _lobby.transform.position = targetPosition;
        _lobby.transform.rotation = targetRotation;

        _stage.gameObject.SetActive(false);
        SpawnLobbyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnLobbyServerRpc()
    {
        _stage.Despawn();
    }
}

