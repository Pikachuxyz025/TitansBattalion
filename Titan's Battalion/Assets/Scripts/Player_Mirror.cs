using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine.UI;
using UnityEngine.Events;

public class Player_Mirror : NetworkBehaviour
{
    public static Player_Mirror localplayer;
    [SerializeField] private GameObject playerSpawnSystem = null;

    [SyncVar] public string MatchID;
    [SyncVar] public int playerIndex;
    [SyncVar] public string playerUsername;
    [SyncVar] public int BoardID, ArmyID;
    [SyncVar] public Matche currentMatch;
    [SerializeField] GameObject playerLobbyUI;
    [SerializeField] private GameObject gameManager;
    [SerializeField] private PlayerInfo newPlayerPrefab = null;
    NetworkMatchChecker networkMatchChecker;
    public bool /*armyActive, boardActive,*/ isHost = false;
    [SyncVar] public bool isReady = false;


    public delegate void ConnectionChangeDelegate(NetworkConnection conn, int play, int arm, string use, GameObject sum);
    public event ConnectionChangeDelegate EventConnectionChange;


    public ConnectionChangedEvent connectionChanged;

    void Awake()
    {
        networkMatchChecker = GetComponent<NetworkMatchChecker>();
    }

    [Command]
    public void CmdBecomeHost(bool host)
    {
        isHost = host;
        TargetBecomeHost(host);
    }

    [TargetRpc]
    public void TargetBecomeHost(bool host) => isHost = host;

    public override void OnStartClient()
    {
        if (isLocalPlayer)
            localplayer = this;
        else
            playerLobbyUI = UILobby.instance.SpawnPlayerPrefab(this);
        DontDestroyOnLoad(this.gameObject);
    }

    public void Ready() => CmdReady();

    [Command]
    public void CmdReady() => isReady = !isReady;

    public override void OnStopClient() => ClientDisconnect();

    public override void OnStopServer() => ServerDisconnect();

    [Command]
    public void CmdBoard(int boardID)
    {
        BoardID = boardID;
        TargetBoard(boardID);
    }
    [TargetRpc]
    public void TargetBoard(int boardID) => BoardID = boardID;
    [Command]
    public void CmdArmy(int armyID) => ArmyID = armyID;

    #region Host Match

    public void HostGame(bool publicMatch)
    {
        string matchID = MatchMaker.GetRandomMatchID();
        CmdHostGame(matchID, publicMatch);
    }

    [Command]
    void CmdHostGame(string _matchID, bool publicMatch)
    {
        MatchID = _matchID;
        if (MatchMaker.instance.HostGame(_matchID, publicMatch, gameObject, out playerIndex))
        {
            Debug.Log($"<color=green>Game hosted successfully</color>");
            networkMatchChecker.matchId = _matchID.ToGuid();
            TargetHostGame(true, _matchID, playerIndex);
        }
        else
        {
            Debug.Log($"<color=red>Game hosted failed</color>");
            TargetHostGame(false, _matchID, playerIndex);
        }
    }

    [TargetRpc]
    void TargetHostGame(bool success, string _matchID, int _playerIndex)
    {
        playerIndex = _playerIndex;
        MatchID = _matchID;
        Debug.Log($"MatchID: {MatchID} == {_matchID}");
        UILobby.instance.HostSuccess(success, _matchID);
    }
    #endregion

    #region Join Match

    public void JoinGame(string _inputField)
    {
        CmdJoinGame(_inputField);
    }

    [Command]
    void CmdJoinGame(string _matchID)
    {
        MatchID = _matchID;
        if (MatchMaker.instance.JoinGame(_matchID, gameObject, out playerIndex))
        {
            Debug.Log($"<color=green>Game joined successfully</color>");
            networkMatchChecker.matchId = _matchID.ToGuid();
            TargetJoinGame(true, _matchID, playerIndex);
        }
        else
        {
            Debug.Log($"<color=red>Game joined failed</color>");
            TargetJoinGame(false, _matchID, playerIndex);
        }
    }

    [TargetRpc]
    void TargetJoinGame(bool success, string _matchID, int _playerIndex)
    {
        MatchID = _matchID;
        this.playerIndex = _playerIndex;
        Debug.Log($"MatchID: {MatchID} == {_matchID}");
        UILobby.instance.JoinSuccess(success, _matchID);
    }
    #endregion

    #region Begin Match
    public void BeginGame()
    {
        CmdBeginGame();
    }

    [Command]
    void CmdBeginGame()
    {
        MatchMaker.instance.BeginGame(MatchID);
        Debug.Log($"<color = green>Beginning...</color>");
    }

    public void StartGame() => TargetBeginGame();

    //public void BackToMenu() => TargetReturn();

    //[TargetRpc]
    public void BackToMenu()
    {
        StartCoroutine(Return());
        //UILobby.instance.gameObject.SetActive(true);
    }

    [TargetRpc]
    void TargetBeginGame()
    {
        Debug.Log($"MatchID: {MatchID} | Beginning");
        //Additively Load game scene
        //DontDestroyOnLoad(MatchMaker.instance.gameObject);
        //SceneManager.LoadScene(2, LoadSceneMode.Additive);
        UILobby.instance.gameObject.SetActive(false);
        StartCoroutine(SetupnActive());
    }

    public void Restarted() => StartCoroutine(Reload());

    [TargetRpc]
    void TargetRestartGame()
    {
        StartCoroutine(Reload());
    }

    private IEnumerator Return()
    {
        AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);

        yield return asyncOperation;

            UILobby.instance.gameObject.SetActive(true);
        yield return null;
    }

    private IEnumerator Reload()
    {
        AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        yield return asyncOperation;

        if (isHost)
            BeginGame();
        yield return null;
    }

    IEnumerator SetupnActive()
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);
        yield return asyncOperation;
        Scene scnee = SceneManager.GetSceneByBuildIndex(2);
        SceneManager.SetActiveScene(scnee);
        Debug.Log(SceneManager.GetActiveScene().name);

        SetupLevel();
    }

    #endregion

    #region Set Scene and Game

    public void SetupLevel()
    {
        if (!hasAuthority) { return; }
        if (isHost)
        {
            Debug.Log("Player " + playerIndex + " giving message");
            CmdSceneChange();
        }
        StartCoroutine(Spawn());
    }

    [Command]
    public void CmdSceneChange() => SceneChange(BoardID);

    IEnumerator Spawn()
    {
        yield return new WaitForSeconds(.4f);
        CmdSetConnection();
    }

    public void SceneChange(int BoardID)
    {
        GameObject manager = Instantiate(gameManager);
        manager.GetComponent<NetworkMatchChecker>().matchId = networkMatchChecker.matchId;
        NetworkServer.Spawn(manager);
        manager.GetComponent<MirrorGameManager>().RpcSetupCurInfo();
        manager.GetComponent<MirrorGameManager>().SelectBoard(BoardID);
        manager.GetComponent<PlayerSpawnSystem>().SpawnSystemSetup();

        RpcService("the board ID for this scene is: " + BoardID);
    }

    [ClientRpc]
    public void RpcService(string ladi)
    {
        Debug.Log(ladi);
    }

    [Command]
    public void CmdSetConnection() => PlayerSpawnSystem.spawnSystem.SpawnMultiPlayer(this.connectionToClient, playerIndex, ArmyID, playerUsername, this, networkMatchChecker.matchId);

    #endregion

    #region Search Match
    public void SearchGame()
    {
        CmdSearchGame();
    }

    [TargetRpc]
    public void TargetSearchGame(bool success, string _matchID, int _playerIndex)
    {
        playerIndex = _playerIndex;
        Debug.Log($"MatchID: {MatchID} == {_matchID}");
        UILobby.instance.SearchSuccess(success, _matchID);
    }

    [Command]
    public void CmdSearchGame()
    {
        if (MatchMaker.instance.SearchGame(gameObject, out playerIndex, out MatchID))
        {
            Debug.Log($"<color=green>Game Found</color>");
            networkMatchChecker.matchId = MatchID.ToGuid();
            TargetSearchGame(true, MatchID, playerIndex);
        }
        else
        {
            Debug.Log($"<color=red>Game Not Found</color>");
            TargetSearchGame(false, MatchID, playerIndex);
        }
    }
    #endregion

    #region Disconnect Match
    public void DisconnectGame()
    {
        CmdDisconnectGame();
    }
    [Command]
    public void CmdDisconnectGame()
    {
        ServerDisconnect();
    }
    void ServerDisconnect()
    {
        MatchMaker.instance.PlayerDisconnected(this, MatchID);
        RpcDisconnectGame();
        networkMatchChecker.matchId = string.Empty.ToGuid();
    }

    [ClientRpc]
    public void RpcDisconnectGame()
    {
        ClientDisconnect();
    }

    void ClientDisconnect()
    {
        //Debug.Log("Player " + playerLobbyUI.GetComponent<Player_Mirror>().playerIndex + " has left");
        if (playerLobbyUI != null)
            Destroy(playerLobbyUI);
    }
    #endregion
}
