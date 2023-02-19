using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

#pragma warning disable CS4014

/// <summary>
///     Lobby orchestrator. I put as much UI logic within the three sub screens,
///     but the transport and RPC logic remains here. It's possible we could pull
/// </summary>
public class LobbyOrchestrator : NetworkBehaviour
{
    [SerializeField] private MainLobbyScreen _mainLobbyScreen;
    [SerializeField] private CreateLobbyScreen _createScreen;
    [SerializeField] private RoomScreen _roomScreen;
    [SerializeField] private DataSend dataSend;

    [SerializeField] ulong currentPlayerId;
    [SerializeField] private string currentPlayerName;
    private void Start()
    {
        _mainLobbyScreen.gameObject.SetActive(true);
        _createScreen.gameObject.SetActive(false);
        _roomScreen.gameObject.SetActive(false);

        CreateLobbyScreen.LobbyCreated += CreateLobby;
        LobbyRoomPanel.LobbySelected += OnLobbySelected;
        RoomScreen.LobbyLeft += OnLobbyLeft;
        RoomScreen.StartPressed += OnGameStart;
        RoomScreen.ReadySet += CreateSendDataServerRpc;


        NetworkObject.DestroyWithScene = true;
        currentPlayerName = Authentication.PlayerName;

        if (NetworkManager.Singleton.IsListening)
            ReturnToLobby();
        //Debug.Log(currentPlayerName);
    }



    #region Main Lobby

    // Join Lobby Here
    private async void OnLobbySelected(Lobby lobby)
    {
        using (new Load("Joining Lobby..."))
        {
            try
            {
                await MatchmakingService.JoinLobbyWithAllocation(lobby.Id);

                _mainLobbyScreen.gameObject.SetActive(false);
                _roomScreen.gameObject.SetActive(true);

                NetworkManager.Singleton.StartClient();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                CanvasUtilities.Instance.ShowError("Failed joining lobby");
            }
        }
    }



    #endregion


    #region Create

    private async void CreateLobby(LobbyData data)
    {
        using (new Load("Creating Lobby..."))
        {
            try
            {
                await MatchmakingService.CreateLobbyWithAllocation(data);

                _createScreen.gameObject.SetActive(false);
                _roomScreen.gameObject.SetActive(true);



                // Starting the host immediately will keep the relay server alive
                NetworkManager.Singleton.StartHost();
                //CreateSendDataServerRpc(data.GameMode);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                CanvasUtilities.Instance.ShowError("Failed creating lobby");
            }
        }
    }
    private async void ReturnToLobby()
    {
        using (new Load("Returning To Lobby..."))
        {
            try
            {
                _mainLobbyScreen.gameObject.SetActive(false);
                _roomScreen.gameObject.SetActive(true);

                if (NetworkManager.Singleton.IsHost)
                    await MatchmakingService.UnlockLobby();
                LobbySetServerRpc();

                //PropagateToClients();

                //UpdateInterface();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                CanvasUtilities.Instance.ShowError("Failed returning to lobby");
            }
        }
    }
    [ServerRpc(RequireOwnership =false)]
    public void LobbySetServerRpc()
    {
        Debug.Log("shower");
        PropagateToClients();

        UpdateInterface();
    }

    #endregion

    #region Room

    private readonly Dictionary<ulong, bool> _playersInLobby = new();
    private readonly Dictionary<ulong, string> _playerLobbyNames = new();
    public static event Action<Dictionary<ulong, bool>, Dictionary<ulong, string>> LobbyPlayersUpdated;
    private float _nextLobbyUpdate;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("Show me network");
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            _playersInLobby.Add(NetworkManager.Singleton.LocalClientId, false);
            _playerLobbyNames.Add(NetworkManager.Singleton.LocalClientId, Authentication.PlayerName);
           // NetworkManager.Singleton.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
            UpdateInterface();
        }
        currentPlayerId = NetworkManager.Singleton.LocalClientId;
        if(currentPlayerName==String.Empty)
            currentPlayerName = Authentication.PlayerName;
        AddServerRpc(currentPlayerId, currentPlayerName);
        // Client uses this in case host destroys the lobby
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
    }



    private void OnClientConnectedCallback(ulong playerId)
    {
        if (!IsServer) return;
        Debug.Log(playerId);
        // Add locally


        if (!_playersInLobby.ContainsKey(playerId)) _playersInLobby.Add(playerId, false);

        PropagateToClients();

        UpdateInterface();
    }

    [ServerRpc(RequireOwnership = false)]
    void AddServerRpc(ulong player, string name)
    {if (name == String.Empty)
            Debug.Log("Current client " + player + " no name");
        else
            Debug.Log("Current client " + player + " " + name);
        if (!_playerLobbyNames.ContainsKey(player)) _playerLobbyNames.Add(player, name);
        if (!_playersInLobby.ContainsKey(player)) _playersInLobby.Add(player, false);
    }

    private void PropagateToClients()
    {
        Debug.Log("Number of players in lobby: "+_playersInLobby.Count);
        foreach (var player in _playersInLobby) UpdatePlayerClientRpc(player.Key, player.Value, _playerLobbyNames[player.Key]);
    }

    [ClientRpc]
    private void UpdatePlayerClientRpc(ulong clientId, bool isReady, string playerName)
    {
        if (IsServer) return;

        if (!_playersInLobby.ContainsKey(clientId)) _playersInLobby.Add(clientId, isReady);
        else _playersInLobby[clientId] = isReady;

        //Debug.Log(currentPlayerName);
        if (!_playerLobbyNames.ContainsKey(clientId)) _playerLobbyNames.Add(clientId, playerName);
        else _playerLobbyNames[clientId] = playerName;

        UpdateInterface();
    }

    private void OnClientDisconnectCallback(ulong playerId)
    {
        if (IsServer)
        {
            // Handle locally
            if (_playersInLobby.ContainsKey(playerId)) _playersInLobby.Remove(playerId);
            if (_playerLobbyNames.ContainsKey(playerId)) _playerLobbyNames.Remove(playerId);
            // Propagate all clients
            RemovePlayerClientRpc(playerId);

            UpdateInterface();
        }
        else
        {
            // This happens when the host disconnects the lobby
            _roomScreen.gameObject.SetActive(false);
            _mainLobbyScreen.gameObject.SetActive(true);
            OnLobbyLeft();
        }
    }

    [ClientRpc]
    private void RemovePlayerClientRpc(ulong clientId)
    {
        if (IsServer) return;

        if (_playersInLobby.ContainsKey(clientId)) _playersInLobby.Remove(clientId);
        if (_playerLobbyNames.ContainsKey(clientId)) _playerLobbyNames.Remove(clientId);
        UpdateInterface();
    }

    public void OnReadyClicked()
    {
        SetReadyServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetReadyServerRpc(ulong playerId)
    {
        _playersInLobby[playerId] = true;
        PropagateToClients();
        UpdateInterface();
    }

    [ServerRpc]
    void CreateSendDataServerRpc(GameMode mode)
    {
        var createdData = Instantiate(dataSend);
        DataSend.boardData = mode;
        createdData.GetComponent<NetworkObject>().Spawn(false);
    }

    private void UpdateInterface()
    {
        LobbyPlayersUpdated?.Invoke(_playersInLobby, _playerLobbyNames);
    }

    private async void OnLobbyLeft()
    {
        using (new Load("Leaving Lobby..."))
        {
            _playersInLobby.Clear();
            _playerLobbyNames.Clear();
            NetworkManager.Singleton.Shutdown();
            await MatchmakingService.LeaveLobby();
        }
    }

    public override void OnDestroy()
    {

        base.OnDestroy();
        CreateLobbyScreen.LobbyCreated -= CreateLobby;
        LobbyRoomPanel.LobbySelected -= OnLobbySelected;
        RoomScreen.LobbyLeft -= OnLobbyLeft;
        RoomScreen.StartPressed -= OnGameStart;

        // We only care about this during lobby
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }

    }

    public void AddPlayerGameData(GameData _game)
    {

    }

    private async void OnGameStart()
    {
        Scene currentscene = SceneManager.GetActiveScene();
        using (new Load("Starting the game..."))
        {
            await MatchmakingService.LockLobby();
NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);            
        }
    }

    private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
    {
        // Both client and server receive these notifications
        switch (sceneEvent.SceneEventType)
        {
            // Handle server to client Load Notifications
            case SceneEventType.Load:
                {
                    // This event provides you with the associated AsyncOperation
                    // AsyncOperation.progress can be used to determine scene loading progression
                    var asyncOperation = sceneEvent.AsyncOperation;
                    // Since the server "initiates" the event we can simply just check if we are the server here
                    if (IsServer)
                    {
                        // Handle server side load event related tasks here
                    }
                    else
                    {
                        // Handle client side load event related tasks here
                    }
                    break;
                }
            // Handle server to client unload notifications
            case SceneEventType.Unload:
                {
                    // You can use the same pattern above under SceneEventType.Load here
                    break;
                }
            // Handle client to server LoadComplete notifications
            case SceneEventType.LoadComplete:
                {
                    // This will let you know when a load is completed
                    // Server Side: receives this notification for both itself and all clients
                    if (IsServer)
                    {
                        if (sceneEvent.ClientId == NetworkManager.LocalClientId)
                        {
                            // Handle server side LoadComplete related tasks here
                        }
                        else
                        {
                            // Handle client LoadComplete **server-side** notifications here
                        }
                    }
                    else // Clients generate this notification locally
                    {
                        // Handle client side LoadComplete related tasks here
                    }

                    // So you can use sceneEvent.ClientId to also track when clients are finished loading a scene
                    break;
                }
            // Handle Client to Server Unload Complete Notification(s)
            case SceneEventType.UnloadComplete:
                {
                    // This will let you know when an unload is completed
                    // You can follow the same pattern above as SceneEventType.LoadComplete here

                    // Server Side: receives this notification for both itself and all clients
                    // Client Side: receives this notification for itself

                    // So you can use sceneEvent.ClientId to also track when clients are finished unloading a scene
                    break;
                }
            // Handle Server to Client Load Complete (all clients finished loading notification)
            case SceneEventType.LoadEventCompleted:
                {
                    // This will let you know when all clients have finished loading a scene
                    // Received on both server and clients
                    SceneManager.SetActiveScene(SceneManager.GetSceneByName("Game"));
                    foreach (var clientId in sceneEvent.ClientsThatCompleted)
                    {
                        // Example of parsing through the clients that completed list
                        if (IsServer)
                        {
                            // Handle any server-side tasks here
                        }
                        else
                        {
                            // Handle any client-side tasks here
                        }
                    }
                    break;
                }
            // Handle Server to Client unload Complete (all clients finished unloading notification)
            case SceneEventType.UnloadEventCompleted:
                {
                    // This will let you know when all clients have finished unloading a scene
                    // Received on both server and clients
                    foreach (var clientId in sceneEvent.ClientsThatCompleted)
                    {
                        // Example of parsing through the clients that completed list
                        if (IsServer)
                        {
                            // Handle any server-side tasks here
                        }
                        else
                        {
                            // Handle any client-side tasks here
                        }
                    }
                    break;
                }
        }
    }

    #endregion
}