using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class InGameManager : NetworkSingletonBehaviour<InGameManager>
{
    /// <summary>
    /// InGameManager의 상태 Enum
    /// </summary>
    private enum InGameState
    {
        Init, Progress, Reset, End
    }

    private InGameState _inGameState;
    
    private Transform _blueSpawnPoint;
    private Transform _redSpawnPoint;
    
    [SerializeField] private GameObject[] _playerPrefabs = new GameObject[2];
    
    public StageLoader StageLoader { get; set; }
    public StageManager StageManager { get; set; }
    
    protected override void Init()
    {
        _isDestroyOnLoad = true;
        base.Init();

        StageLoader = null;
        StageManager = null;
        _inGameState = InGameState.Init;
    }

    /// <summary>
    /// 선택한 스테이지의 Loader를 생성합니다.
    /// Loader를 생성하다 오류가 발생하면 LobbyScene으로 돌아갑니다.
    /// </summary>
    private void Start()
    {
        base.OnNetworkSpawn();
        if (!IsHost)
        {
            return;
        }

        _redSpawnPoint = null;
        _blueSpawnPoint = null;

        try
        {
            // 선택한 스테이지를 가져옵니다. 스테이지 값이 이상하면 예외가 발생합니다.
            StageName stageName = SessionManager.Instance.SelectedStage;
            if (stageName == StageName.Size)    
            {
                throw new Exception("StageName MissMatch");
            }   

            // 해당 스테이지의 Loader를 만듭니다.
            StageLoader = StageLoadManager.Instance.LoadLoader(stageName);
            Logger.Log("StageLoader Generated");
            
            // Loader를 이용해 Stage를 Load합니다.
            StageLoader.LoadStage();
        }
        catch (Exception e)
        {
            StageLoadFailed(e);
        }
        
    }

    /// <summary>
    /// 게임이 시작하면 SpawnPoint를 찾고 해당 위치에 플레이어를 Spawn합니다. 이후 해당 스테이지 매니저에서 게임 처리를 하면 됩니다.
    /// 중간 과정에서 예외 발생 시 생성한 모든 Object를 파괴하고 Lobby로 되돌아 갑니다.
    /// </summary>
    public void StartGame()
    {
        try
        {
            FindSpawnPoint();

            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                SpawnPlayer(clientId);
            }
            
            UIManager.Instance.CloseAllOpenUI();
            _inGameState = InGameState.Progress;
            
            // StageManager를 상속받은 스크립트가 Loader에 포함되어 있어야 정상작동합니다.
            StageManager.Instance.StartGame();

            // 테스트용으로 일정 시간 후에 Lobby로 복귀
            StartCoroutine(CoTest());
        }
        catch (Exception e)
        {
            DestoryAllObjects();
            StageLoadFailed(e);
        }
    }

    private IEnumerator CoTest()
    {
        yield return new WaitForSeconds(60f);
        // EndGameServerRpc();
    }
    
    /// <summary>
    /// Tag를 이용해 SpawnPoint를 탐색합니다.
    /// </summary>
    private void FindSpawnPoint()
    {
        _blueSpawnPoint = GameObject.FindWithTag("Blue Spawn Point").transform;
        _redSpawnPoint = GameObject.FindWithTag("Red Spawn Point").transform;
    }
    
    /// <summary>
    /// PlayerConfig를 참고해 지정된 위치에 Player를 Spawn합니다.
    /// </summary>
    /// <param name="clientId"></param>
    private void SpawnPlayer(ulong clientId)
    {
        PlayerConfig playerConfig = NetworkManager.Singleton.SpawnManager
            .GetPlayerNetworkObject(clientId).GetComponent<PlayerConfig>();
        int isBlue = playerConfig.IsBlue ? 1 : 0;
        GameObject player = Instantiate(_playerPrefabs[isBlue]);
        
        player.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        PlayerController playerController = player.GetComponent<PlayerController>();
        playerConfig.MyPlayer = playerController;
        //playerController.PlayerColor = playerConfig.IsBlue ? ColorType.Blue : ColorType.Red;
    }
    
    /// <summary>
    /// 게임이 종료되었으면 LobbyScene으로 되돌아갑니다. Host만 호출할 수 있습니다.
    /// </summary>
    [ServerRpc]
    public void EndGameServerRpc()
    {
        // ServerRpc의 중복 호출을 방지하기 위해 이미 종료 단계일 땐 처리하지 않는다.
        if (_inGameState == InGameState.End)
        {
            return;
        }

        _inGameState = InGameState.End;
        
        DestoryAllObjects();
        SceneLoadManager.Instance.LoadScene(SceneType.Lobby.ToString(), true);
    }

    /// <summary>
    /// Spawn된 모든 Network Object들을 Despawn합니다.
    /// </summary>
    private void DestoryAllObjects()
    {
        foreach (ulong playerId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            PlayerConfig playerConfig = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerId).GetComponent<PlayerConfig>();
            if (playerConfig.MyPlayer is not null)
            {
                playerConfig.MyPlayer.GetComponent<NetworkObject>().Despawn();
                playerConfig.MyPlayer = null;
            }   
            
        }
        
        StageLoader.DestoryStage();
        StageLoader.GetComponent<NetworkObject>().Despawn();
    }

    /// <summary>
    /// StageLoad를 실패했을 경우 Lobby Scene으로 되돌아갑니다.
    /// </summary>
    private void StageLoadFailed(Exception e)
    {
        _inGameState = InGameState.End;
        Logger.LogError($"StageLoad Failed\n{e}");
        SceneLoadManager.Instance.LoadScene(SceneType.Lobby.ToString(), true);
        Logger.Log($"Scene Transition {SceneManager.GetActiveScene().name} to {SceneType.Lobby}");
    }
}
