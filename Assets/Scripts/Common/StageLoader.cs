using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Stage들을 Load하는 Class
/// </summary>
public class StageLoader : NetworkBehaviour
{
    private int _clientLoadCount;
    
    [SerializeField] private List<GameObject> _networkObjects;

    [SerializeField] private List<GameObject> _localObjects;

    private List<NetworkObject> _instantiatedNetworkObjects;

    private List<GameObject> _instantiatedLocalObjects;
    
    /// <summary>
    /// 해당 Stage에 필요한 NetworkObject와 LocalObject를 모두 Load
    /// </summary>
    public void LoadStage()
    {
        Init();
        
        // NetworkObject는 모두 서버에서 Spawn
        LoadAllNetworkObjects();
        
        // Local Object는 Client에 개별적으로 Load 한다.
        LoadAllLocalObjectClientRpc(); 
    }

    /// <summary>
    /// LoadStage를 통해 생성된 모든 Object들을 Destory
    /// </summary>
    public void DestoryStage()
    {   
        DestroyAllNetworkObject();
        DestoryAllLocalObjectClientRpc();
    }
    
    private void Init()
    {
        _clientLoadCount = 0;
        _instantiatedLocalObjects = new List<GameObject>();
        _instantiatedNetworkObjects = new List<NetworkObject>();
    }

    /// <summary>
    /// _networkObject List에 저장된 모든 NetworkObject를 서버 상에서 Spawn 하는 메소드
    /// </summary>
    private void LoadAllNetworkObjects()
    {
        if (_networkObjects.Count == 0)
        {
            return;
        }
        
        foreach (GameObject networkObject in _networkObjects)
        {
            NetworkObject instantiatedNetworkObject = Instantiate(networkObject).GetComponent<NetworkObject>();

            if (!instantiatedNetworkObject)
            {
                Logger.LogError($"{networkObject.name} dose not have a NetworkObject Component.");
                
                // TODO
                // 예외 처리 로직이 필요합니다.
                // ex) 이전 맵으로 돌아가던가 하는?
                continue;
            }
            
            _instantiatedNetworkObjects.Add(instantiatedNetworkObject);
            instantiatedNetworkObject.Spawn();
        }
    }
    
    /// <summary>
    /// _localObject List에 저장된 모든 Object를 각 클라이언트에서 Spawn 하는 메소드
    /// </summary>
    [ClientRpc]
    private void LoadAllLocalObjectClientRpc()
    {
        if (IsClient)
        {
            _instantiatedLocalObjects = new List<GameObject>();
        }

        if (_localObjects.Count != 0)
        {
            foreach (GameObject localObject in _localObjects)
            {
                GameObject instantiateLocalObject = Instantiate(localObject);
                _instantiatedLocalObjects.Add(instantiateLocalObject);
            }
        }
        
        CompleteLoadAllLocalObjectServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void CompleteLoadAllLocalObjectServerRpc()
    {
        // 혼자 테스트가 필요할 땐 1일 때 StartGame을 해주면 됩니다.
        if (++_clientLoadCount == 1)
        {
            // Client의 Loading이 모두 완료되었으므로 게임 시작
            Logger.Log("Client Load Completed");
            InGameManager.Instance.StartGame();
        }
        // TODO 
        // 둘 중 한 플레이어의 Load가 TimeOut되면 연결을 초기화 한다.
    }
    
    /// <summary>
    /// 생성된 모든 NetworkObject를 Destroy하는 메소드
    /// </summary>
    private void DestroyAllNetworkObject()
    {
        if (_instantiatedNetworkObjects.Count == 0)
        {
            return;
        }
        
        foreach (NetworkObject networkObject in _instantiatedNetworkObjects)
        {
            networkObject.Despawn();
            // Destroy(networkObject);
        }
        _instantiatedNetworkObjects.Clear();
    }

    /// <summary>
    /// 생성된 모든 LocalObject를 Destory하는 메소드
    /// </summary>
    [ClientRpc]
    private void DestoryAllLocalObjectClientRpc()
    {
        if (_instantiatedLocalObjects.Count != 0)
        {
            foreach (GameObject localObject in _instantiatedLocalObjects)
            {
                Destroy(localObject);
            }
            _instantiatedLocalObjects.Clear();
        }
    }


}
 