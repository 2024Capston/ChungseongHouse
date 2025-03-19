using System.Linq;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace RaB.Connection
{
    internal class OfflineState : ConnectionState
    {
        public override void Enter()
        {
            ConnectionManager.Instance.CurrentLobby?.Leave();
            ConnectionManager.Instance.CurrentLobby = null;
            NetworkManager.Singleton.Shutdown();
            
            // Offline 상태에서는 Home Scene에 있어야 합니다.
            SceneLoadManager.Instance.LoadScene(SceneType.Home.ToString(), false);
        }

        public override void Exit() { }

        public override void StartServer()
        {

            BaseUIData baseUIData = new BaseUIData();
            UIManager.Instance.OpenUI<LoadingUI>(baseUIData);
            
            // Server를 시작하면 StartingHost State로 전환
            ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.StartingHost);
        }

        public override async Task<Result> StartClient(string lobbyId)
        {
            Lobby? lobby;
            Result result;
            (lobby, result) =  await FindLobby(lobbyId);
            
            if (result == Result.OK)
            {
                BaseUIData baseUIData = new BaseUIData();
                UIManager.Instance.OpenUI<LoadingUI>(baseUIData);
                
                ConnectionManager.Instance.CurrentLobby = lobby;
                ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.ClientConnecting);
            }

            return result;
        }
        
        private async Task<(Lobby? lobby, Result result)> FindLobby(string lobbyId)
        {
            if (!ulong.TryParse(lobbyId, out ulong id))
            {
                return (null, Result.InvalidParam);
            }

            // RaB에서 생성된 모든 Lobby를 불러온 후 주어진 id와 일치하는 Lobby가 있는지 확인한다.
            Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithKeyValue("game_id", "RaB").RequestAsync();
            foreach (Lobby lobby in lobbies ?? Enumerable.Empty<Lobby>())
            {
                if (lobby.Id == id)
                {
                    if (lobby.MemberCount == 2)
                    {
                        return (null, Result.Busy);
                    }

                    return (lobby, Result.OK);
                }
            }

            return (null, Result.InvalidName);
        }
        
        public override void GameLobbyJoinRequested(Lobby lobby, SteamId _)
        {
            ConnectionManager.Instance.CurrentLobby = lobby;
            ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.ClientConnecting);
        }
    }
}