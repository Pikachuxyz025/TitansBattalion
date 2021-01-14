using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class PlayerInfo : NetworkBehaviour
{
    [SyncVar]
    public int ArmyId;
    [SyncVar]
    public string username;

    [SyncVar]
    public int playerNum;

    [SyncVar]
    public GameObject gameManager,highlightman;

    [SyncVar]
    [SerializeField] private GameObject reso;
    public MirrorGameManager mirrorGameManager;
    public SID_BoardManager_Mirror SID_BM;


    public CameraController cam;
    public BoardLocation BoLo;
    public Vector3[] buildPos = new Vector3[4];

    private Ray rai;

    public List<GameObject> armyPieces = new List<GameObject>();

    [SyncVar]
    [SerializeField] private bool armyIsSet = false, boardset = false,onBoard=false;

    [SyncVar]
    public bool setToMatch = false, isWhite = false;

    [SyncVar]
    [SerializeField] private GameObject armyBoard;

    public SID_Chessman_Mirror highlightchessman;

    public GameObject[] set = new GameObject[2];

    public int show = 0;
    public override void OnStartAuthority()
    {
        Debug.Log("YOOOOOOOOOOOO!");
    }

    [ClientRpc]
    public void RpcSetInfo(int number, int army, string user, GameObject mana)
    {
        ArmyId = army;
        username = user;
        playerNum = number;
        gameManager = mana;
        reso = gameManager.GetComponent<MirrorGameManager>().varin;
        cam.isCameraMoblie = false;
        if (playerNum == 1)
            isWhite = true;
        else if (playerNum == 2)
            isWhite = false;
    }

    [ClientCallback]
    private void Update()
    {
        if (!hasAuthority) { return; }
        if (reso != null && show < 1)
        {
            CmdSet();
            CmdStart();
        }

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
                    //CmdStarknStrike();
                    if (onBoard)
                    {
                        SelectandMove();
                    }
                }

            }
        }
    }

    void UpdateSelectional()
    {
        if (playerNum == 1)
        {
            if (SID_BM.isWhiteTurn)
            {
                Debug.Log("Player One's Turn");
                //CmdCameraRay();
                UpdateSelection();
            }
            else
            {
                onBoard = false;
                highlightchessman = null;
            }
        }
        else if (playerNum == 2)
        {
            if (!SID_BM.isWhiteTurn)
            {
                Debug.Log("Player Two's Turn");
                //CmdCameraRay();
                UpdateSelection();
            }
            else
            {
                onBoard = false;
                highlightchessman = null;
            }
        }
    }

    public void UpdateSelection()
    {
        Debug.Log("Player " + playerNum + " is updating");

        RaycastHit hit;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, LayerMask.GetMask("Pieces")))
        {
            Debug.Log("there are picecs here for Player " + playerNum);
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
            CmdDeHiighlight();
            onBoard = false;
        }

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
    [Command]
    void CmdHighlight(GameObject select)
    {
        Debug.Log(select);
        highlightman = select;
        highlightchessman = highlightman.GetComponent<SID_Chessman_Mirror>();
    }

    [Command]
    void CmdDeHiighlight()
    {
        highlightman = null;
    }
    [Command]
    void CmdUpdateSelection()
    {
        UpdateSelection();
    }
    [Command]
    void CmdCameraRay()
    {
        RpcCameraRay();
    }
    [ClientRpc]
    void RpcCameraRay()
    {
        if (!Camera.main)
        {
            Debug.Log("Can't find camera");
            return;
        }
        rai = Camera.main.ScreenPointToRay(Input.mousePosition);
    }
    [ClientRpc]
    void RpcChessman() => highlightchessman = SID_BM.highlightedChessman;

    [Command]
    void CmdChessman() => highlightman = SID_BM.highlightedChessman.gameObject;//RpcChessman();

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
        if (SID_BM.allMoves[new Points(x, y)])
        {
            for (int i = 0; i < SID_BM.gridblocksarray.Length; i++)
            {
                if (x == SID_BM.gridblocksarray[i].GridX && y == SID_BM.gridblocksarray[i].GridY)
                {
                    SID_Chessman_Mirror c = SID_BM.gridblocksarray[i].chessM;
                    if (c != null && c.isWhite != SID_BM.isWhiteTurn)
                    {
                        SID_BM.activeChessman.Remove(c.gameObject);
                        Destroy(c.gameObject);
                    }
                }
            }
            SID_BM.selectedChessmanPlayer.transform.position = SID_BM.GetTileCenter(x, y, 2);
            //RpcMove(x, y);
            SID_BoardManager_Mirror.M_eventmoment.Invoke();

            //SID_BM.isWhiteTurn = !SID_BM.isWhiteTurn;
            CmdSwitch();
            //SID_BoardHighlight_Mirror.Instance.RpcHideHighlights();
            SID_BM.highlightOn = false;
            SID_BM.selectedChessmanPlayer = null;
        }
        else
        {
            for (int i = 0; i < SID_BM.gridblocksarray.Length; i++)
            {
                if (SID_BM.gridblocksarray[i] != null)
                {
                    if (x == SID_BM.gridblocksarray[i].GridX && y == SID_BM.gridblocksarray[i].GridY)
                    {
                        if (SID_BM.gridblocksarray[i].pieceOn && SID_BM.gridblocksarray[i].chessM.isWhite == SID_BM.isWhiteTurn)
                        {
                            //SID_BoardHighlight_Mirror.Instance.RpcHideHighlights();
                            SID_BM.highlightOn = false;
                            SID_BM.selectedChessmanPlayer = SID_BM.gridblocksarray[i].chessM;
                            SID_BM.allowedMoves = SID_BM.selectedChessmanPlayer.confirmation;
                            SID_BM.highlightOn = true;
                            SID_BoardHighlight_Mirror.Instance.HighLightAllowedMoves(SID_BM.allowedMoves);
                        }
                    }
                    else
                    {
                        Debug.Log("move not available");
                        //SID_BoardHighlight_Mirror.Instance.RpcHideHighlights();
                        SID_BM.highlightOn = false;
                        SID_BM.selectedChessmanPlayer = null;
                    }
                }
            }
        }
    }

    [Command]
    void CmdSwitch()
    {
        SID_BM.isWhiteTurn = !SID_BM.isWhiteTurn;
    }
    [Command]
    void CmdClearMoves()
    {
        SID_BM.ClearMoves();
    }

    void SelectChessman()
    {
        Debug.Log("DEATH");
        SID_BM.selectedChessmanPlayer = highlightchessman;
        SID_BM.selectedChesspiece = SID_BM.selectedChessmanPlayer.gameObject;

        SID_BM.allowedMoves = SID_BM.selectedChessmanPlayer.confirmation;
        SID_BM.highlightOn = true;
        SID_BoardHighlight_Mirror.Instance.HighLightAllowedMoves(SID_BM.allowedMoves);
    }

    [ClientRpc]
    public void RpcSelectChessman()
    {
        if (highlightchessman.isWhite != SID_BM.isWhiteTurn || !highlightchessman.hasAuthority)
        {
            Debug.Log("not your men to choose");
            return;
        }
        //RpcSetnSelect();

    }
    [Command]
    public void CSelectChessman(int x, int y)
    {
        //RpcSelectChessman();
        SID_BM.selectedChessmanPlayer = highlightchessman;
        SID_BM.selectedChesspiece = SID_BM.selectedChessmanPlayer.gameObject;

        SID_BM.allowedMoves = SID_BM.selectedChessmanPlayer.confirmation;
        SID_BM.highlightOn = true;
        SID_BoardHighlight_Mirror.Instance.HighLightAllowedMoves(SID_BM.allowedMoves);
    }


    [Command]
    public void CmdStarknStrike()
    {
        SID_BM.RpcDetermineAuthority();
    }
    [ClientRpc]
    void RpcSelection()
    {
        SID_BM.SelectandMove();
    }
    [Command]
    public void CmdSelectnShow()
    {
        //RpcSelection();
        SID_BM.SelectandMove();
    }

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
        {
            SID_BM.PlayerOneIntSetMatch(this.connectionToClient);
        }
        else if (playerNum == 2)
        {
            SID_BM.PlayerTwoIntSetMatch(this.connectionToClient);
        }
        boardset = true;
    }

    [ClientRpc]
    public void RpcBuildArmy(bool source)
    {
        if (hasAuthority)
        {
            if (SID_BM.buildModeOn && armyBoard != null)
            {
                if (playerNum == 1)
                    SID_BM.FixedPositionOne(armyBoard, buildPos[0], buildPos[1], Vector3.left, Vector3.right, source);
                else if (playerNum == 2)
                    SID_BM.FixedPositionTwo(armyBoard, buildPos[3], buildPos[2], Vector3.left, Vector3.right, !source);
            }
        }
    }

    [Command]
    public void CmdStart()
    {
        RpcSetupBoard(reso);
    }

    [Command]
    public void CmdBoard(bool source)
    {
        RpcBuildArmy(source);
    }

    [ClientRpc]
    public void RpcAssertPlayerOne()
    {
        SID_BM.p1chessmanPrefabs = armyPieces;
        SID_BM.playerOneArmy = ArmyId;
    }

    [ClientRpc]
    public void RpcAssertPlayerTwo()
    {
        SID_BM.p2chessmanPrefabs = armyPieces;
        SID_BM.playerTwoArmy = ArmyId;
    }

    [ClientRpc]
    void RpcTest()
    {
        mirrorGameManager = gameManager.GetComponent<MirrorGameManager>();
        SID_BM = gameManager.GetComponent<SID_BoardManager_Mirror>();
    }

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



    [ClientRpc]
    public void RpcSetupBoard(GameObject targetGameObject)
    {
        BoLo = targetGameObject.GetComponent<BoardLocation>();

        buildPos[0] = BoLo.playerOnePointA.transform.position;
        buildPos[1] = BoLo.playerOnePointB.transform.position;
        buildPos[2] = BoLo.playerTwoPointA.transform.position;
        buildPos[3] = BoLo.playerTwoPointB.transform.position;

        show++;
    }

    [Command]
    public void CmdSelectArmy(int armyId, int playerid)
    {
        if (playerid == 1)
        {
            if (!armyIsSet)
            {
                GameObject yo = Instantiate(MirrorGameManager.curArmy[armyId - 1].armyGrid, buildPos[0], mirrorGameManager.neutralCoordination) as GameObject;
                NetworkServer.Spawn(yo, connectionToClient);
                RpcSetArmyBoard(yo);
                SID_BoardGridSet[] children = yo.GetComponentsInChildren<SID_BoardGridSet>();
                foreach (SID_BoardGridSet item in children)
                {
                    if (item.startingPieceone)
                    {
                        SID_BM.originBoardPiece[0] = item.gameObject;
                        armyIsSet = true;
                        //Debug.Log(item.transform.position);
                    }
                }
            }
        }
        else if (playerid == 2)
        {
            if (!armyIsSet)
            {
                GameObject yo = Instantiate(MirrorGameManager.curArmy[armyId - 1].armyGrid, buildPos[2] + new Vector3(0, 0, MirrorGameManager.curArmy[armyId - 1].armyOffset), mirrorGameManager.neutralCoordination) as GameObject;
                NetworkServer.Spawn(yo, connectionToClient);
                RpcSetArmyBoard(yo);
                SID_BoardGridSet[] children = yo.GetComponentsInChildren<SID_BoardGridSet>();
                foreach (SID_BoardGridSet item in children)
                {
                    if (item.startingPiecetwo)
                    {
                        SID_BM.originBoardPiece[1] = item.gameObject;
                        armyIsSet = true;
                        //Debug.Log(item.transform.position);
                    }
                }
            }
        }
        else
            Debug.Log("Failure");
    }


    [Command]
    public void CmdOpinoin()
    {
        if (playerNum == 1)
            SID_BM.PlayerOneIntSetMatch(this.connectionToClient);
        else if (playerNum == 2)
            SID_BM.PlayerTwoIntSetMatch(this.connectionToClient);

        show++;
    }

    public void SetObjectPiece(int i, GameObject tho)
    {

    }

    [ClientRpc]
    public void RpcSetArmyBoard(GameObject obj)
    {
        if (hasAuthority)
            armyBoard = obj;
    }
}
