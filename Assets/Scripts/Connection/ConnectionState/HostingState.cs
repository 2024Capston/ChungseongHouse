using Steamworks;
using Steamworks.Data;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RaB.Connection
{
    internal class HostingState : OnlineState
    {
        public override void Enter()
        {
            SceneLoadManager.Instance.LoadScene(SceneType.Lobby.ToString(), useNetworkSceneManager: true);
        }

        public override void Exit() { } 

        public override void OnClientDisconnect()
        {
            // 현재 Scene이 InGame일 때는 Lobby Scene으로 이동해야 한다.
            if (SceneManager.GetActiveScene().name == SceneType.InGame.ToString())
            {
                SceneLoadManager.Instance.LoadScene(SceneType.Lobby.ToString(), useNetworkSceneManager: true);
            }
        }

        public override void OnUserRequestedShutdown()
        {
            for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsIds.Count; i++)
            {
                ulong id = NetworkManager.Singleton.ConnectedClientsIds[i];
                if (id != NetworkManager.Singleton.LocalClientId)
                {
                    NetworkManager.Singleton.DisconnectClient(id, "HostEndedSession");
                }
            }
            ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.Offline);
        }

        public override void OnServerStopped()
        {
            ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.Offline);
        }
    }
}