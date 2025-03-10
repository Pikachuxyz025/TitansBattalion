using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public enum GameState
{
    None = 0,
    Loading = 1,
    Running = 2,
    End = 3,
    Restart = 4,
}

public class GameManager : NetworkBehaviour
{
    public static event Action<GameState> OnBeforeStateChanged;
    public static event Action<GameState> OnAfterStateChanged;
    public static event Action GameReset;
    public static event Action GameEnded;


    [SerializeField] private ChessboardTileManager chessPieceManager;
    [SerializeField] private CentralBoardGenerator chesGen;

    public List<Player> playerList = new List<Player>();
    [SerializeField] private List<Player> playerActiveList = new List<Player>();
    public int mainBoardId;

    [SerializeField] private NetworkVariable<bool> gameStarted=new NetworkVariable<bool>(false);
    [SerializeField] private Player playerPrefab;
    [SerializeField] private DataSend dataSend;

    public GameState currentState;
    private bool isRestarting=false;
    public static GameManager instance;
    private int playerCount = 0;
    private int turnNumber;

    private void Awake()
    {
        instance = this;
        GameReset += ResetingGame;
        GameEnded += EndingGame;
        //GameEnded += LeavingGame;
    }

    public ChessboardTemplate GetMainBoard()
    {
        return chesGen.chessboard;
    }

    public override void OnDestroy()
    {

        //GameReset -= ResetingGame;
        //GameEnded -= EndingGameClientRpc;
        //GameEnded -= LeavingGame;
    }

    private async void LeavingGame()
    {
        await MatchmakingService.LeaveLobby();
        NetworkManager.Singleton.Shutdown();
    }

    public override void OnNetworkSpawn()
    {
        SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);

    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ulong playerId)
    {
        var spawn = Instantiate(playerPrefab);

        Player chessGen = spawn.GetComponent<Player>();
        playerList.Add(chessGen);
        chessGen.SetupVariables(DataSend.boardData, playerList.IndexOf(chessGen) + 1);
        chessGen.GetPlayerTerritorySpawn().SetupVariables(playerList.IndexOf(chessGen) + 1, chesGen, this);
        spawn.NetworkObject.SpawnWithOwnership(playerId);
        playerCount++;

        if (playerCount > 1)        
            SetupGameServerRpc();        
    }


    [ClientRpc]
    private void SetClientRpc(NetworkObjectReference target,ulong playerId)
    {
        if (target.TryGet(out NetworkObject targetObject))
        {
            Player chessGen = targetObject.GetComponent<Player>();
        }
    }



    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerTurnServerRpc()
    {
        playerList[turnNumber].NetworkIsMyTurn.Value = false;
        chessPieceManager.SetActiveMoveList(playerList[turnNumber]);

        turnNumber++;
        if (turnNumber + 1 > playerCount)
            turnNumber = 0;
        if (playerList[turnNumber].currentSetModeNet.Value == SetMode.GameOver)
            SetPlayerTurnServerRpc();


        playerList[turnNumber].NetworkIsMyTurn.Value = true;

        if (!gameStarted.Value)
            return;
        chessPieceManager.SetTilesInCheck();
        //playerList[turnNumber].EnterTurnServerRpc();
    }

    public void CheckGameOver(Player chess)
    {
        if (playerActiveList.Contains(chess))
        {
            chess.currentSetModeNet.Value = SetMode.GameOver;
            playerActiveList.Remove(chess);
        }
        if (playerActiveList.Count == 1)
        {
            playerActiveList[0].currentSetModeNet.Value = SetMode.GameOver;
            foreach (Player player in playerList)
            {
                player.CheckMateClientRpc(playerActiveList[0].teamNumber.Value);
            }
        }
    }


    private bool CheckForGameRestart()
    {
        foreach (Player player in playerList)
        {
            if (!player.NetworkRetryBool.Value)
                return false;
        }
        return true;
    }
    public void GameRestart()
    {
        if (CheckForGameRestart())
        {
            if (IsServer)
                GameReset?.Invoke();
        }
    }

    public void GameEnd()
    {
        if (CheckForGameEnd())
        {
            if (IsServer)
                GameEnded?.Invoke();
        }
    }

    private bool CheckForGameEnd()
    {
        foreach (Player player in playerList)
        {
            if (player.NetworkEndBool.Value)
                return true;
        }
        return false;
    }

    private void ResetingGame()
    {
        isRestarting = true;
        if (IsServer)
            DestroyPlayers();


        NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);

    }


    private void DestroyPlayers()
    {
        if (playerList.Count > 0)
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                Player player = playerList[i];
                playerList[i] = null;
                if (player != null)
                {
                    player.GetPlayerTerritorySpawn().RemoveChesspieces();
                    Destroy(player.gameObject);
                }
            }
        }
    }

    private void EndingGame()
    {
        isRestarting = false;
        if (IsServer)       
            DestroyPlayers();
            NetworkManager.Singleton.SceneManager.LoadScene("Login Menu", LoadSceneMode.Single);
               
    }


    [ServerRpc(RequireOwnership =false)]
    public void SetupGameServerRpc()
    {
        turnNumber = 1;
        playerList[turnNumber].NetworkIsMyTurn.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc()
    {
        if (CheckSetMode())
        {
            SetPlayerTurnServerRpc();
            chessPieceManager.SetupTiles();
            chessPieceManager.SetTilesInCheck();
            gameStarted.Value = true;
        }
        else
            SetPlayerTurnServerRpc();
    }

    public bool HasGameStarted()
    {
        return gameStarted.Value;
    }

    private bool CheckSetMode()
    {
        foreach (Player chester in playerList)
        {
            if (chester.currentSetModeNet.Value != SetMode.Set)
                return false;
            else
                playerActiveList.Add(chester);
        }
        return true;
    }
}
