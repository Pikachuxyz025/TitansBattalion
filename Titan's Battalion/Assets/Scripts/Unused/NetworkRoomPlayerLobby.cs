using Mirror;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NetworkRoomPlayerLobby : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[4];
    [SerializeField] private TMP_Text[] playerReadyTexts = new TMP_Text[4];
    [SerializeField] private Button startGameButton = null;
    [SerializeField] private GameObject boardPanel = null;


    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Loading...";
    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;
    [SyncVar]
    public int ArmyId = 0;
    [SyncVar]
    public int BoardId = 0;

   private bool isLeader;
    public bool IsLeader
    {
        set
        {
            isLeader = value;
            startGameButton.gameObject.SetActive(value);
            boardPanel.SetActive(value);
        }
    }

    private NetworkManagerLobby room;
    
    private NetworkManagerLobby Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerLobby;
        }
    }

    public override void OnStartAuthority()
    {
        CmdSetDisplayName(PlayerNameInput_Mirror.DisplayName);
        CmdSetArmyId(PlayerNameInput_Mirror.ArmyID);

        lobbyUI.SetActive(true);
    }

    public override void OnStartClient()
    {
        Room.RoomPlayers.Add(this);

        UpdateDisplay();
    }
    public override void OnStopServer()
    {
        Room.RoomPlayers.Remove(this);

        UpdateDisplay();
    }

    public void HandleReadyStatusChanged(bool oldValue, bool newValue) => UpdateDisplay();
    public void HandleDisplayNameChanged(string oldValue, string newValue) => UpdateDisplay();

    private void UpdateDisplay()
    {
        if (!hasAuthority)
        {
            foreach (var player in Room.RoomPlayers)
            {
                if (player.hasAuthority)
                {
                    player.UpdateDisplay();
                    break;
                }
            }
            return;
        }

        for (int i = 0; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting For Player...";
            playerReadyTexts[i].text = string.Empty;
        }
        for (int i = 0; i < Room.RoomPlayers.Count; i++)
        {
            playerNameTexts[i].text = Room.RoomPlayers[i].DisplayName;
            playerReadyTexts[i].text = Room.RoomPlayers[i].IsReady ?
                "<color=green>Ready</color>" :
                "<color=red>Not Ready</color>";
        }
    }

    public void HandleReadyToStart(bool readyToStart)
    {
        if (!isLeader) { return; }

        if (BoardId == 0)
            startGameButton.interactable = false;
        else
            startGameButton.interactable = readyToStart;
    }

    [Command]
    private void CmdSetDisplayName(string displayName)
    {
        DisplayName = displayName;
    }

    [Command]
    private void CmdSetArmyId(int armyID)
    {
        ArmyId = armyID;
    }

    [Command]
    private void CmdSetBoardId(int boardID)
    {
        BoardId = boardID;
    }

    [Command]
    public void CmdReadyUp()
    {
        IsReady = !IsReady;
        NetworkManagerLobby.PlayerName[Room.RoomPlayers.IndexOf(this)] = DisplayName;
        NetworkManagerLobby.PlayerArmy[Room.RoomPlayers.IndexOf(this)] = ArmyId;
        Debug.Log(NetworkManagerLobby.PlayerName[Room.RoomPlayers.IndexOf(this)] + " added. Total count: " + NetworkManagerLobby.PlayerName.IndexOf(DisplayName));
        Room.NotifyPlayersOfReadyState();
    }

    [Command]
    public void CmdStartGame()
    {
        if (Room.RoomPlayers[0].connectionToClient != connectionToClient) { return; }
        NetworkManagerLobby.BoardId = BoardId;
        Room.StartGame();
    }
}
