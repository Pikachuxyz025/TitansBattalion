using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public enum GameState
{
    Starting,
    Victory,
    Lose,
    MainMenu,

}

public class GameManager : NetworkBehaviour
{
    public static event Action<GameState> OnBeforeStateChanged;
    public static event Action<GameState> OnAfterStateChanged;
    [SerializeField] private ChessPieceManager chessPieceManager;
    [SerializeField] private ChessboardGenerator chesGen;
    [SerializeField] private ChessboardManager chessboardManager;
    [SerializeField] public GameUIManager uiManager;

    public List<ChessGen_Test> playerList = new List<ChessGen_Test>();
    private List<ChessGen_Test> playerActiveList = new List<ChessGen_Test>();
    public int mainBoardId;

    [SerializeField] private ChessGen_Test playerPrefab;
    [SerializeField] private DataSend dataSend;
    public GameState state;
    private bool isRestarting=false;
    public static GameManager instance;
    private int playerCount = 0;
    private int turnNumber;
    private void Awake()
    {
        instance = this;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (!isRestarting)
        {
            MatchmakingService.LeaveLobby();
            if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
        }
        //else
        //{
        //    mainBoardId = DataSend.mainBoardId;
        //    Destroy(DataSend.instance.gameObject);
        //    CreateSendDataServerRpc();
        //}
    }
    [ServerRpc]
    void CreateSendDataServerRpc()
    {
        var createdData = Instantiate(dataSend);
        DataSend.mainBoardId = mainBoardId;
        createdData.GetComponent<NetworkObject>().Spawn(false);
    }

    public override void OnNetworkSpawn()
    {
        SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);

    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ulong playerId)
    {
        var spawn = Instantiate(playerPrefab);

        ChessGen_Test chessGen = spawn.GetComponent<ChessGen_Test>();
        playerList.Insert((int)playerId, chessGen);
        chessGen.pieceManager = chessPieceManager;
        chessGen.boardGenerator = chesGen;
        spawn.NetworkObject.SpawnWithOwnership(playerId);
        SetClientRpc(spawn.NetworkObject);
        ChessGen_Test.OnSetModeSet += StartGameServerRpc;
        playerCount++;
    }

    [ClientRpc]
    private void SetClientRpc(NetworkObjectReference target)
    {
        if (target.TryGet(out NetworkObject targetObject))
        {
            ChessGen_Test chessGen = targetObject.GetComponent<ChessGen_Test>();
            chessGen.pieceManager = chessPieceManager;
            chessGen.boardGenerator = chesGen;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerTurnServerRpc()
    {

        playerList[turnNumber].isMyTurnNet.Value = false;

        turnNumber++;
        if (turnNumber + 1 > playerCount)
            turnNumber = 0;
        if (playerList[turnNumber].currentSetModeNet.Value == SetMode.GameOver)
            SetPlayerTurnServerRpc();


        playerList[turnNumber].isMyTurnNet.Value = true;

        chessPieceManager.SetTilesInCheck();
        chessPieceManager.SetActiveMoveList();

    }

    public void CheckGameOver(ChessGen_Test chess)
    {
        if (playerActiveList.Contains(chess))
        {
            chess.currentSetModeNet.Value = SetMode.GameOver;
            playerActiveList.Remove(chess);
        }
        if (playerActiveList.Count == 1)
        {
            playerActiveList[0].currentSetModeNet.Value = SetMode.GameOver;
            foreach (ChessGen_Test player in playerList)
            {
                player.CheckMateClientRpc(playerActiveList[0].teamNumber.Value);
            }
        }
    }


    private bool CheckForGameRestart()
    {
        foreach (ChessGen_Test player in playerList)
        {
            if (!player.retryBool.Value)
                return false;
        }
        return true;
    }
    public void GameRestart()
    {
        if (CheckForGameRestart())
        { if (IsServer)
                ResetingGame();
        }
    }
    public void GameEnd()
    {
        if (CheckForGameEnd())
        {
            if (IsServer)
                EndingGameClientRpc();
        }
    }

    private bool CheckForGameEnd()
    {
        foreach (ChessGen_Test player in playerList)
        {
            if (player.endBool.Value)
                return true;
        }
        return false;
    }

    private void ResetingGame()
    {
        isRestarting = true;
        //DontDestroyOnLoad(DataSend.instance.gameObject);
        using (new Load("Restarting Game..."))
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
    }

    [ClientRpc]
    private void EndingGameClientRpc()
    {
        isRestarting = false;
       // DontDestroyOnLoad(DataSend.instance.gameObject);
        using (new Load("Leaving Game..."))
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Authentication", LoadSceneMode.Single);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc()
    {
        if (CheckSetMode())
        {
            turnNumber = 0;
            playerList[turnNumber].isMyTurnNet.Value = true;
            chessPieceManager.SetTilesInCheck();
            ChessGen_Test.OnSetModeSet -= StartGameServerRpc;
        }
    }

    private bool CheckSetMode()
    {
        foreach (ChessGen_Test chester in playerList)
        {
            if (chester.currentSetModeNet.Value != SetMode.Set)
            {
                Debug.Log("not set");
                return false;
            }
            else
                playerActiveList.Add(chester);
        }
        Debug.Log("All set");
        return true;
    }

    public void ChangeState(GameState newState)
    {
        OnBeforeStateChanged?.Invoke(newState);

        state = newState;
        switch (newState)
        {
            case GameState.Starting:
                break;
            case GameState.Victory:
                break;
            case GameState.Lose:
                break;
            case GameState.MainMenu:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnAfterStateChanged?.Invoke(newState);
    }

    public void GetPlayerCount(int playerNum) => playerCount = playerNum;

    private void OnGameEnd()
    {

    }
}
