using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Mirror;
using UnityEngine.Events;

public class SID_BoardManager_Mirror : NetworkBehaviour
{
    public static SID_BoardManager_Mirror Instance;

    [SyncVar]
    public bool buildModeOn = false, setActive = false, isWhiteTurn = true;

    public static UnityEvent M_eventmoment;
    private ArmyManager aSys;

    private const float TileSize = 1.0f, TileOffset = 0.5f;
    [HideInInspector]
    public int selectionX = -1, selectionY = -1, controlarmy, boardselectionX, boardselectionY;

    public SID_Chessman_Mirror selectedChessmanPlayer, highlightedChessman;

    public List<GameObject> p1chessmanPrefabs, p2chessmanPrefabs, activeChessman;

    public List<GameObject> setpos = new List<GameObject>();

    [SyncVar]
    public int playerOneArmy, playerTwoArmy;

    [SyncVar]
    public bool highlightOn = false;

    [SyncVar]
    public GameObject selectedChesspiece;

    public bool onBoard;

    public System.Guid matching;

    public PlayerInfo currentPlayerTurn;

    public GameObject[] originBoardPiece = new GameObject[3];
    private Quaternion neutralCoordination = Quaternion.Euler(0, 0, 0), coordination = Quaternion.Euler(0, 180, 0);
    [HideInInspector] public Dictionary<Points, bool> allMoves = new Dictionary<Points, bool>(new Points.EqualityComparer());
    [HideInInspector] public Dictionary<Points, bool> allowedMoves = new Dictionary<Points, bool>(new Points.EqualityComparer());
    [SerializeField] SID_BoardPieceManager PieceManager;


    private void Start()
    {
        Instance = this;
        if (M_eventmoment == null)
            M_eventmoment = new UnityEvent();
        aSys = GetComponent<ArmyManager>();
        PieceManager = SID_BoardPieceManager.instance;
        matching = GetComponent<NetworkMatchChecker>().matchId;
        //buildPos.Callback += UpdatedSetUpBoard;
    }

    private void Update()
    {
        if (setActive)
        {
            //UpdateSelection();
            AccountAllMoves();
            UpdateAllMoves();
            originBoardPiece[2] = PieceManager.orginPiece;
            if (isServer)
                RpcHightlight();
        }
        //BuildArmy();
    }

    [ClientRpc]
    public void RpcHightlight()
    {
        if (!highlightOn)
            SID_BoardHighlight_Mirror.Instance.HideHighlights();
    }

    //[ClientRpc]
    /*public void SelectandMove()
    {
        Debug.Log("Working: " + connectionToClient);
        if (selectedChessmanPlayer == null)
        {
            //select the chessman
            if (highlightedChessman != null)
            {
                Debug.Log("Come on");
                ClearMoves();
                SelectChessman(selectionX, selectionY);
            }
        }
        else
        {
            //move the chessman
            MoveChessman(boardselectionX, boardselectionY);
        }
    }*/

    public void SelectandMove()
    {
        if (selectedChessmanPlayer == null)
        {
            //select the chessman
            if (highlightedChessman != null)
            {
                ClearMoves();
                if (currentPlayerTurn.highlightchessman.isWhite != isWhiteTurn || !currentPlayerTurn.highlightchessman.hasAuthority)
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
            MoveChessman(boardselectionX, boardselectionY);
        }
    }

    private void UpdateAllMoves()
    {
        if (allowedMoves.Count != 0)
        {
            foreach (Points pointone in allMoves.Keys.ToList<Points>())
            {
                foreach (Points pointtwo in allowedMoves.Keys.ToList<Points>())
                {
                    if (SameCoord(pointone, pointtwo))
                    {
                        if (allMoves[pointtwo] != allowedMoves[pointtwo])
                            allMoves[pointtwo] = allowedMoves[pointtwo];
                    }
                }
            }
        }
    }

    public void ClearMoves()
    {
        foreach (Points point in allMoves.Keys.ToList<Points>())
        {
            allMoves[point] = false;
        }
        highlightOn = false;
        //SID_BoardHighlight_Mirror.Instance.RpcHideHighlights();
    }
    private void AccountAllMoves()
    {
        for (int i = 0; i < PieceManager.gridblocksarray.Length; i++)
        {
            if (PieceManager.gridblocksarray[i].connected)
            {
                if (!allMoves.ContainsKey(PieceManager.coordinates[PieceManager.gridblocksarray[i]]))
                    allMoves.Add(PieceManager.coordinates[PieceManager.gridblocksarray[i]], false);
            }
        }
    }

    /*public void MoveChessman(int x, int y)
    {
        if (allMoves[new Points(x, y)])
        {
            for (int i = 0; i < PieceManager.gridblocksarray.Length; i++)
            {
                if (x == PieceManager.gridblocksarray[i].GridX && y == PieceManager.gridblocksarray[i].GridY)
                {
                    SID_Chessman_Mirror c = PieceManager.gridblocksarray[i].chessM;
                    if (c != null && c.isWhite != isWhiteTurn)
                    {
                        activeChessman.Remove(c.gameObject);
                        Destroy(c.gameObject);
                    }
                }
            }
            selectedChessmanPlayer.transform.position = GetTileCenter(x, y, 2);
            M_eventmoment.Invoke();

            isWhiteTurn = !isWhiteTurn;
            highlightOn = false;
            selectedChessmanPlayer = null;
        }
        else
        {
            for (int i = 0; i < PieceManager.gridblocksarray.Length; i++)
            {
                if (PieceManager.gridblocksarray[i] != null)
                {
                    if (x == PieceManager.gridblocksarray[i].GridX && y == PieceManager.gridblocksarray[i].GridY)
                    {
                        if (PieceManager.gridblocksarray[i].pieceOn && PieceManager.gridblocksarray[i].chessM.isWhite == isWhiteTurn)
                        {
                            highlightOn = false;
                            selectedChessmanPlayer = PieceManager.gridblocksarray[i].chessM;
                            allowedMoves = selectedChessmanPlayer.confirmation;
                            highlightOn = true;
                            SID_BoardHighlight_Mirror.Instance.HighLightAllowedMoves(allowedMoves);
                        }
                    }
                    else
                    {
                        Debug.Log("move not available");
                        highlightOn = false;
                        selectedChessmanPlayer = null;
                    }
                }
            }
        }
    }*/

    public void MoveChessman(int x, int y)
    {
        PieceState pieceStateDestination = PieceManager.VaildatePieces(x, y, selectedChessmanPlayer);
        if (allMoves[new Points(x, y)])
        {
            if (pieceStateDestination == PieceState.Enemy)
                SeekandDestroy(PieceManager.FindGridPiece(new Points(x, y)).chesspiece);

            switch (selectedChessmanPlayer.GetType().ToString())
            {
                case "SID_Pawn_Mirror":
                    Debug.Log("is pawn");
                    SID_Pawn_Mirror pawn = selectedChesspiece.GetComponent<SID_Pawn_Mirror>();
                    if (y == pawn.CurrentY + 2 || y == pawn.CurrentY - 2)
                        pawn.duoMovement = true;
                    break;
                case "SID_Rook_Mirror":
                    Debug.Log("is rook");
                    SID_Rook_Mirror rook = selectedChesspiece.GetComponent<SID_Rook_Mirror>();
                    rook.hasMoved = true;
                    break;
                case "SID_King_Mirror":
                    Debug.Log("is king");
                    SID_King_Mirror king = selectedChesspiece.GetComponent<SID_King_Mirror>();

                    king.Castling(x, y);
                    king.hasMoved = true;
                    break;
            }

            selectedChessmanPlayer.transform.position = GetTileCenter(x, y, 2);

            isWhiteTurn = !isWhiteTurn;

            M_eventmoment.Invoke();

            highlightOn = false;
            selectedChessmanPlayer = null;
        }
        else
        {

            if (pieceStateDestination == PieceState.Friendly)
            {
                SID_BoardHighlight_Mirror.Instance.HideHighlights();
                selectedChessmanPlayer = PieceManager.FindGridPiece(new Points(x, y)).chessM;
                allowedMoves = selectedChessmanPlayer.confirmedMoves;
                SID_BoardHighlight_Mirror.Instance.HighLightAllowedMoves(allowedMoves);
            }
            else if (pieceStateDestination == PieceState.Free)
            {
                Debug.Log("move not available");
                highlightOn = false;
                selectedChessmanPlayer = null;
            }
        }
    }

    void SeekandDestroy(GameObject g)
    {
        activeChessman.Remove(g);
        NetworkServer.Destroy(g);
    }

    [ClientRpc]
    private void RpcMove(int x, int y)
    {
        selectedChesspiece.transform.position = GetTileCenter(x, y, 2);
    }

    [ClientRpc]
    public void RpcDetermineAuthority()
    {
        Debug.Log("is this working at all?");
        if (highlightedChessman != null)
        {
            //if (isClient)
            //{
            if (highlightedChessman.hasAuthority)
                Debug.Log("joy, this is yours");
            else
                Debug.Log("this is either not yours or this isn't working");
            // }
        }
    }

    /*public void SelectChessman(int x, int y)
    {
        Debug.Log("selection");
        if (highlightedChessman.isWhite != isWhiteTurn || !hasAuthority)
        {
            Debug.Log("not your men to choose");
            return;
        }
        //RpcSetnSelect();
        selectedChessmanPlayer = highlightedChessman;
        selectedChesspiece = selectedChessmanPlayer.gameObject;

        allowedMoves = selectedChessmanPlayer.confirmation;
        highlightOn = true;
        SID_BoardHighlight_Mirror.Instance.HighLightAllowedMoves(allowedMoves);
    }*/

    public void SelectChessman()
    {
        // highlighted chessman becomes Selected Chessman Player 
        selectedChessmanPlayer = currentPlayerTurn.highlightchessman;
        selectedChesspiece = selectedChessmanPlayer.gameObject;

        // options selected chessman have to offer
        allowedMoves = selectedChessmanPlayer.confirmedMoves;
        highlightOn = true;
        SID_BoardHighlight_Mirror.Instance.HighLightAllowedMoves(allowedMoves);
        //CmdHighlighting();
    }

    [ClientRpc]
    void RpcSetnSelect()
    {
        selectedChessmanPlayer = highlightedChessman;
    }

    [ClientRpc]
    public void Rpcshow()
    {
        SID_BoardHighlight_Mirror.Instance.HighLightAllowedMoves(allowedMoves);
    }


    /*public void UpdateSelection()
    {
        if (!Camera.main)
            return;

        RaycastHit hit;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, LayerMask.GetMask("Pieces")))
        {
            SID_Chessman_Mirror sid = hit.collider.GetComponent<SID_Chessman_Mirror>();
            selectionX = sid.CurrentX;
            selectionY = sid.CurrentY;
            highlightedChessman = sid;
            onBoard = true;
        }
        else
        {
            highlightedChessman = null;
            onBoard = false;
        }

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, LayerMask.GetMask("ChessPlane")))
        {
            SID_BoardGridSet sid = hit.collider.GetComponent<SID_BoardGridSet>();
            boardselectionX = sid.GridX;
            boardselectionY = sid.GridY;
            onBoard = true;
        }
        else
        {
            onBoard = false;
        }
    }*/

    public void UpdateSelection()
    {
        RaycastHit hit;

        if (currentPlayerTurn.isWhite != isWhiteTurn)
            currentPlayerTurn.highlightchessman = null;
        else
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, LayerMask.GetMask("Pieces")))
            {
                SID_Chessman_Mirror sid = hit.collider.GetComponent<SID_Chessman_Mirror>();
                selectionX = sid.CurrentX;
                selectionY = sid.CurrentY;
                highlightedChessman = sid;
                currentPlayerTurn.highlightchessman = sid;
                currentPlayerTurn.CmdHighlight(currentPlayerTurn.highlightchessman.gameObject);
                onBoard = true;
            }
            else
            {
                currentPlayerTurn.highlightchessman = null;
                currentPlayerTurn.CmdDeHighlight();
                onBoard = false;
            }

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, LayerMask.GetMask("ChessPlane")))
            {
                SID_BoardGridSet sid = hit.collider.GetComponent<SID_BoardGridSet>();
                boardselectionX = sid.GridX;
                boardselectionY = sid.GridY;
                onBoard = true;
            }
            else
            {
                onBoard = false;
            }
        }
    }

    public void FixedPositionOne(GameObject yo, Vector3 zoneOne, Vector3 zoneTwo, Vector3 dirOne, Vector3 dirTwo, bool player)
    {
        if (yo != null)
        {
            if (player)
            {
                if (yo.transform.position.x > zoneOne.x)
                {
                    yo.transform.position += dirOne;
                }
            }
            else
            {
                if (yo.transform.position.x < zoneTwo.x)
                {
                    yo.transform.position += dirTwo;
                }
            }
        }
    }

    public void FixedPositionTwo(GameObject yo, Vector3 zoneOne, Vector3 zoneTwo, Vector3 dirOne, Vector3 dirTwo, bool player)
    {
        if (yo != null)
        {
            if (player)
            {
                if (yo.transform.position.x > zoneOne.x)
                {
                    yo.transform.position += dirOne;
                }
            }
            else
            {
                if (yo.transform.position.x < zoneTwo.x)
                {
                    yo.transform.position += dirTwo;
                }
            }
        }
    }

    private void SpawnChessman(int index, int x, int y, bool iswhite, NetworkConnection conn,PlayerInfo cur)
    {
        GameObject go;
        if (iswhite)
        {
            go = Instantiate(p1chessmanPrefabs[index], GetTileCenter(x, y, 0), coordination) as GameObject;
            go.GetComponent<SID_Chessman_Mirror>().authorityPlayer = cur;
            go.GetComponent<NetworkMatchChecker>().matchId = matching;
            NetworkServer.Spawn(go, conn);
        }
        else
        {
            go = Instantiate(p2chessmanPrefabs[index], GetTileCenter(x, y, 1), coordination) as GameObject;
            go.GetComponent<SID_Chessman_Mirror>().authorityPlayer = cur;
            go.GetComponent<NetworkMatchChecker>().matchId = matching;
            NetworkServer.Spawn(go, conn);
        }
        go.GetComponent<SID_Chessman_Mirror>().isWhite = iswhite;
        //go.transform.SetParent(this.transform);
        activeChessman.Add(go);
    }

    public void PlayerOneIntSetMatch(NetworkConnection conn,PlayerInfo curPlayer)
    {

        switch (playerOneArmy)
        {
            case 1:
                SpawnBasicSet(true, conn, curPlayer);
                break;
        }
    }

    public void PlayerTwoIntSetMatch(NetworkConnection conn, PlayerInfo curPlayer)
    {
        switch (playerTwoArmy)
        {
            case 1:
                SpawnBasicSet(false, conn, curPlayer);
                break;
        }
    }


    private void SpawnBasicSet(bool side, NetworkConnection co,PlayerInfo curPlay)
    {
        activeChessman = new List<GameObject>();

        if (side)
        {
            //king
            SpawnChessman(0, 4, 0, side, co,  curPlay);
            //Queen
            SpawnChessman(1, 3, 0, side, co, curPlay);
        }
        else
        {
            //king
            SpawnChessman(0, 3, 0, side, co, curPlay);
            //Queen
            SpawnChessman(1, 4, 0, side, co, curPlay);
        }

        //Rooks
        SpawnChessman(2, 0, 0, side, co, curPlay);
        SpawnChessman(2, 7, 0, side, co, curPlay);

        //Bishop
        SpawnChessman(3, 2, 0, side, co, curPlay);
        SpawnChessman(3, 5, 0, side, co, curPlay);

        //Knights
        SpawnChessman(4, 1, 0, side, co, curPlay);
        SpawnChessman(4, 6, 0, side, co, curPlay);

        //pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(5, i, 1, side, co, curPlay);
        }
    }

    public Vector3 GetTileCenter(int x, int y, int spawn)
    {
        Vector3 origin = originBoardPiece[spawn].transform.position;
        if (spawn == 0)
        {
            origin.x += (TileSize * x) + TileOffset;
            origin.y += 1.01f;
            origin.z += (TileSize * y) - TileOffset;
        }
        else if (spawn == 1)
        {
            origin.x -= (TileSize * x) - TileOffset;
            origin.y += 1.01f;
            origin.z -= (TileSize * y) + TileOffset;
        }
        else if (spawn == 2)
        {
            origin.x += (TileSize * x) + TileOffset;
            origin.y += 1.01f;
            origin.z += (TileSize * y) - TileOffset;
        }
        return origin;
    }
    private bool SameCoord(Points firstPoint, Points secondPoint)
    {
        if (firstPoint.X != secondPoint.X)
            return false;
        else if (firstPoint.Y != secondPoint.Y)
            return false;
        return true;
    }
}

[Serializable]
public class Points
{
    public int X;
    public int Y;
    public Points(int x, int y)
    {
        X = x;
        Y = y;
    }
    public class EqualityComparer : IEqualityComparer<Points>
    {
        public bool Equals(Points x, Points y)
        {
            return x.X == y.X && x.Y == y.Y;
        }
        public int GetHashCode(Points x)
        {
            return x.X ^ x.Y;
        }
    }
}
