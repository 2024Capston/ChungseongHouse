using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;

namespace RaB.Connection
{
    /// <summary>
    /// 네트워크 연결관련 처리를 하는 매니저
    /// 현재 연결 상태와 입장한 Lobby를 관리하고 있습니다.
    /// </summary>
    public class ConnectionManager : SingletonBehavior<ConnectionManager>
    {
        private ConnectionState _currentState;

        internal readonly OfflineState Offline = new OfflineState();
        internal readonly StartingHostState StartingHost = new StartingHostState();
        internal readonly HostingState Hosting = new HostingState();
        internal readonly ClientConnectingState ClientConnecting = new ClientConnectingState();
        internal readonly ClientConnectedState ClientConnected = new ClientConnectedState();
        
        public Lobby? CurrentLobby { get; internal set; }
        
        private void Start()
        {
            CurrentLobby = null;
            _currentState = Offline;
            
            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
            SteamFriends.OnGameLobbyJoinRequested += GameLobbyJoinRequested;
            NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.OnServerStopped += OnServerStopped;
            NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;
        }

        protected override void OnDestroy()
        {
            CurrentLobby = null;
            SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
            SteamFriends.OnGameLobbyJoinRequested -= GameLobbyJoinRequested;
            
            /*
             NetworkManager에 등록된 델리게이트를 제거하는 코드가 있었으나
             NetworkManager가 더 빨리 Destroy 되기 때문에 제거하였습니다.
             */
        }

        public void StartServer()
        {
            _currentState.StartServer();
        }

        public async Task<Result> StartClient(string lobbyId)
        {
            return await _currentState.StartClient(lobbyId);
        }

        public void RequestShutdown()
        {
            _currentState.OnUserRequestedShutdown();
        }

        /// <summary>
        /// 현재 State를 나가고 nextState에 들어간다. 
        /// </summary>
        /// <param name="nextState"></param>
        internal void ChangeState(ConnectionState nextState)
        {
            Debug.Log($"{name}: Changed connection state from {_currentState.GetType().Name} to {nextState.GetType().Name}.");

            if (_currentState != null)
            {
                _currentState.Exit();
            }

            _currentState = nextState;
            _currentState.Enter();
        }
        
        /// <summary>
        /// 연결이벤트가 발생했을 때 이벤트 타입에 따라 Connect, DisConnect를 구분합니다.
        /// </summary>
        private void OnConnectionEvent(NetworkManager networkManager, ConnectionEventData connectionEventData)
        {
            if (connectionEventData.EventType == ConnectionEvent.ClientConnected)
            {
                OnClientConnectedCallback();
            }
            else if (connectionEventData.EventType == ConnectionEvent.ClientDisconnected)
            {
                OnClientDisconnetCallback();
            }
        }

        private void OnClientConnectedCallback()
        {
            _currentState.OnClientConnected();
        }

        private void OnClientDisconnetCallback()
        {
            _currentState.OnClientDisconnect();
        }

        private void OnServerStarted()
        {
            _currentState.OnServerStarted();
        }

        private void OnServerStopped(bool _)
        {
            _currentState.OnServerStopped();
        }

        private void OnTransportFailure()
        {
            _currentState.OnTransportFailure();
        }

        private void OnLobbyCreated(Result result, Lobby lobby)
        {
            _currentState.OnLobbyCreated(result, lobby);
        }

        private void OnLobbyEntered(Lobby lobby)
        {
            _currentState.OnLobbyEntered(lobby);
        }

        private void GameLobbyJoinRequested(Lobby lobby, SteamId steamId)
        {
            _currentState.GameLobbyJoinRequested(lobby, steamId);
        }
    }
}

