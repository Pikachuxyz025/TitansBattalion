using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerInfo : NetworkBehaviour
{
    public static PlayerInfo localplayer;
    public SID_King_Mirror myKing;

    [SyncVar] public int ArmyId;
    [SyncVar] public string username;

    [SyncVar]
    public int playerNum;

    public UIGame UiSystem;
    public RematchState RematchSet = RematchState.None;

    [SyncVar]
    public GameObject gameManager, highlightman;

    [SyncVar]
    [SerializeField] private GameObject reso;
    public MirrorGameManager mirrorGameManager;
    public SID_BoardManager_Mirror SID_BM;

    public CameraController cam;
    public BoardLocation BoLo;
    public Vector3[] buildPos = new Vector3[4];

    public List<GameObject> armyPieces = new List<GameObject>();

    [SyncVar]
    [SerializeField] private bool armyIsSet = false, boardset = false, onBoard = false;

    [SyncVar]
    public bool setToMatch = false, isWhite = false;

    [SyncVar]
    [SerializeField] private GameObject armyBoard;

    public SID_Chessman_Mirror highlightchessman;

    [SerializeField] SID_BoardPieceManager PieceManager;
    public int show = 0;

    /*public override void OnStartAuthority()
    {
        Debug.Log("YOOOOOOOOOOOO!");
    }*/

    #region Normal

    public void Setsking(SID_King_Mirror kingme) => myKing = kingme;

    public void SetInfo(int number, int army, string user, GameObject mana, GameObject vari)
    {
        ArmyId = army;
        username = user;
        playerNum = number;
        gameManager = mana;
        reso = vari;
        cam.isCameraMoblie = false;
        if (playerNum == 1)
            isWhite = true;
        else if (playerNum == 2)
            isWhite = false;
        Debug.Log("everything is set");
    }

    public void DeActiveButton(Button button) => button.interactable = false;

    private void Update()
    {
        if (!hasAuthority) { return; }
        Debug.Log("I'm player " + playerNum);
        if (reso != null && show < 1)
        {
            localplayer = this;
            UiSystem.gameObject.SetActive(true);
            CmdSet();
            CmdStart();
        }
        ChangeAuthority();
        if (Input.GetKeyDown(KeyCode.E) && BoLo != null && !boardset)
        {
            if (armyBoard == null)
                CmdSelectArmy(ArmyId, playerNum);
            else
                CmdSetupBoard();
        }

        if (!setToMatch)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                CmdBoard(true);
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                CmdBoard(false);
        }
        CmdCameraOn();

        if (SID_BM != null)
        {
            if (SID_BM.setActive)
            {
                UpdateSelectional();
                if (Input.GetMouseButtonDown(0))
                {
                    if (onBoard)
                    {
                        SelectandMove();
                    }
                }
            }
        }
    }

    public void SetupBoard(GameObject targetGameObject)
    {
        BoLo = targetGameObject.GetComponent<BoardLocation>();

        buildPos[0] = BoLo.playerOnePoints[0].transform.position;
        buildPos[1] = BoLo.playerOnePoints[1].transform.position;
        buildPos[2] = BoLo.playerTwoPoints[0].transform.position;
        buildPos[3] = BoLo.playerTwoPoints[1].transform.position;
    }

    public void UpdateSelectional()
    {
        if (playerNum == 1)
        {
            if (SID_BM.isWhiteTurn)
                UpdateSelection();
            else
            {
                onBoard = false;
                highlightchessman = null;
            }
        }
        else if (playerNum == 2)
        {
            if (!SID_BM.isWhiteTurn)
                UpdateSelection();
            else
            {
                onBoard = false;
                highlightchessman = null;
            }
        }
    }

    public void UpdateSelection()
    {
        RaycastHit hit;

        if (isWhite != SID_BM.isWhiteTurn)
            highlightchessman = null;
        else if (highlightchessman == null)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, LayerMask.GetMask("Pieces")))
            {
                SID_Chessman_Mirror sid = hit.collider.GetComponent<SID_Chessman_Mirror>();
                SID_BM.selectionX = sid.CurrentX;
                SID_BM.selectionY = sid.CurrentY;
                SID_BM.highlightedChessman = sid;
                highlightchessman = sid;
                CmdHighlight(highlightchessman.gameObject);
                onBoard = true;
            }
            else
            {
                highlightchessman = null;
                CmdDeHighlight();
                onBoard = false;
            }
        }
        else if (highlightchessman != null)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, LayerMask.GetMask("ChessPlane")))
            {
                SID_BoardGridSet sid = hit.collider.GetComponent<SID_BoardGridSet>();
                SID_BM.boardselectionX = sid.GridX;
                SID_BM.boardselectionY = sid.GridY;
                onBoard = true;
            }
            else
            {
                onBoard = false;
            }
        }
    }

    public void SelectandMove()
    {
        if (SID_BM.selectedChessmanPlayer == null)
        {
            //select the chessman
            if (SID_BM.highlightedChessman != null)
            {
                CmdClearMoves();
                SID_BM.ClearMoves();
                if (highlightchessman.isWhite != SID_BM.isWhiteTurn || !highlightchessman.hasAuthority)
                {
                    Debug.Log("not your men to choose");
                    highlightchessman = null;
                    return;
                }
                SelectChessman();
            }
        }
        else
        {
            //move the chessman
            MoveChessman(SID_BM.boardselectionX, SID_BM.boardselectionY);
        }
    }

    public void MoveChessman(int x, int y)
    {
        PieceState pieceStateDestination = PieceManager.VaildatePieces(x, y, SID_BM.selectedChessmanPlayer);
        if (SID_BM.allMoves[new Points(x, y)])
        {
            SID_BM.selectedChessmanPlayer.transform.position = SID_BM.GetTileCenter(x, y, 2);
            if (pieceStateDestination == PieceState.Enemy)
            {
                if (PieceManager.FindChessman(x, y).GetType().ToString() == "SID_King_Mirror")
                    CmdWeHaveAWinner(this, GameState.Check);
                CmdSeekandDestroy(PieceManager.FindGridPiece(new Points(x, y)).chesspiece);
            }
            CmdSwitch();

            CmdReset();

            SID_BM.highlightOn = false;
            SID_BM.selectedChessmanPlayer = null;
        }
        else
        {
            if (pieceStateDestination == PieceState.Friendly)
            {
                SID_BoardHighlight_Mirror.Instance.HideHighlights();
                SID_BM.selectedChessmanPlayer = PieceManager.FindGridPiece(new Points(x, y)).chessM;
                SID_BM.allowedMoves = SID_BM.selectedChessmanPlayer.confirmedMoves;
                SID_BoardHighlight_Mirror.Instance.HighLightAllowedMoves(SID_BM.allowedMoves);
            }
            else if (pieceStateDestination == PieceState.Free)
            {
                Debug.Log("move not available");
                SID_BM.highlightOn = false;
                SID_BM.selectedChessmanPlayer = null;
            }
        }
    }

    public void SelectChessman()
    {
        // highlighted chessman becomes Selected Chessman Player 
        SID_BM.selectedChessmanPlayer = highlightchessman;
        SID_BM.selectedChesspiece = SID_BM.selectedChessmanPlayer.gameObject;

        // options selected chessman have to offer
        SID_BM.allowedMoves = SID_BM.selectedChessmanPlayer.confirmedMoves;
        SID_BM.highlightOn = true;
        SID_BoardHighlight_Mirror.Instance.HighLightAllowedMoves(SID_BM.allowedMoves);
    }

    public void ChangeAuthority()
    {
        if (gameManager != null && SID_BM != null)
        {
            if (SID_BM.isWhiteTurn == isWhite && !gameManager.GetComponent<NetworkIdentity>().hasAuthority)
            {
                CmdChangeAuthority();
            }
        }
    }

    #endregion

    #region Command

    [Command]
    public void CmdWeHaveAWinner(PlayerInfo winningPlayer, GameState victoryHow)
    {
        mirrorGameManager.currentState = victoryHow;
        RpcWeHaveAWinner(winningPlayer, victoryHow);
    }


    [Command]
    public void CmdChangeAuthority()
    {
        gameManager.GetComponent<NetworkIdentity>().RemoveClientAuthority();
        gameManager.GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        RpcSelection();
    }

    [Command]
    public void CmdHighlight(GameObject select)
    {
        highlightman = select;
        highlightchessman = highlightman.GetComponent<SID_Chessman_Mirror>();
    }

    [Command]
    public void CmdDeHighlight()
    {
        highlightman = null;
    }

    [Command]
    void CmdSeekandDestroy(GameObject g)
    {
        RpcSeekandDestroy(g);
        NetworkServer.Destroy(g);
    }

    [Command]
    void CmdChessman() => highlightman = SID_BM.highlightedChessman.gameObject;

    [Command]
    public void CmdCameraOn()
    {
        if (mirrorGameManager != null)
        {
            if (mirrorGameManager.armyIsSet)
                cam.isCameraMoblie = true;
        }
    }

    [Command]
    public void CmdSetupBoard()
    {
        setToMatch = true;
        if (playerNum == 1)
            SID_BM.PlayerOneIntSetMatch(this.connectionToClient, this);
        else if (playerNum == 2)
            SID_BM.PlayerTwoIntSetMatch(this.connectionToClient, this);
        boardset = true;
    }

    [Command]
    void CmdSwitch()
    {
        if (playerNum == 2)
            mirrorGameManager.turnCount++;
        SID_BM.isWhiteTurn = !SID_BM.isWhiteTurn;
    }

    [Command]
    void CmdClearMoves()
    {
        SID_BM.ClearMoves();
    }

    [Command]
    public void CmdStart()
    {
        SetupBoard(reso);
        RpcSetupBoard(reso);
    }

    [Command]
    public void CmdBoard(bool source) => RpcBuildArmy(source);


    [Command]
    public void CmdSet()
    {
        if (playerNum == 1)
        {
            mirrorGameManager = gameManager.GetComponent<MirrorGameManager>();
            SID_BM = gameManager.GetComponent<SID_BoardManager_Mirror>();
            RpcTest();
            mirrorGameManager.playerInfos[playerNum - 1] = this;
            armyPieces = MirrorGameManager.curArmy[ArmyId - 1].piecesPrefabs;
            SID_BM.p1chessmanPrefabs = armyPieces;
            SID_BM.playerOneArmy = ArmyId;
        }
        else if (playerNum == 2)
        {
            mirrorGameManager = gameManager.GetComponent<MirrorGameManager>();
            SID_BM = gameManager.GetComponent<SID_BoardManager_Mirror>();
            RpcTest();
            mirrorGameManager.playerInfos[playerNum - 1] = this;
            armyPieces = MirrorGameManager.curArmy[ArmyId - 1].piecesPrefabs;
            SID_BM.p2chessmanPrefabs = armyPieces;
            SID_BM.playerTwoArmy = ArmyId;
        }
    }

    [Command]
    public void CmdSelectArmy(int armyId, int playerid)
    {
        if (playerid == 1)
        {
            if (!armyIsSet)
            {
                GameObject yo = Instantiate(mirrorGameManager.army.allArmies[armyId - 1].armyGrid, buildPos[0], Quaternion.Euler(0, 0, 0)) as GameObject;
                yo.GetComponent<NetworkMatchChecker>().matchId = GetComponent<NetworkMatchChecker>().matchId;
                NetworkServer.Spawn(yo, connectionToClient);
                armyBoard = yo;
                SID_BoardGridSet[] children = yo.GetComponentsInChildren<SID_BoardGridSet>();
                foreach (SID_BoardGridSet bgs in children)
                {
                    if (bgs.startingPieceOrigin == BoardStartPoint.StartingPiecePlayerOne)
                        SID_BM.originBoardPiece[0] = bgs.gameObject;
                }
                armyIsSet = true;
            }
        }
        else if (playerid == 2)
        {
            if (!armyIsSet)
            {
                GameObject yo = Instantiate(mirrorGameManager.army.allArmies[armyId - 1].armyGrid, buildPos[3] + new Vector3(0, 0, mirrorGameManager.army.allArmies[armyId - 1].armyOffset), Quaternion.Euler(0, 0, 0)) as GameObject;
                yo.GetComponent<NetworkMatchChecker>().matchId = GetComponent<NetworkMatchChecker>().matchId;
                NetworkServer.Spawn(yo, connectionToClient);
                armyBoard = yo;
                SID_BoardGridSet[] children = yo.GetComponentsInChildren<SID_BoardGridSet>();
                foreach (SID_BoardGridSet bgs in children)
                {
                    if (bgs.startingPieceOrigin == BoardStartPoint.StartingPiecePlayerTwo)
                        SID_BM.originBoardPiece[1] = bgs.gameObject;
                }
                armyIsSet = true;
            }
        }
        else
            Debug.Log("Failure");
    }

    [Command]
    public void CmdReset() => RpcReset();

    [Command]
    public void CmdRematchZone(int rematching)
    {
        RematchState rematchers = new RematchState();
        switch (rematching)
        {
            case 0:
                rematchers = RematchState.Rematch;
                break;
            case 1:
                rematchers = RematchState.NoRematch;
                break;
        }
        RematchSet = rematchers;
        if (!mirrorGameManager.continueorno.ContainsKey(this))
            mirrorGameManager.continueorno.Add(this, RematchSet);
        mirrorGameManager.Checking();
    }

    #endregion

    #region ClientRpc

    [ClientRpc]
    public void RpcWeHaveAWinner(PlayerInfo winningPlayer, GameState victoryHow)
    {
        //set gamestate to new state
        mirrorGameManager.currentState = victoryHow;
        //player num has won the game
        UiSystem.victoryText.text = winningPlayer.gameObject.name + " is the winner";
    }

    [ClientRpc]
    public void RpcSetArmyBoard(GameObject obj)
    {
        if (hasAuthority)
            armyBoard = obj;
    }

    [ClientRpc]
    void RpcSeekandDestroy(GameObject g) => SID_BM.activeChessman.Remove(g);

    [ClientRpc]
    void RpcSelection() => gameManager.GetComponent<SID_BoardManager_Mirror>().currentPlayerTurn = this;

    [ClientRpc]
    public void RpcBuildArmy(bool source)
    {
        if (hasAuthority)
        {
            if (SID_BM.buildModeOn && armyBoard != null)
            {
                if (playerNum == 1)
                    SID_BM.FixedPositionOne(armyBoard.GetComponent<BoardLocation>(), buildPos[0], buildPos[1], Vector3.left, Vector3.right, source);
                else if (playerNum == 2)
                    SID_BM.FixedPositionTwo(armyBoard.GetComponent<BoardLocation>(), buildPos[2], buildPos[3], Vector3.left, Vector3.right, source);
            }
        }
    }

    [ClientRpc]
    void RpcTest()
    {
        mirrorGameManager = gameManager.GetComponent<MirrorGameManager>();
        SID_BM = gameManager.GetComponent<SID_BoardManager_Mirror>();
        PieceManager = SID_BoardPieceManager.instance;
        this.gameObject.name = "Player" + " " + playerNum;
    }

    [ClientRpc]
    public void RpcSetupBoard(GameObject targetGameObject)
    {
        BoLo = targetGameObject.GetComponent<BoardLocation>();

        buildPos[0] = BoLo.playerOnePoints[0].transform.position;
        buildPos[1] = BoLo.playerOnePoints[1].transform.position;
        buildPos[2] = BoLo.playerTwoPoints[0].transform.position;
        buildPos[3] = BoLo.playerTwoPoints[1].transform.position;
        show++;
    }

    [ClientRpc]
    public void RpcReset() => SID_BoardManager_Mirror.M_eventmoment.Invoke();

    #endregion
}
