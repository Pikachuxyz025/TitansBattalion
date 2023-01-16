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


        NetworkObject.DestroyWithScene = true;
        currentPlayerName = Authentication.PlayerName;
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
                CreateSendDataServerRpc(data.MainBoard);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                CanvasUtilities.Instance.ShowError("Failed creating lobby");
            }
        }
    }

    #endregion

    #region Room

    private readonly Dictionary<ulong, bool> _playersInLobby = new();
    private readonly Dictionary<ulong,string> _playerLobbyNames = new();
    public static event Action<Dictionary<ulong, bool>,Dictionary<ulong,string>> LobbyPlayersUpdated;
    private float _nextLobbyUpdate;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            _playersInLobby.Add(NetworkManager.Singleton.LocalClientId, false);
            _playerLobbyNames.Add(NetworkManager.Singleton.LocalClientId, Authentication.PlayerName);
            UpdateInterface();
        }
        currentPlayerId = NetworkManager.Singleton.LocalClientId;
        AddServerRpc(currentPlayerId,currentPlayerName);
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
    {
        Debug.Log("Current client " + player + " " + name);
        if (!_playerLobbyNames.ContainsKey(player)) _playerLobbyNames.Add(player, name);
    }

    private void PropagateToClients()
    {
        foreach (var player in _playersInLobby) UpdatePlayerClientRpc(player.Key, player.Value, _playerLobbyNames[player.Key]);
    }

    [ClientRpc]
    private void UpdatePlayerClientRpc(ulong clientId, bool isReady,string playerName)
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
    void CreateSendDataServerRpc(int mainData)
    {
        var createdData = Instantiate(dataSend);
        DataSend.mainBoardId = mainData;
        createdData.GetComponent<NetworkObject>().Spawn(false);
    }

    private void UpdateInterface()
    {
        LobbyPlayersUpdated?.Invoke(_playersInLobby,_playerLobbyNames);
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
        //Scene currentscene = SceneManager.GetActiveScene();
        using (new Load("Starting the game..."))
        {
            await MatchmakingService.LockLobby();
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
            //NetworkManager.Singleton.SceneManager.UnloadScene(currentscene);
        }
    }

    #endregion
}