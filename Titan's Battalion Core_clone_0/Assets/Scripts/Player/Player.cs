using System.Collections;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum SetMode
{
    SelectArmy,
    NotSpawned,
    FlipMode,
    Spawned,
    Set,
    GameOver,
}

public class Player : ChessGenerator, IMainBoardInfo
{
    [Header("Editor Filled Variables")]
    [SerializeField] private ChessboardGenerator boardGenerator;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private TextMeshProUGUI whoWin;
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private GameObject checkmateButton;
    [SerializeField] private List<ChessboardTemplate> armyBoardList = new List<ChessboardTemplate>();

    [Header("board check")]
    [SerializeField] private GameModeTerritory gameModeTerritory = new GameModeTerritory();

    [Header("Script Filled Variables")]
    [SerializeField] private Points[] armyEdgedPoints;
    [SerializeField] private ChessPieceConnection[] armyEdgedobjects;

    [SerializeField] private Camera currentCamera;

    public CheckableList playerCheckableList { get; private set; } = new CheckableList();

    [SerializeField] private List<GameObject> spawnedObjects = new List<GameObject>();
    [SerializeField] private List<ChessPieceConnection> connections = new List<ChessPieceConnection>();


    public NetworkVariable<int> teamNumber = new NetworkVariable<int>(0);
    [SerializeField] private int mainBoardOffsetX;
    [SerializeField] private int mainBoardOffsetY;


    public NetworkVariable<bool> NetworkRetryBool = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> NetworkEndBool = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> NetworkIsKingInCheck = new NetworkVariable<bool>(false);
    [SerializeField] private King currentKing;

    [SerializeField] private Vector3[] setCameraTransform;
    [SerializeField] private Vector3[] setCameraRotation;
    public NetworkVariable<SetMode> currentSetModeNet = new NetworkVariable<SetMode>(SetMode.SelectArmy);
    public NetworkVariable<GameMode> currentGameMode = new NetworkVariable<GameMode>(GameMode.None);

    public NetworkVariable<bool> NetworkIsMyTurn = new NetworkVariable<bool>(false);

    [SerializeField] private Interaction interaction;
    [SerializeField] private PlayerUI _playerUI;
    [SerializeField] private PlayerTerritorySpawn playerTerritorySpawn;

    private bool isMyTurn
    {
        get { return NetworkIsMyTurn.Value; }
    }
    public bool isKingInCheck
    {
        get { return NetworkIsKingInCheck.Value; }
        private set { }
    }

    bool isMoving = false;
    public static event Action OnSetModeSet;

    public void SetupVariables(GameMode _mode, int _teamNumber, ChessboardGenerator _chessGen)
    {
        currentGameMode.Value = _mode;
        teamNumber.Value = _teamNumber;
        boardGenerator = _chessGen;
    }

    [ServerRpc(RequireOwnership = false)]
    public void EnterTurnServerRpc()
    {
        if (gameManager.HasGameStarted())
            ActiveCheckMateServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    public void ActiveCheckMateServerRpc() // player should keep this script to connect with game manager and player ui 
    {
        ClientRpcParams clientRpcParams;

        clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {

                TargetClientIds = new ulong[] { OwnerClientId }
            }
        };

        if (currentKing == null)
            return;
        if (currentKing.CompleteKingCheckmate())
        {
            gameManager.CheckGameOver(this);
            return;
        }

        if (currentKing.SetCheckableList())
        {
            ActivateCheckmateButtonClientRpc(true, clientRpcParams);
            NetworkIsKingInCheck.Value = true;
        }
        else
            ActivateCheckmateButtonClientRpc(false, clientRpcParams);

    }

    [ClientRpc]
    public void ActivateCheckmateButtonClientRpc(bool set, ClientRpcParams clientRpc = default) => checkmateButton.SetActive(set);

    [ServerRpc(RequireOwnership = false)]
    public void ExitTurnServerRpc() { }

    public void CameraSetup()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            interaction.SetCurrentCamera(currentCamera);
            if (IsOwner)
            {
                switch (teamNumber.Value)
                {
                    case 1:
                        currentCamera.transform.position = setCameraTransform[0];
                        currentCamera.transform.rotation = Quaternion.Euler(setCameraRotation[0].x, setCameraRotation[0].y, setCameraRotation[0].z);
                        break;
                    case 2:
                        currentCamera.transform.position = setCameraTransform[1];
                        currentCamera.transform.rotation = Quaternion.Euler(setCameraRotation[1].x, setCameraRotation[1].y, setCameraRotation[1].z);
                        break;
                    case 3:
                        currentCamera.transform.position = setCameraTransform[2];
                        currentCamera.transform.rotation = Quaternion.Euler(setCameraRotation[2].x, setCameraRotation[2].y, setCameraRotation[2].z);
                        break;
                    case 4:
                        currentCamera.transform.position = setCameraTransform[3];
                        currentCamera.transform.rotation = Quaternion.Euler(setCameraRotation[3].x, setCameraRotation[3].y, setCameraRotation[3].z);
                        break;
                }
            }
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        CameraSetup();

        if (IsOwner)
        {
            HandleInput();
        }
    }
    void HandleInput()
    {
        switch (currentSetModeNet.Value)
        {
            case SetMode.NotSpawned:
                if (!isMyTurn)
                    return;
                if (Input.GetKeyDown(KeyCode.C))
                {
                    // Debugging based on player number
                    GenerateAllTilesServerRpc(teamNumber.Value);

                    SetModeChangeServerRpc(SetMode.Spawned);
                    //currentSetMode = SetMode.Spawned;
                }
                break;
            case SetMode.Spawned:
                if (!isMyTurn)
                    return;
                MoveChessboard();
                if (Input.GetKeyDown(KeyCode.C))
                {
                    SpawnAllPiecesServerRpc();
                    PositionAllPiecesServerRpc();
                    SetModeChangeServerRpc(SetMode.Set);
                    gameManager.StartGameServerRpc();
                    //currentSetMode = SetMode.Set;
                }
                break;
        }
    }
    #region Starting The Game

    public override void OnNetworkSpawn()
    {
        SetBoardGeneratorServerRpc();
        Setupd();

        if (currentGameMode.Value == GameMode.None)
            return;
        

        if (currentGameMode.Value != GameMode.Chess)
        {
            if (IsOwner)
                playerUI.SetActive(true);
        }
        else
        {
            SetChessIdServerRpc(currentGameMode.Value);
            SetModeChangeServerRpc(SetMode.NotSpawned);
        }

    }

    [ServerRpc(RequireOwnership = false)] // Sets up event for changing mainboardOffset
    public void SetBoardGeneratorServerRpc() => boardGenerator.ChangeValue += InsertMainBoardInfo;

    // Finds the gamemanager in scene (Server only)
    private void Setupd() => gameManager = GameManager.instance;

    // Takes the dropdown value of the armyboard list and selects territory
    // Attached to button press in game
    public void SetupTerritory()
    {
        if (armyBoardList.Count >= dropdown.value)
            SetChessIdServerRpc(dropdown.value);
        else
            return;

        if (IsOwner)
            SetModeChangeServerRpc(SetMode.NotSpawned);
    }

    public List<ChessboardTemplate> GetArmyBoardList()
    {
        return armyBoardList;
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetChessIdServerRpc(int id)
    {
        Debug.Log(id);
        chessboard = armyBoardList[id];
        armyEdgedPoints = chessboard.TileEdgedPoints;
        armyEdgedobjects = new ChessPieceConnection[armyEdgedPoints.Length];
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetChessIdServerRpc(GameMode mode)
    {
        chessboard = gameModeTerritory.GetTerritory(mode);
        armyEdgedPoints = chessboard.TileEdgedPoints;
        armyEdgedobjects = new ChessPieceConnection[armyEdgedPoints.Length];
    }

    [ServerRpc]
    public void PositionAllPiecesServerRpc()
    {
        if (spawnedObjects.Count != chessboard.prefabs.Count)
        {
            //Debug.Log("Set is off");
            return;
        }
        King king = null;

        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            Points setPoint = chessboard.armyCoordinates[i];
            GameObject setobject = spawnedObjects[i];

            if (setobject.GetComponent<Chesspiece>() is King)
                king = setobject.GetComponent<King>();

            if (setupTiles.ContainsKey(setPoint))
            {
                //Debug.Log("set Found " + setPoint.X + ", " + setPoint.Y);

                Chesspiece cp = setobject.GetComponent<Chesspiece>();
                ChessPieceConnection tp = setupTiles[setPoint].GetComponent<ChessPieceConnection>();
                cp.SetGameMode(currentGameMode.Value);
                Vector3 pos = tp.pieceSetPoint.transform.position;
                cp.SetPositionServerRpc(tp.GridX.Value, tp.GridY.Value, pos, true);
                cp.gameObject.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);
                tp.SetOccupiedPiece(cp);
                //PositionSinglePiece(setobject, setupTiles[setPoint], true);
            }
        }

        AssignRookToKing(king);
    }

    public void AssignRookToKing(King king)
    {
        if (king == null) return;

        currentKing = king;

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

    public void SetCurrentKing(King king)
    { currentKing = king; }
    #endregion


    #region Ending The Game

    [ServerRpc(RequireOwnership = false)]
    public void ChangeRetyBoolServerRpc(bool value)
    {
        NetworkRetryBool.Value = value;
        gameManager.GameRestart();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeEndBoolServerRpc(bool value)
    {
        NetworkEndBool.Value = value;
        gameManager.GameEnd();
    }

    public void RemoveChessPieces()
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            GameObject pm = spawnedObjects[i];
            spawnedObjects[i] = null;
            Destroy(pm);
        }
        List<Points> listd = new List<Points>(setupTiles.Keys);
        for (int i = 0; i < listd.Count; i++)
        {
            GameObject gm = setupTiles[listd[i]];
            setupTiles[listd[i]] = null;
            Destroy(gm);
        }
    }

    [ServerRpc]
    public void OnCheckMateServerRpc()
    {
        gameManager.CheckGameOver(this);
    }

    [ClientRpc]
    public void CheckMateClientRpc(int team)
    {
        if (IsOwner)
            DisplayVictory(team);
    }

    private void DisplayVictory(int winningTeam)
    {
        checkmateButton.gameObject.SetActive(false);
        winScreen.SetActive(true);
        whoWin.text = new string("Player " + winningTeam + " Wins!");
    }
    #endregion

    [ServerRpc(RequireOwnership = false)]
    public void SetModeChangeServerRpc(SetMode set) => currentSetModeNet.Value = set;

    void MoveChessboard()
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
    void ControlAServerRpc()
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
                        //connection.AlterGridClientRpc(1, 0);
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
                        //connection.AlterGridClientRpc(-1, 0);
                    }
                }
                break;
            case 3:
                    gameObject.transform.position += Vector3.back;

                foreach (ChessPieceConnection connection in connections)
                {
                    // Change the alter grid movement for each player.
                    connection.AlterGrid(0, -1);
                    //connection.AlterGridClientRpc(0, -1);
                }
                break;
            case 4:
                gameObject.transform.position += Vector3.forward;

                foreach (ChessPieceConnection connection in connections)
                {
                    // Change the alter grid movement for each player.
                    connection.AlterGrid(0, 1);
                    //connection.AlterGridClientRpc(0, 1);
                }
                break;
        }
    }

    [ServerRpc]
    void ControlDServerRpc()
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
                        //connection.AlterGridClientRpc(-1, 0);
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
                        //.AlterGridClientRpc(1, 0);
                    }
                }
                break;
            case 3:
                gameObject.transform.position += Vector3.forward;

                foreach (ChessPieceConnection connection in connections)
                {
                    // Change the alter grid movement for each player.
                    connection.AlterGrid(0, 1);
                    //connection.AlterGridClientRpc(0, 1);
                }
                break;
            case 4:
                gameObject.transform.position += Vector3.back;

                foreach (ChessPieceConnection connection in connections)
                {
                    // Change the alter grid movement for each player.
                    connection.AlterGrid(0, -1);
                    //connection.AlterGridClientRpc(0, -1);
                }
                break;
        }
    }

    public void InsertMainBoardInfo(int x, int y)
    {
        mainBoardOffsetX = x;
        mainBoardOffsetY = y;
    }

    public void CreatePieceList(ChessPieceConnection connection)
    {
        connections.Add(connection);
    }

    [ServerRpc]
    public void SpawnAllPiecesServerRpc()
    {
        if (chessboard.prefabs.Count == 0) return;
        for (int i = 0; i < chessboard.prefabs.Count; i++)
        {
            GameObject setobject = SpawnSinglePiece(chessboard.prefabs[i]);
            setobject.GetComponent<NetworkObject>().Spawn();

            spawnedObjects.Add(setobject);

            Chesspiece newChesspiece = setobject.GetComponent<Chesspiece>();
            newChesspiece.SetupPiece(teamNumber.Value,this);
        }
    }

    public GameObject SpawnSinglePiece(GameObject reference)
    {
        GameObject spawnedObject = Instantiate(reference);

        return spawnedObject;
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
}
