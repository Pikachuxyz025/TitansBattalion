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
        //buildPos.Callback += UpdatedSetUpBoard;
    }
    public override void OnStartAuthority()
    {
        Debug.Log("I'm here now");
    }

    private void Update()
    {
        if (setActive)
        {
            //UpdateSelection();
            AccountAllMoves();
            UpdateAllMoves();
            originBoardPiece[2] = PieceManager.orginPiece;
        }
        if (isServer)
            RpcHightlight();
        //BuildArmy();
    }

    [ClientRpc]
    public void RpcHightlight()
    {
        if(!highlightOn)
            SID_BoardHighlight_Mirror.Instance.HideHighlights();
    }

    //[ClientRpc]
    public void SelectandMove()
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

    public void MoveChessman(int x, int y)
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
            //RpcMove(x, y);
            M_eventmoment.Invoke();

            isWhiteTurn = !isWhiteTurn;
            //SID_BoardHighlight_Mirror.Instance.RpcHideHighlights();
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
                            //SID_BoardHighlight_Mirror.Instance.RpcHideHighlights();
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
                        //SID_BoardHighlight_Mirror.Instance.RpcHideHighlights();
                        highlightOn = false;
                        selectedChessmanPlayer = null;
                    }
                }
            }
        }
    }

    [ClientRpc]
    private void RpcMove(int x,int y)
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

    public void SelectChessman(int x, int y)
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


    public void UpdateSelection()
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
            SID_BoardGridSet  sid = hit.collider.GetComponent<SID_BoardGridSet>();
            boardselectionX = sid.GridX;
            boardselectionY = sid.GridY;
            onBoard = true;
        }
        else
        {
            onBoard = false;
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

    private void SpawnChessman(int index, int x, int y, bool iswhite,NetworkConnection conn)
    {
        GameObject go;
        if (iswhite)
        {
            go = Instantiate(p1chessmanPrefabs[index], GetTileCenter(x, y, 0), coordination) as GameObject;
            NetworkServer.Spawn(go, conn);
        }
        else
        {
            go = Instantiate(p2chessmanPrefabs[index], GetTileCenter(x, y, 1), coordination) as GameObject;
            NetworkServer.Spawn(go, conn);
        }
        go.GetComponent<SID_Chessman_Mirror>().isWhite = iswhite;
        //go.transform.SetParent(this.transform);
        activeChessman.Add(go);
    }

    public void PlayerOneIntSetMatch(NetworkConnection conn)
    {
        switch (playerOneArmy)
        {
            case 1:
                SpawnBasicSet(true,conn);
                break;
        }
    }

    public void PlayerTwoIntSetMatch(NetworkConnection conn)
    {
        switch (playerTwoArmy)
        {
            case 1:
                SpawnBasicSet(false,conn);
                break;
        }
    }


    private void SpawnBasicSet(bool side,NetworkConnection co)
    {
        activeChessman = new List<GameObject>();

        if (side)
        {
            //king
            SpawnChessman(0, 4, 0, side, co);
            //Queen
            SpawnChessman(1, 3, 0, side, co);
        }
        else
        {
            //king
            SpawnChessman(0, 3, 0, side, co);
            //Queen
            SpawnChessman(1, 4, 0, side, co);
        }

        //Rooks
        SpawnChessman(2, 0, 0, side,co);
        SpawnChessman(2, 7, 0, side,co);

        //Bishop
        SpawnChessman(3, 2, 0, side,co);
        SpawnChessman(3, 5, 0, side,co);

        //Knights
        SpawnChessman(4, 1, 0, side,co);
        SpawnChessman(4, 6, 0, side,co);

        //pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(5, i, 1, side,co);
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
