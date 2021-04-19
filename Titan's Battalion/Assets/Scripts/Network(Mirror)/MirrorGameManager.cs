using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState
{
    None,
    Setup,
    Stalemate,
    Check,
    GracePeriod,
    Ongoing
}
public enum RematchState
{
    None,
    Rematch,
    NoRematch
}
public class MirrorGameManager : NetworkBehaviour
{
    public SID_BoardManager_Mirror SID_BM;
    public Player_Mirror[] playerHosts = new Player_Mirror[2];
    public PlayerInfo[] playerInfos = new PlayerInfo[2];
    public ArmyManager army;
    [HideInInspector] public static Dictionary<int, Field> curBoard;
    [HideInInspector] public static Dictionary<int, Army> curArmy;
    [HideInInspector] public Quaternion neutralCoordination = Quaternion.Euler(0, 0, 0), coordination = Quaternion.Euler(0, 180, 0);
    public Dictionary<PlayerInfo, RematchState> continueorno = new Dictionary<PlayerInfo, RematchState>();
    [SyncVar]
    public bool armyIsSet;
    [SyncVar]
    public GameObject varin;
    [SyncVar]
    private int startup = 0;
    public GameState currentState;

    [SyncVar]
    public int turnCount = 0;

    [ClientRpc]
    public void RpcSetupPlayerHosts(int play, Player_Mirror sum)
    {
        playerHosts[play - 1] = sum;
    }

    [ClientRpc]
    public void RpcSetupPlayerHost(int play, PlayerInfo sum)
    {
        playerInfos[play - 1] = sum;
    }
    private void Update()
    {
        if (isServer)
        {
            RpcSetupCurInfo();

            ReadyToGo();
            if (armyIsSet == true)
            {
                if (startup < 1)
                {
                    SID_BM.buildModeOn = false;
                    SID_BM.setActive = true;
                    turnCount = 1;
                    SID_BoardManager_Mirror.M_eventmoment.Invoke();
                    currentState = GameState.GracePeriod;
                    RpcShould();
                    startup++;
                }
            }
            //Checking();
        }

    }

    public void Checking()
    {
        if (currentState == GameState.Check || currentState == GameState.Stalemate)
        {
            SID_BM.setActive = false;
            RpcDebbugin();
            if (!continueorno.ContainsKey(playerInfos[0]) || !continueorno.ContainsKey(playerInfos[1]))
                return;
            if (continueorno[playerInfos[0]] == RematchState.NoRematch || continueorno[playerInfos[1]] == RematchState.NoRematch)
            {
                Debug.Log("let's return");
                RpcReturnScene();
            }
            else if (continueorno[playerInfos[0]] == RematchState.Rematch && continueorno[playerInfos[1]] == RematchState.Rematch)
            {
                Debug.Log("let's play again");
                RpcRestartScene();
            }
        }
    }

    [ClientRpc]
    void RpcDebbugin()
    {
        Debug.Log("self ending");
    }
    #region Setup
    [ClientRpc]
    public void RpcShould()
    {
        SID_BoardManager_Mirror.M_eventmoment.Invoke();
        currentState = GameState.GracePeriod;
    }

    [ClientRpc]
    public void RpcRestartScene()
    {
        foreach (Player_Mirror item in playerHosts)
        {
            item.Restarted();
        }
    }

    [ClientRpc]
    public void RpcReturnScene()
    {
        foreach (Player_Mirror item in playerHosts)
        {
            item.BackToMenu();
        }
    }

    public void ReadyToGo() => armyIsSet = AllBoardsAreSet();

    private bool AllBoardsAreSet()
    {
        if (playerInfos[1] != null && playerInfos[0] != null)
        {
            for (int i = 0; i < playerInfos.Length; i++)
            {
                if (playerInfos[i].setToMatch == false)
                    return false;
            }
        }
        else
            return false;
        return true;
    }

    public void SelectBoard(int boardId)
    {
        GameObject yo = Instantiate(curBoard[boardId - 1].fieldgrid, new Vector3(0, 0, 0), neutralCoordination);
        yo.GetComponent<NetworkMatchChecker>().matchId = GetComponent<NetworkMatchChecker>().matchId;
        NetworkServer.Spawn(yo, connectionToClient);

        varin = yo;

        SID_BM.buildModeOn = true;
    }



    private void OnEnable()
    {
        curBoard = army.allBoards;
        curArmy = army.allArmies;
        currentState = GameState.Setup;
    }

    [ClientRpc]
    public void RpcSetupCurInfo()
    {
        curBoard = army.allBoards;
        curArmy = army.allArmies;
    }

    #endregion
}
