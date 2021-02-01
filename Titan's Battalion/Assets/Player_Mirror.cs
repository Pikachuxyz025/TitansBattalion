using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class Player_Mirror : NetworkBehaviour
{
    public static Player_Mirror localplayer;

    [SyncVar]
    public string MatchID;
    [SyncVar]
    public int playerIndex;

    [SyncVar] public Matche currentMatch;
    GameObject playerLobbyUI;
    NetworkMatchChecker networkMatchChecker;
    // Start is called before the first frame update
    void Start()=> networkMatchChecker = GetComponent<NetworkMatchChecker>();


    public override void OnStartClient()
    {
        if (isLocalPlayer)
            localplayer = this;
        else
            playerLobbyUI = UILobby.instance.SpawnPlayerPrefab(this);
    }
    public override void OnStopClient() => ClientDisconnect();

    public override void OnStopServer() => ServerDisconnect();



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

    public void StartGame()
    {
        TargetBeginGame();
    }

    [TargetRpc]
    void TargetBeginGame()
    {
        Debug.Log($"MatchID: {MatchID} | Beginning");
        //Additively Load game scene

        SceneManager.LoadScene(2, LoadSceneMode.Additive);
    }
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
        networkMatchChecker.matchId = string.Empty.ToGuid();
        RpcDisconnectGame();
    }

    [ClientRpc]
    public void RpcDisconnectGame()
    {
        ClientDisconnect();
    }

    void ClientDisconnect()
    {
        if (playerLobbyUI != null)
            Destroy(playerLobbyUI);
    }
    #endregion
}
