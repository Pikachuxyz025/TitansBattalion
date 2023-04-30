using System.Collections;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerTerritorySpawn : ChessGenerator, IMainBoardInfo
{
    private Points[] armyEdgedPoints;
    private ChessPieceConnection[] armyEdgedobjects;

     private List<GameObject> spawnedObjects = new List<GameObject>();
    private List<ChessPieceConnection> connections = new List<ChessPieceConnection>();

    public NetworkVariable<int> teamNumber = new NetworkVariable<int>(0);

    [SerializeField] private int mainBoardOffsetX;
    [SerializeField] private int mainBoardOffsetY;

    [SerializeField] private GameManager gameManager;
    [SerializeField] private ChessboardGenerator boardGenerator;
    [SerializeField] private Player controllingPlayer;

    #region Private/Protected
    private void AssignRookToKing(King king)
    {
        if (king == null) return;

        controllingPlayer.SetCurrentKing(king);

        foreach (GameObject _object in spawnedObjects)
        {
            if (_object.GetComponent<Chesspiece>() is Rook)
            {
                Rook rook = _object.GetComponent<Rook>();
                king.rooks.Add(rook);
                rook.SetKing(king);
            }
        }
    }

    [ServerRpc]
    private void ControlAServerRpc()
    {
        //Adjust the X and Y of the coordinates
        switch (teamNumber.Value)
        {
            case 1:
                // if boardedge d > armyedge a

                if (gameManager.GetMainBoard().TileEdgedPoints[3].X > armyEdgedobjects[0].CurrentTilePoint().X)
                {
                    gameObject.transform.position += Vector3.right;

                    foreach (ChessPieceConnection connection in connections)
                    {
                        // Change the alter grid movement for each player.
                        connection.AlterGrid(1, 0);
                    }
                }
                break;

            case 2:
                // if boardedge a < armyedge a
                if (gameManager.GetMainBoard().TileEdgedPoints[0].X < armyEdgedobjects[0].CurrentTilePoint().X)
                {
                    gameObject.transform.position += Vector3.left;

                    foreach (ChessPieceConnection connection in connections)
                    {
                        // Change the alter grid movement for each player.
                        connection.AlterGrid(-1, 0);
                    }
                }
                break;
            case 3:
                gameObject.transform.position += Vector3.back;

                foreach (ChessPieceConnection connection in connections)
                {
                    // Change the alter grid movement for each player.
                    connection.AlterGrid(0, -1);
                }
                break;
            case 4:
                gameObject.transform.position += Vector3.forward;

                foreach (ChessPieceConnection connection in connections)
                {
                    // Change the alter grid movement for each player.
                    connection.AlterGrid(0, 1);
                }
                break;
        }
    }

    [ServerRpc]
    private void ControlDServerRpc()
    {
        // Adjust the X and Y of the coordinates
        switch (teamNumber.Value)
        {
            case 1:
                // if boardedge c < armyedge b
                if (gameManager.GetMainBoard().TileEdgedPoints[2].X < armyEdgedobjects[1].CurrentTilePoint().X)
                {
                    gameObject.transform.position += Vector3.left;

                    foreach (ChessPieceConnection connection in connections)
                    {
                        // Change the alter grid movement for each player.
                        connection.AlterGrid(-1, 0);
                    }
                }
                break;
            case 2:
                // if boardedge b > armyedge b
                if (gameManager.GetMainBoard().TileEdgedPoints[1].X > armyEdgedobjects[1].CurrentTilePoint().X)
                {
                    gameObject.transform.position += Vector3.right;

                    foreach (ChessPieceConnection connection in connections)
                    {
                        // Change the alter grid movement for each player.
                        connection.AlterGrid(1, 0);
                    }
                }
                break;
            case 3:
                gameObject.transform.position += Vector3.forward;

                foreach (ChessPieceConnection connection in connections)
                {
                    // Change the alter grid movement for each player.
                    connection.AlterGrid(0, 1);

                }
                break;
            case 4:
                gameObject.transform.position += Vector3.back;

                foreach (ChessPieceConnection connection in connections)
                {
                    // Change the alter grid movement for each player.
                    connection.AlterGrid(0, -1);
                }
                break;
        }
    }

    private void CreateTerritory()
    {
        GenerateAllTilesServerRpc(teamNumber.Value);
        controllingPlayer.SetModeChangeServerRpc(SetMode.Spawned);
    }

    protected override GameObject GenerateSingleTile(ref int x, ref int y)
    {
        int x_R = 0;
        int y_R = 0;
        GameObject tileObject = Instantiate(piece);

        // Add to setup tiles to setup piece placement later
        setupTiles.Add(new Points(x, y), tileObject);
        if (armyEdgedPoints.Length > 0)
        {
            for (int i = 0; i < armyEdgedPoints.Length; i++)
            {
                if (setupTiles.ContainsKey(armyEdgedPoints[i]))
                    armyEdgedobjects[i] = setupTiles[armyEdgedPoints[i]].GetComponent<ChessPieceConnection>();

            }
        }
        switch (teamNumber.Value)
        {
            case 1:
                x_R = mainBoardOffsetX - x - 1;
                y_R = mainBoardOffsetY + (chessboard.tileCountY - 1) - y;
                break;
            case 2:
                x_R = x;
                y_R = y - chessboard.tileCountY;
                break;
            case 3:
                x_R = y + mainBoardOffsetX;
                y_R = x;
                break;
            case 4:
                x_R = y - chessboard.tileCountY;
                y_R = mainBoardOffsetY - x - 1;
                break;
        }

        x = x_R;
        y = y_R;
        return tileObject;
    }

    private void MoveChessboard()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ControlAServerRpc();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            ControlDServerRpc();
        }
    }

    [ServerRpc]
    private void PositionAllPiecesServerRpc()
    {
        if (spawnedObjects.Count != chessboard.prefabs.Count)
            return;

        King king = null;

        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            Points setPoint = chessboard.armyCoordinates[i];
            GameObject setobject = spawnedObjects[i];

            if (setobject.GetComponent<Chesspiece>() is King)
                king = setobject.GetComponent<King>();

            if (setupTiles.ContainsKey(setPoint))
            {
                Chesspiece newChesspiece = setobject.GetComponent<Chesspiece>();
                ChessPieceConnection boardPiece = setupTiles[setPoint].GetComponent<ChessPieceConnection>();

                newChesspiece.SetGameMode(controllingPlayer.currentGameMode.Value);
                Vector3 pos = boardPiece.pieceSpawnPoint.transform.position;
                newChesspiece.SetPositionServerRpc(boardPiece.GridX.Value, boardPiece.GridY.Value, pos, true);
                newChesspiece.gameObject.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);
                boardPiece.SetOccupiedPiece(newChesspiece);
            }
        }

        AssignRookToKing(king);
    }
    [ServerRpc(RequireOwnership = false)] // Sets up event for changing mainboardOffset
    private void SetBoardGeneratorServerRpc() => boardGenerator.ChangeValue += InsertMainBoardInfo;

    [ServerRpc]
    private void SpawnAllPiecesServerRpc()
    {
        if (chessboard.prefabs.Count == 0) return;
        for (int i = 0; i < chessboard.prefabs.Count; i++)
        {
            GameObject setobject = SpawnSinglePiece(chessboard.prefabs[i]);
            setobject.GetComponent<NetworkObject>().Spawn();

            spawnedObjects.Add(setobject);

            Chesspiece newChesspiece = setobject.GetComponent<Chesspiece>();
            newChesspiece.SetupPiece(teamNumber.Value, controllingPlayer);
        }
    }

    private void SpawnPieces()
    {
        SpawnAllPiecesServerRpc();
        PositionAllPiecesServerRpc();
        controllingPlayer.SetModeChangeServerRpc(SetMode.Set);
        gameManager.StartGameServerRpc();
    }

    private GameObject SpawnSinglePiece(GameObject reference)
    {
        GameObject spawnedObject = Instantiate(reference);

        return spawnedObject;
    }

    private void Update()
    {
        if (!IsOwner)
            return;
        if (!controllingPlayer.NetworkIsMyTurn.Value)
            return;
        if (controllingPlayer.currentSetModeNet.Value == SetMode.Spawned) MoveChessboard();
        if (Input.GetKeyDown(KeyCode.C))
        {
            switch (controllingPlayer.currentSetModeNet.Value)
            {
                case SetMode.NotSpawned:
                    CreateTerritory();
                    break;
                case SetMode.Spawned:
                    SpawnPieces();
                    break;
            }
        }
    }

    #endregion

    #region Public
    public void CreatePieceList(ChessPieceConnection connection) => connections.Add(connection);

    public List<GameObject> GetSpawnedObjects()
    {
        return spawnedObjects;
    }

    public void InsertMainBoardInfo(int x, int y)
    {
        mainBoardOffsetX = x;
        mainBoardOffsetY = y;
    }

    public override void OnNetworkSpawn()
    {
        SetBoardGeneratorServerRpc();
        gameManager = GameManager.instance;
        boardGenerator = ChessboardGenerator.Instance;
    }
    public void RemoveChesspieces()
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            GameObject destroyedChesspiece = spawnedObjects[i];
            spawnedObjects[i] = null;
            Destroy(destroyedChesspiece);
        }
        List<Points> listd = new List<Points>(setupTiles.Keys);
        for (int i = 0; i < listd.Count; i++)
        {
            GameObject destroyedChesspiece = setupTiles[listd[i]];
            setupTiles[listd[i]] = null;
            Destroy(destroyedChesspiece);
        }
    }

    public void SetChessboardTemplate(ChessboardTemplate chessTemp)
    {
        chessboard = chessTemp;
        armyEdgedPoints = chessboard.TileEdgedPoints;
        armyEdgedobjects = new ChessPieceConnection[armyEdgedPoints.Length];
    }
    public void SetupVariables(int _teamNumber, ChessboardGenerator _chessGen, GameManager _gameManager)
    {
        teamNumber.Value = _teamNumber;
        if (!boardGenerator)
            boardGenerator = _chessGen;
        if (!gameManager)
            gameManager = _gameManager;
    }
    #endregion
}
