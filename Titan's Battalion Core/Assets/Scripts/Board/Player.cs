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
   [SerializeField] private ChessPieceManager pieceManager;
    [SerializeField] private ChessboardGenerator boardGenerator;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private TextMeshProUGUI whoWin;
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private GameObject checkmateButton;
    [SerializeField] private List<Chessboard_Testing> armyBoardList = new List<Chessboard_Testing>();
    [SerializeField] private float yoffset = .2f;
    [SerializeField] private float dragOffset = 1.5f;

    [Header("board check")]
    [SerializeField] private GameModeTerritory gameModeTerritory = new GameModeTerritory();

    [Header("Script Filled Variables")]
    [SerializeField] private Points[] armyEdgedPoints;
    [SerializeField] private ChessPieceConnection[] armyEdgedobjects;

    [SerializeField] private Camera currentCamera;
    [SerializeField] private Chesspiece currentlyDragging;
    [SerializeField] private Points currentHover = new Points(1984987, 51684);

    [SerializeField] private List<Points> availableMoves = new List<Points>();
    [SerializeField] private List<Points> availableSpecialMoves = new List<Points>();

    [SerializeField] private List<GameObject> spawnedObject = new List<GameObject>();
    [SerializeField] private List<ChessPieceConnection> connections = new List<ChessPieceConnection>();


    public NetworkVariable<int> teamNumber = new NetworkVariable<int>(0);
    [SerializeField] private int mainBoardOffsetX;
    [SerializeField] private int mainBoardOffsetY;


    public NetworkVariable<bool> retryBool = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> endBool = new NetworkVariable<bool>(false);
    [SerializeField] private King currentKing;


    [SerializeField] private bool setCurrentDrag = false;
    [SerializeField] private Vector3[] setCameraTransform;
    [SerializeField] private Vector3[] setCameraRotation;
    public NetworkVariable<SetMode> currentSetModeNet = new NetworkVariable<SetMode>(SetMode.SelectArmy);
    public NetworkVariable<GameMode> currentGameMode = new NetworkVariable<GameMode>(GameMode.None);

    public NetworkVariable<bool> isMyTurnNet = new NetworkVariable<bool>(false);
    private bool isMyTurn
    {
        get { return isMyTurnNet.Value; }
    }
    bool isMoving = false;
    public static event Action OnSetModeSet;

    [ServerRpc]
    public void ResetCurrentHoverServerRpc()
    {
        if (pieceManager.IsCoordinateInList(currentHover))
        {
            pieceManager.SwapLayerServerRpc(currentHover.X, currentHover.Y, "Tile", OwnerClientId);
            currentHover = new Points(1984987, 51684);
        }
    }

    public void SetupVariables(GameMode _mode, int _teamNumber, ChessPieceManager _pieceManager, ChessboardGenerator _chessGen)
    {
        currentGameMode.Value = _mode;
        teamNumber.Value = _teamNumber;
        pieceManager = _pieceManager;
        boardGenerator = _chessGen;
    }

    // Update is called once per frame
    void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
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

        if (IsOwner)
        {
            HandleInput();
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
    private void Setupd() => gameManager = FindObjectOfType<GameManager>();

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

    [ServerRpc(RequireOwnership = false)]
    void SetChessIdServerRpc(int id)
    {
        Debug.Log(id);
        chessboard = armyBoardList[id];
        armyEdgedPoints = chessboard.TileEdgedPoints;
        armyEdgedobjects = new ChessPieceConnection[armyEdgedPoints.Length];
    }

    [ServerRpc(RequireOwnership = false)]
    void SetChessIdServerRpc(GameMode mode)
    {
        chessboard = gameModeTerritory.GetTerritory(mode);
        armyEdgedPoints = chessboard.TileEdgedPoints;
        armyEdgedobjects = new ChessPieceConnection[armyEdgedPoints.Length];
    }

    [ServerRpc]
    public void PositionAllPiecesServerRpc()
    {
        if (spawnedObject.Count != chessboard.prefabs.Count)
        {
            //Debug.Log("Set is off");
            return;
        }
        King king = null;

        for (int i = 0; i < spawnedObject.Count; i++)
        {
            Points setPoint = chessboard.armyCoordinates[i];
            GameObject setobject = spawnedObject[i];

            if (setobject.GetComponent<Chesspiece>() is King)
                king = setobject.GetComponent<King>();

            if (setupTiles.ContainsKey(setPoint))
            {
                //Debug.Log("set Found " + setPoint.X + ", " + setPoint.Y);

                Chesspiece cp = setobject.GetComponent<Chesspiece>();
                ChessPieceConnection tp = setupTiles[setPoint].GetComponent<ChessPieceConnection>();
                Vector3 pos = tp.pieceSetPoint.transform.position;
                cp.SetPositionServerRpc(tp.GridX.Value, tp.GridY.Value, pos, true);
                cp.gameObject.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);
                tp.SetOccupiedPieceClientRpc(cp.GetComponent<NetworkObject>());
                //PositionSinglePiece(setobject, setupTiles[setPoint], true);
            }
        }

        AssignRookToKing(king);
    }

    void AssignRookToKing(King king)
    {
        if (king == null) return;

        currentKing = king;

        foreach (GameObject _object in spawnedObject)
        {
            if (_object.GetComponent<Chesspiece>() is Rook)
            {
                Rook rook = _object.GetComponent<Rook>();
                king.rooks.Add(rook);
                rook.SetKing(king);
            }
        }
    }
    #endregion

    #region Ending The Game

    [ServerRpc(RequireOwnership = false)]
    public void ChangeRetyBoolServerRpc(bool value)
    {
        retryBool.Value = value;
        gameManager.GameRestart();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeEndBoolServerRpc(bool value)
    {
        endBool.Value = value;
        gameManager.GameEnd();
    }


    [ServerRpc(RequireOwnership = false)]
    public void ActiveCheckMateServerRpc()
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

        if (currentKing.IsInCheck())
            ActivateCheckmateButtonClientRpc(true, clientRpcParams);
        else
            ActivateCheckmateButtonClientRpc(false, clientRpcParams);

    }

    [ClientRpc]
    public void ActivateCheckmateButtonClientRpc(bool set, ClientRpcParams clientRpc = default) => checkmateButton.SetActive(set);

    public void RemoveChessPieces()
    {
        for (int i = 0; i < spawnedObject.Count; i++)
        {
            GameObject pm = spawnedObject[i];
            spawnedObject[i] = null;
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



    void HandleInput()
    {
        switch (currentSetModeNet.Value)
        {
            case SetMode.NotSpawned:
                if (Input.GetKeyDown(KeyCode.C))
                {
                    // Debugging based on player number
                    GenerateAllTilesServerRpc(teamNumber.Value);

                    SetModeChangeServerRpc(SetMode.Spawned);
                    //currentSetMode = SetMode.Spawned;
                }
                break;
            case SetMode.Spawned:
                MoveChessboard();
                if (Input.GetKeyDown(KeyCode.C))
                {
                    SpawnAllPiecesServerRpc();
                    PositionAllPiecesServerRpc();
                    SetConnectionServerRpc();
                    SetModeChangeServerRpc(SetMode.Set);
                    gameManager.StartGameServerRpc();
                    //currentSetMode = SetMode.Set;
                }
                break;
            case SetMode.Set:
                if (isMyTurn)
                    Interact();
                else
                    ResetCurrentHoverServerRpc();
                if (gameManager.HasGameStarted())
                    ActiveCheckMateServerRpc();
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetModeChangeServerRpc(SetMode set) => currentSetModeNet.Value = set;

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
    public void SetConnectionServerRpc()
    {
        foreach (ChessPieceConnection connection in connections)
            connection.n_isConnected.Value = true;
    }

    [ServerRpc]
    public void SpawnAllPiecesServerRpc()
    {
        if (chessboard.prefabs.Count == 0) return;
        for (int i = 0; i < chessboard.prefabs.Count; i++)
        {
            GameObject setobject = SpawnSinglePiece(chessboard.prefabs[i]);
            setobject.GetComponent<NetworkObject>().Spawn();
            //SpawnObjectClientRpc(setobject.GetComponent<NetworkObject>());

            spawnedObject.Add(setobject);

            Chesspiece cp = setobject.GetComponent<Chesspiece>();
            cp.SetupPieceClientRpc(teamNumber.Value);
            cp.AddActivePiece();
        }
    }

    public GameObject SpawnSinglePiece(GameObject reference)
    {
        GameObject _spawnedObject = Instantiate(reference);

        return _spawnedObject;
    }


    public void SetupBoard(Chessboard_Testing chess)
    {
        chessboard = chess;
    }



    #region Interact with board
    private void Interact()
    {


        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0) && !isMoving)
            ChangeSetServerRpc(true);
        else if (Input.GetMouseButtonUp(0))
            ChangeSetServerRpc(false);

        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            ChessPieceConnection connectionContact = info.transform.gameObject.GetComponent<ChessPieceConnection>();

            // Get the indexes of the tile i've hit
            Points hitPosition = connectionContact.CurrentTilePoint();//chessboard.LookupTileIndex(info.transform.gameObject);
            if (!connectionContact.isConnected)
                return;
            SetCurrentHoverServerRpc(currentHover.X, currentHover.Y, hitPosition.X, hitPosition.Y);
        }
        else
        {
            OutsideBoardServerRpc();
        }


        DraggingServerRpc(ray);
    }

    [ServerRpc]
    void DraggingServerRpc(Ray ray)
    {
        if (currentlyDragging)
        {
            Plane horizonPlane = new Plane(Vector3.up, Vector3.up * yoffset);
            float distance = 0.0f;
            if (horizonPlane.Raycast(ray, out distance))
                currentlyDragging.ReturnPositionServerRpc(ray.GetPoint(distance) + Vector3.up * dragOffset);
        }
    }
    [ServerRpc]
    void SetCurrentHoverServerRpc(int cX, int cY, int hX, int hY)
    {
        Points cHover = new Points(cX, cY);
        Points hHover = new Points(hX, hY);


        if (!pieceManager.IsCoordinateInList(currentHover))
        {
            currentHover = hHover;
            Debug.Log("New Hover Set: " + currentHover.X + ", " + currentHover.Y);
            pieceManager.SwapLayerServerRpc(currentHover.X, currentHover.Y, "Hover", OwnerClientId);

        }

        // If we were already hovering a tile, change the previous one
        if (!Points.DualEquals(currentHover, hHover))
        {
            if (ContainsVaildMove(ref availableMoves, currentHover) || ContainsVaildMove(ref availableSpecialMoves, currentHover))
                pieceManager.SwapLayerServerRpc(currentHover.X, currentHover.Y, "Highlight", OwnerClientId);
            else
                pieceManager.SwapLayerServerRpc(currentHover.X, currentHover.Y, "Tile", OwnerClientId);

            currentHover = hHover;
            Debug.Log("New Hover Changed: " + currentHover.X + ", " + currentHover.Y);
            pieceManager.SwapLayerServerRpc(hHover.X, hHover.Y, "Hover", OwnerClientId);

        }
            //if we press down on the mouse
            if (setCurrentDrag)//Input.GetMouseButtonDown(0))
            {
                if (currentlyDragging == null)
                    CurrentDrag(hHover.X, hHover.Y);
            }

        //if we are releasing the mouse
        if (currentlyDragging != null && !setCurrentDrag)//Input.GetMouseButtonUp(0))
        {
            ReleaseDrag(hHover.X, hHover.Y);
        }
    }

    [ServerRpc]
    void ChangeSetServerRpc(bool t)
    {
        setCurrentDrag = t;
    }

    [ServerRpc]
    void OutsideBoardServerRpc()
    {
        // that the current hover position and set the tag back to the original tag
        if (pieceManager.IsCoordinateInList(currentHover))
        {
             // whether that tag is highlight
            if (ContainsVaildMove(ref availableMoves, currentHover) || ContainsVaildMove(ref availableSpecialMoves, currentHover))
                pieceManager.SwapLayerServerRpc(currentHover.X, currentHover.Y, "Highlight", OwnerClientId);
            else
                pieceManager.SwapLayerServerRpc(currentHover.X, currentHover.Y, "Tile", OwnerClientId); // or the tag is Tile
            // identify current hover out the range of the board
            currentHover = new Points(1984987, 51684); 
        }

       // If we aren't carrying the object anymore or if we let go of the object we're holding
        if (currentlyDragging != null && !setCurrentDrag)
        {
            // the position the object was originally at
            Points priorPosition = new Points(currentlyDragging.currentX, currentlyDragging.currentY);
            // move the object back to that position
            currentlyDragging.ReturnPositionServerRpc(pieceManager.GetNewPiecePosition(priorPosition));
            currentlyDragging = null;
            RemoveHighlightTilesServerRpc();
        }
    }



    void ReleaseDrag(int x, int y)
    {
        Points newPosition = new Points(x, y);
        Points priorPosition = new Points(currentlyDragging.currentX, currentlyDragging.currentY);
        bool validMove = MoveTo(currentlyDragging, newPosition);
        if (!validMove)
            currentlyDragging.ReturnPositionServerRpc(pieceManager.GetNewPiecePosition(priorPosition));

        currentlyDragging = null;
        RemoveHighlightTilesServerRpc();
        isMoving = false;
    }

    //[ServerRpc(RequireOwnership =false)]
    void CurrentDrag(int x, int y)
    {
        Points newPosition = new Points(x, y);
        if (pieceManager.GetChesspieceGameObject(newPosition) != null)
        {
            Chesspiece conn = pieceManager.GetChesspieceConnection(newPosition).GetOccupiedPiece();
            // Is it our turn
            if (true && conn != null)
            {
                if (conn.OwnerClientId == OwnerClientId)
                    currentlyDragging = conn;
                if (currentlyDragging != null)
                {
                    // Get List of where I can go, highlight list as well
                    availableMoves = currentlyDragging.GetAvailableMoves();
                    if (currentlyDragging.GetSpecialMoves().Count > 0)
                        availableSpecialMoves = currentlyDragging.GetSpecialMoves();
                }
                HighlightTilesServerRpc();
            }
        }
    }

    private bool MoveTo(Chesspiece cp, Points hitPosition)
    {
        isMoving = true;
        Points previousPosition = new Points(cp.currentX, cp.currentY);

        if (ContainsVaildMove(ref availableSpecialMoves, hitPosition))
        {
            switch (cp.specialMove)
            {
                case SpecialMove.EnPassant:
                    if (!cp.gameObject.GetComponent<Pawn>())
                        break;
                    Pawn pp = cp.gameObject.GetComponent<Pawn>();
                    if (pp.pawns.ContainsKey(hitPosition))
                        Destroy(pp.pawns[hitPosition].gameObject);

                    pieceManager.GetChesspieceConnection(hitPosition).SetOccupiedPiece(cp);
                    pieceManager.GetChesspieceConnection(previousPosition).SetOccupiedPiece(null);
                    cp.hasMoved = true;
                    pieceManager.PositionSinglePiece(cp, hitPosition);


                    break;
                case SpecialMove.Castling:

                    Rook rook = null;
                    if (!cp.gameObject.GetComponent<King>())
                        break;

                    int a = 0;
                    King kp = cp.gameObject.GetComponent<King>();

                    //switch (teamNumber.Value)
                    //{
                    //    case 2:
                    if (hitPosition.X > previousPosition.X)
                    {
                        a = 1;
                        foreach (Rook rookie in kp.rooks)
                        {
                            if (rookie.currentX > cp.currentX && rookie.currentY == cp.currentY)
                                rook = rookie;
                        }
                        Points s = new Points(kp.currentX + a, kp.currentY);
                        Points r = new Points(rook.currentX, rook.currentY);
                        SetKingRook(ref cp, hitPosition, previousPosition, ref rook, s, r);
                    }
                    else if (hitPosition.X < previousPosition.X)
                    {
                        a = -1;
                        foreach (Rook rookie in kp.rooks)
                        {
                            if (rookie.currentX < cp.currentX && rookie.currentY == cp.currentY)
                                rook = rookie;
                        }
                        Points s = new Points(kp.currentX + a, kp.currentY);
                        Points r = new Points(rook.currentX, rook.currentY);
                        SetKingRook(ref cp, hitPosition, previousPosition, ref rook, s, r);
                    }
                    if (hitPosition.Y < previousPosition.Y)
                    {
                        a = -1;
                        foreach (Rook rookie in kp.rooks)
                        {
                            if (rookie.currentY < cp.currentY && rookie.currentX == cp.currentX)
                                rook = rookie;
                        }
                        Points s = new Points(kp.currentX, kp.currentY + a);
                        Points r = new Points(rook.currentX, rook.currentY);
                        SetKingRook(ref cp, hitPosition, previousPosition, ref rook, s, r);
                    }
                    else if (hitPosition.Y > previousPosition.Y)
                    {
                        a = 1;
                        foreach (Rook rookie in kp.rooks)
                        {
                            if (rookie.currentY > cp.currentY && rookie.currentX == cp.currentX)
                                rook = rookie;
                        }
                        Points s = new Points(kp.currentX, kp.currentY + a);
                        Points r = new Points(rook.currentX, rook.currentY);
                        SetKingRook(ref cp, hitPosition, previousPosition, ref rook, s, r);
                    }
                    break;

            }
            gameManager.SetPlayerTurnServerRpc();
            return true;
        }


        if (!ContainsVaildMove(ref availableMoves, hitPosition) && !ContainsVaildMove(ref availableSpecialMoves, hitPosition))
            return false;

        if (pieceManager.IsOccupied(hitPosition))

        {
            Chesspiece occupiedPiece = pieceManager.GetChesspieceConnection(hitPosition).occupiedChesspiece;

            // if the teams are the same return false
            if (occupiedPiece.team == cp.team)
                return false;

            // If it's the enemy
            //if (occupiedPiece.isKing)
            //CheckMate(teamNumber.Value); 
            // Temporary Delete
            // Remove From List

            Destroy(occupiedPiece.gameObject);
        }

        // Self Capture 
        // Might need an enum for types of piece


        pieceManager.GetChesspieceConnection(hitPosition).SetOccupiedPiece(cp);
        pieceManager.GetChesspieceConnection(previousPosition).SetOccupiedPiece(null);
        cp.hasMoved = true;
        pieceManager.PositionSinglePiece(cp, hitPosition);


        gameManager.SetPlayerTurnServerRpc();
        return true;
    }

    private void SetKingRook(ref Chesspiece cp, Points hitPosition, Points previousPosition, ref Rook rook, Points s, Points r)
    {
        pieceManager.GetChesspieceConnection(hitPosition).SetOccupiedPiece(cp);
        pieceManager.GetChesspieceConnection(previousPosition).SetOccupiedPiece(null);
        pieceManager.PositionSinglePiece(cp, hitPosition);

        pieceManager.GetChesspieceConnection(s).SetOccupiedPiece(rook);
        pieceManager.GetChesspieceConnection(r).SetOccupiedPiece(null);
        pieceManager.PositionSinglePiece(rook, s);
    }


    [ServerRpc]
    void HighlightTilesServerRpc()
    {
        for (int i = 0; i < availableMoves.Count; i++)
            pieceManager.SwapLayerServerRpc(availableMoves[i].X, availableMoves[i].Y, "Highlight", OwnerClientId);

        for (int i = 0; i < availableSpecialMoves.Count; i++)
            pieceManager.SwapLayerServerRpc(availableSpecialMoves[i].X, availableSpecialMoves[i].Y, "Highlight", OwnerClientId);
        //pieceManager.GetChesspieceGameObject(availableMoves[i]).layer = LayerMask.NameToLayer("Highlight");
    }

    [ServerRpc]
    void RemoveHighlightTilesServerRpc()
    {
        for (int i = 0; i < availableMoves.Count; i++)
            pieceManager.SwapLayerServerRpc(availableMoves[i].X, availableMoves[i].Y, "Tile", OwnerClientId);

        for (int i = 0; i < availableSpecialMoves.Count; i++)
            pieceManager.SwapLayerServerRpc(availableSpecialMoves[i].X, availableSpecialMoves[i].Y, "Tile", OwnerClientId);

        //pieceManager.GetChesspieceGameObject(availableMoves[i]).layer = LayerMask.NameToLayer("Tile");
        availableMoves.Clear();
        availableSpecialMoves.Clear();
    }


    private bool ContainsVaildMove(ref List<Points> moves, Points pointPos)
    {
        for (int i = 0; i < moves.Count; i++)
            if (Points.DualEquals(moves[i], pointPos))
                return true;
        return false;

    }

    #endregion

    protected override GameObject GenerateSingleTile(ref int x, ref int y)
    {
        int x_R = 0;
        int y_R = 0;
        GameObject tileObject = Instantiate(piece);
        //GameObject pieceSet = new GameObject(string.Format("PieceSpawnPoint"));
        //pieceSet.transform.parent = tileObject.transform;

        // Add to setup tiles to setup piece placement later
        setupTiles.Add(new Points(x, y), tileObject);
        if (armyEdgedPoints.Length > 0)
        {
            for (int i = 0; i < armyEdgedPoints.Length; i++)
            {
                if (setupTiles.ContainsKey(armyEdgedPoints[i]))
                {
                    Debug.Log("shooow");
                    armyEdgedobjects[i] = setupTiles[armyEdgedPoints[i]].GetComponent<ChessPieceConnection>();
                }
            }
        }


        //Mesh mesh = new Mesh();
        //tileObject.AddComponent<MeshFilter>().mesh = mesh;
        //tileObject.AddComponent<MeshRenderer>().material = chessboard.tileMaterial;

        //Vector3[] vertics = new Vector3[4];

        switch (teamNumber.Value)
        {
            case 1:
                //pieceSet.transform.position = new Vector3(x + .5f, 0 + .1f, y + mainBoardTileCountY + .5f);
                //vertics[0] = new Vector3((mainBoardOffsetX - x - 1) * tileSize, 0, (mainBoardOffsetY + (chessboard.tileCountY - 1) - y /*+ tileCountY*/) * tileSize);
                //vertics[1] = new Vector3((mainBoardOffsetX - x - 1) * tileSize, 0, ((mainBoardOffsetY + (chessboard.tileCountY - 1) - y /*+ tileCountY*/) + 1) * tileSize);
                //vertics[2] = new Vector3(((mainBoardOffsetX - x - 1) + 1) * tileSize, 0, (mainBoardOffsetY + (chessboard.tileCountY - 1) - y /*+ tileCountY*/) * tileSize);
                //vertics[3] = new Vector3(((mainBoardOffsetX - x - 1) + 1) * tileSize, 0, ((mainBoardOffsetY + (chessboard.tileCountY - 1) - y /*+ tileCountY*/) + 1) * tileSize);
                x_R = mainBoardOffsetX - x - 1;
                y_R = mainBoardOffsetY + (chessboard.tileCountY - 1) - y;
                break;
            case 2:
                //pieceSet.transform.position = new Vector3(x + .5f, 0 + .1f, y + mainBoardTileCountY + .5f);
                //vertics[0] = new Vector3((x) * tileSize, 0, (y - chessboard.tileCountY) * tileSize);
                //vertics[1] = new Vector3((x) * tileSize, 0, ((y - chessboard.tileCountY) + 1) * tileSize);
                //vertics[2] = new Vector3(((x) + 1) * tileSize, 0, (y - chessboard.tileCountY) * tileSize);
                //vertics[3] = new Vector3(((x) + 1) * tileSize, 0, ((y - chessboard.tileCountY) + 1) * tileSize);
                x_R = x;
                y_R = y - chessboard.tileCountY;
                break;
            case 3:
                //vertics[0] = new Vector3((y + mainBoardOffsetX) * tileSize, 0, x * tileSize);
                //vertics[1] = new Vector3((y + mainBoardOffsetX) * tileSize, 0, (x + 1) * tileSize);
                //vertics[2] = new Vector3(((y + mainBoardOffsetX) + 1) * tileSize, 0, x * tileSize);
                //vertics[3] = new Vector3(((y + mainBoardOffsetX) + 1) * tileSize, 0, (x + 1) * tileSize);
                x_R = y + mainBoardOffsetX;
                y_R = x;
                break;
            case 4:
                //vertics[0] = new Vector3((y - chessboard.tileCountY) * tileSize, 0, (mainBoardOffsetY - x - 1) * tileSize);
                //vertics[1] = new Vector3((y - chessboard.tileCountY) * tileSize, 0, ((mainBoardOffsetY - x - 1) + 1) * tileSize);
                //vertics[2] = new Vector3(((y - chessboard.tileCountY) + 1) * tileSize, 0, (mainBoardOffsetY - x - 1) * tileSize);
                //vertics[3] = new Vector3(((y - chessboard.tileCountY) + 1) * tileSize, 0, ((mainBoardOffsetY - x - 1) + 1) * tileSize);
                x_R = y - chessboard.tileCountY;
                y_R = mainBoardOffsetY - x - 1;
                break;
        }

        x = x_R;
        y = y_R;
        return tileObject;
    }
}
