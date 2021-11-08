using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UILobby : MonoBehaviour
{
    public static UILobby instance;
    public string currentPlayerUsername;

    [Header("Host Join")]
    [SerializeField] InputField joinMatchInput;
    [SerializeField] InputField username;
    [SerializeField] List<Selectable> lobbySelectables = new List<Selectable>();
    [SerializeField] Canvas lobbyCanvas;
    [SerializeField] Canvas searchCanvas;

    [Header("Lobby")]
    [SerializeField] Transform UIPlayerParent;
    [SerializeField] GameObject UIPlayerPrefab;
    [SerializeField] Text matchIDText;
    public GameObject beginGameButton, readyGameButton;
    public GameObject selectBoardDropdown;

    [SerializeField] Dropdown boardSelect, armySelect;
    [SerializeField] MatchMaker matcheHome;
    public string playerMatchID;

    GameObject playerLobbyUI;
    bool searching = false, armyActive = false, boardActive = false, onoff = false;
    [SerializeField] int playerNum = 0;
    private void Start()
    {
        instance = this;
    }

    private void Update()
    {
        if (username.text == "" && username.interactable != false)
            lobbySelectables.ForEach(x => x.interactable = false);
        else
            lobbySelectables.ForEach(x => x.interactable = true);
        currentPlayerUsername = username.text;

        if (playerMatchID != string.Empty)
        {
            if (matcheHome.GetReadyMatch(playerMatchID) && matcheHome.GetMatchFull(playerMatchID))
            {
                beginGameButton.GetComponent<Button>().interactable = true;
                Debug.Log("players ready");
            }
            else
                beginGameButton.GetComponent<Button>().interactable = false;

        }
    }

    public void Back() => Player_Mirror.localplayer.ReturnToMainMenu();

    public void HostPublic()
    {
        joinMatchInput.interactable = false;
        username.interactable = false;
        lobbySelectables.ForEach(x => x.interactable = false);

        Player_Mirror.localplayer.HostGame(true);
    }

    public void HostPrivate()
    {
        joinMatchInput.interactable = false;
        username.interactable = false;
        lobbySelectables.ForEach(x => x.interactable = false);

        Player_Mirror.localplayer.HostGame(false);
    }

    public void HostSuccess(bool success, string matchID)
    {
        if (success)
        {
            lobbyCanvas.enabled = true;
            if (playerLobbyUI != null)
                Destroy(playerLobbyUI);
            playerLobbyUI = SpawnPlayerPrefab(Player_Mirror.localplayer);
            matchIDText.text = matchID;
            beginGameButton.SetActive(true);
            playerMatchID = matchID;

            selectBoardDropdown.SetActive(true);
            readyGameButton.GetComponent<Button>().interactable = false;
            Player_Mirror.localplayer.CmdBecomeHost(true);
        }
        else
        {
            username.interactable = true;
            lobbySelectables.ForEach(x => x.interactable = true);
        }
    }

    public void Join()
    {
        username.interactable = false;
        lobbySelectables.ForEach(x => x.interactable = false);

        Player_Mirror.localplayer.JoinGame(joinMatchInput.text.ToUpper());
    }

    public void JoinSuccess(bool success, string matchID)
    {
        if (success)
        {
            lobbyCanvas.enabled = true;
            ResetSelection();
            if (playerLobbyUI != null)
                Destroy(playerLobbyUI);

            playerLobbyUI = SpawnPlayerPrefab(Player_Mirror.localplayer);
            readyGameButton.GetComponent<Button>().interactable = false;
            matchIDText.text = matchID;
            playerMatchID = matchID;
        }
        else
        {
            username.interactable = true;
            lobbySelectables.ForEach(x => x.interactable = true);
        }
    }

    public GameObject SpawnPlayerPrefab(Player_Mirror player)
    {
        GameObject newUIPlayer = Instantiate(UIPlayerPrefab, UIPlayerParent);
        newUIPlayer.GetComponent<UIPlayer>().SetPlayer(player);
        newUIPlayer.transform.SetSiblingIndex(player.playerIndex - 1);
        return newUIPlayer;
    }

    public void Interaction(bool value) => beginGameButton.GetComponent<Button>().interactable = value;


    public void BeginGame()
    {
        matcheHome.SetMatchInProgress(playerMatchID, true);
        Player_Mirror.localplayer.BeginGame();
    }

    public void ArmySelect(int value)
    {
        armyActive = value == 0 ? false : true;
        Player_Mirror.localplayer.CmdArmy(value);
        readyGameButton.GetComponent<Button>().interactable = ReadyInteractable();
    }

    public void BoardSelect(int value)
    {
        boardActive = value == 0 ? false : true;

        Player_Mirror.localplayer.CmdBoard(value);
        readyGameButton.GetComponent<Button>().interactable = ReadyInteractable();
    }

    public bool ReadyInteractable()
    {
        if (selectBoardDropdown.activeInHierarchy)
        {
            if (armyActive && boardActive)
                return true;
        }
        else
        {
            if (armyActive)
                return true;
        }
        return false;
    }

    public void ReadyToGo()
    {
        onoff = !onoff;
        if (onoff)
            playerLobbyUI.GetComponent<UIPlayer>().SetReady();
        else
            playerLobbyUI.GetComponent<UIPlayer>().SetNotReady();
        Player_Mirror.localplayer.Ready();
       
    }
    public bool NotifyPlayersOfReadyState(SyncListGameObject _players, bool setnStone)
    {
        if (setnStone)
        {
            if (IsReadyToStart(_players))
                return true;
            else
                return false;
        }
        else
            return false;
    }

    private bool IsReadyToStart(SyncListGameObject _players)
    {
        foreach (var player in _players)
        {
            if (!player.GetComponent<Player_Mirror>().isReady)
                return false;
        }
        return true;
    }

    void ResetSelection()
    {
        boardSelect.value = 0;
        armySelect.value = 0;
        boardSelect.interactable = true;
        armySelect.interactable = true;
        beginGameButton.SetActive(false);
        readyGameButton.GetComponent<Button>().interactable = false;
        selectBoardDropdown.SetActive(false);
        Player_Mirror.localplayer.CmdBecomeHost(false);
    }

    public void SearchGame()
    {
        Debug.Log("Searching for Game");
        searchCanvas.enabled = true;
        StartCoroutine(SearchingForGame());
    }

    IEnumerator SearchingForGame()
    {
        searching = true;
        float currentTime = 1;

        while (searching)
        {
            if (currentTime > 0)
                currentTime -= Time.deltaTime;
            else
            {
                Player_Mirror.localplayer.SearchGame();
                currentTime = 1;
            }

            yield return null;
        }
    }

    public void SearchSuccess(bool success, string matchID)
    {
        if (success)
        {
            searchCanvas.enabled = false;
            JoinSuccess(success, matchID);
            searching = false;
        }
    }

    public void SearchCancel()
    {
        searchCanvas.enabled = false;
        username.interactable = true;
        lobbySelectables.ForEach(x => x.interactable = true);
        searching = false;
    }


    public void DisconnectLobby()
    {
        if (playerLobbyUI != null)
            Destroy(playerLobbyUI);
        Player_Mirror.localplayer.DisconnectGame();

        lobbyCanvas.enabled = false;
        username.interactable = true;
        lobbySelectables.ForEach(x => x.interactable = true);
        ResetSelection();
        playerMatchID = string.Empty;
    }
}
