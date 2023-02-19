using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameMode
{None=0,
    Chess=1,
    BasicBoard=2,
    T2=3,
}

/// <summary>
///     NetworkBehaviours cannot easily be parented, so the network logic will take place
///     on the network scene object "NetworkLobby"
/// </summary>
public class RoomScreen : MonoBehaviour
{
    [SerializeField] private LobbyPlayerPanel _playerPanelPrefab;
    [SerializeField] private Transform _playerPanelParent;
    [SerializeField] private TMP_Text _waitingText;
    [SerializeField] private GameObject _startButton, _readyButton;
    [SerializeField] private TMP_Dropdown gameModeDropdown;
    [SerializeField] private GameObject GameModeDropDown;
    [SerializeField] private GameObject BoardSelectDropdown;
    [SerializeField] private TMP_Dropdown boardDropdown;
    [SerializeField] private TMP_Text boardType;

    private readonly List<LobbyPlayerPanel> _playerPanels = new();
    private bool _allReady;
    private bool _ready;
    private GameMode selectedGameMode = GameMode.Chess;

    public static event Action StartPressed;
    public static event Action<GameMode> ReadySet;
    public static event Action<GameData> GameStarted;

    [SerializeField] private GameData _gameData;

    private void OnEnable()
    {
        foreach (Transform child in _playerPanelParent) Destroy(child.gameObject);
        _playerPanels.Clear();

        LobbyOrchestrator.LobbyPlayersUpdated += NetworkLobbyPlayersUpdated;
        MatchmakingService.CurrentLobbyRefreshed += OnCurrentLobbyRefreshed;
        SetOptions(gameModeDropdown, Contants.GameModes);


        GameModeDropDown.SetActive(false);
        BoardSelectDropdown.SetActive(false);
        _startButton.SetActive(false);
        _readyButton.SetActive(false);
        _ready = false;
    }

    private void OnDisable()
    {
        LobbyOrchestrator.LobbyPlayersUpdated -= NetworkLobbyPlayersUpdated;
        MatchmakingService.CurrentLobbyRefreshed -= OnCurrentLobbyRefreshed;
    }

    public static event Action LobbyLeft;

    public void AddMainBoardData(int value)
    {
        _gameData.MainBoard = value;
    }

    public void AddArmyBoardData(int value)
    {
        _gameData.ArmyBoard = value;
    }

    public void OnLeaveLobby()
    {
        LobbyLeft?.Invoke();
    }

    private void NetworkLobbyPlayersUpdated(Dictionary<ulong, bool> players, Dictionary<ulong, string> playerNames)
    {
        var allActivePlayerIds = players.Keys;

        // Remove all inactive panels
        List<LobbyPlayerPanel> toDestroy = _playerPanels.Where(p => !allActivePlayerIds.Contains(p.PlayerId)).ToList();

        foreach (LobbyPlayerPanel panel in toDestroy)
        {
            _playerPanels.Remove(panel);
            Destroy(panel.gameObject);
        }

        foreach (var player in players)
        {
            LobbyPlayerPanel currentPanel = _playerPanels.FirstOrDefault(p => p.PlayerId == player.Key);
            if (currentPanel != null)
            {
                if (player.Value) currentPanel.SetReady();
            }
            else
            {
                LobbyPlayerPanel panel = Instantiate(_playerPanelPrefab, _playerPanelParent);
                if (playerNames.ContainsKey(player.Key))
                    panel.InitializePlayerPanel(player.Key, playerNames[player.Key]);
                _playerPanels.Add(panel);
            }
        }
        GameModeDropDown.SetActive(NetworkManager.Singleton.IsHost);
        _startButton.SetActive(NetworkManager.Singleton.IsHost && players.All(p => p.Value));
        _readyButton.SetActive(!_ready);
    }

    public void SetActiveBoardBool(int value)
    {
        if (value != 0)
        {
            BoardSelectDropdown.SetActive(true);
            switch (value)
            {
                case 1:
                    boardType.text = "Main Board";
                    SetOptions(boardDropdown, Contants.MainBoards);
                    break;
            }
        }
        else
        {
            BoardSelectDropdown.SetActive(false);
            selectedGameMode = GameMode.Chess;
        }
    }

    public void SelectBattalionGameMode(int value)
    {
        switch (value)
        {
            case 0:
                selectedGameMode = GameMode.BasicBoard;
                break;
            case 1:
                selectedGameMode = GameMode.T2;
                break;
        }
    }

    public void SetOptions(TMP_Dropdown dropdown, IEnumerable<string> values)
    {
        dropdown.options = values.Select(type => new TMP_Dropdown.OptionData { text = type }).ToList();
    }

    private void OnCurrentLobbyRefreshed(Lobby lobby)
    {
        _waitingText.text = $"Waiting on players... {lobby.Players.Count}/{lobby.MaxPlayers}";
    }

    public void OnReadyClicked()
    {
        if (NetworkManager.Singleton.IsHost)      
            ReadySet?.Invoke(selectedGameMode);
        

        _readyButton.SetActive(false);
        GameModeDropDown.SetActive(false);
        BoardSelectDropdown.SetActive(false);
        _ready = true;
    }


    public void OnStartClicked()
    {
        StartPressed?.Invoke();
        GameStarted?.Invoke(_gameData);
    }
}
