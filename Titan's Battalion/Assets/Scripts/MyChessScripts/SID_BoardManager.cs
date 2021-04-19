using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Mirror;
using UnityEngine.Events;

public class SID_BoardManager : MonoBehaviour
{
    public static SID_BoardManager Instance { set; get; }

    [HideInInspector]
    public bool buildModeOn = false, setActive = false;
    public static UnityEvent M_eventmoment;
    private ArmyManager aSys;
    public BoardLocation BoLo;

    public Vector3[] buildPos = new Vector3[4];
    private const float TileSize = 1.0f, TileOffset = 0.5f;
    private int selectionX = -1, selectionY = -1, controlarmy;
    public static SID_BoardGridSet[] gridblocksarray;

    public SID_Chessman selectedChessman;

    public List<GameObject> chessmanPrefabs;
    private List<GameObject> activeChessman;

    public bool isWhiteTurn = true;
    public bool onBoard, isWhite;

    [HideInInspector]
    public GameObject[] originBoardPiece = new GameObject[3];
    [HideInInspector]
    public GameObject yo, yoTwo;
    private Quaternion neutralCoordination = Quaternion.Euler(0, 0, 0), coordination = Quaternion.Euler(0, 180, 0);
    public static Dictionary<SID_BoardGridSet, Points> coordinates = new Dictionary<SID_BoardGridSet, Points>();
    public static Dictionary<Points, bool> allMoves = new Dictionary<Points, bool>(new Points.EqualityComparer());
    private Dictionary<Points, bool> allowedMoves = new Dictionary<Points, bool>(new Points.EqualityComparer());

    private void Start()
    {
        Instance = this;

        if (M_eventmoment == null)
            M_eventmoment = new UnityEvent();
        aSys = GetComponent<ArmyManager>();
    }

    void Update()
    {
        if (setActive)
        {
            gridblocksarray = FindObjectsOfType<SID_BoardGridSet>();
            UpdateSelection();
            GenerateCoordinates();
            UpdateAllMoves();
            foreach (SID_BoardGridSet gridblock in gridblocksarray)
            {
                if (gridblock.isFirstPiece)
                {
                    originBoardPiece[2] = gridblock.gameObject;
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (onBoard)
                {
                    if (selectedChessman == null)
                    {
                        //select the chessman
                        ClearMoves();
                        SelectChessman(selectionX, selectionY);
                    }
                    else
                    {
                        //move the chessman
                        MoveChessman(selectionX, selectionY);
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (selectedChessman != null)
                {
                    ClearMoves();
                    SID_BoardHighlight.Instance.HideHighlights();
                    selectedChessman = null;
                }
            }
        }
        BuildArmy();
    }
    public void SetupBoard()
    {
        BoLo = FindObjectOfType<BoardLocation>();
        buildPos[0] = BoLo.playerOnePointA.transform.position;
        buildPos[1] = BoLo.playerOnePointB.transform.position;
        buildPos[2] = BoLo.playerTwoPointA.transform.position;
        buildPos[3] = BoLo.playerTwoPointB.transform.position;
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
    private void ClearMoves()
    {
        foreach (Points point in allMoves.Keys.ToList<Points>())
        {
            allMoves[point] = false;
        }
        SID_BoardHighlight.Instance.HideHighlights();
    }
    private void GenerateCoordinates()
    {
        for (int i = 0; i < gridblocksarray.Length; i++)
        {
            Points coordinate = new Points(gridblocksarray[i].GridX, gridblocksarray[i].GridY);
            if (!coordinates.ContainsKey(gridblocksarray[i]))
                coordinates.Add(gridblocksarray[i], new Points(gridblocksarray[i].GridX, gridblocksarray[i].GridY));
            else
                coordinates[gridblocksarray[i]] = coordinate;
            if (gridblocksarray[i].connected)
            {
                if (!allMoves.ContainsKey(coordinates[gridblocksarray[i]]))
                    allMoves.Add(coordinates[gridblocksarray[i]], false);
            }
        }
    }
    public void MoveChessman(int x, int y)
    {
        if (allMoves[new Points(x, y)])
        {
            for (int i = 0; i < gridblocksarray.Length; i++)
            {
                if (x == gridblocksarray[i].GridX && y == gridblocksarray[i].GridY)
                {
                    SID_Chessman c = gridblocksarray[i].chess;
                    if (c != null && c.isWhite != isWhiteTurn)
                    {
                        activeChessman.Remove(c.gameObject);
                        Destroy(c.gameObject);
                    }
                }
            }
            selectedChessman.transform.position = GetTileCenter(x, y, 2);
            M_eventmoment.Invoke();

            isWhiteTurn = !isWhiteTurn;
            SID_BoardHighlight.Instance.HideHighlights();
            selectedChessman = null;
        }
        else
        {
            for (int i = 0; i < gridblocksarray.Length; i++)
            {
                if (gridblocksarray[i] != null)
                {
                    if (x == gridblocksarray[i].GridX && y == gridblocksarray[i].GridY)
                    {
                        if (gridblocksarray[i].pieceOn && gridblocksarray[i].chess.isWhite == isWhiteTurn)
                        {
                            SID_BoardHighlight.Instance.HideHighlights();
                            selectedChessman = gridblocksarray[i].chess;
                            allowedMoves = selectedChessman.confirmation;
                            SID_BoardHighlight.Instance.HighLightAllowedMoves(allowedMoves);
                        }
                    }
                    else
                    {
                        Debug.Log("move not available");
                        SID_BoardHighlight.Instance.HideHighlights();
                        selectedChessman = null;
                    }
                }
            }
        }
    }
    private void SpawnAllPieces(bool side)
    {
        activeChessman = new List<GameObject>();

        //king
        SpawnChessman(0, 3, 0, side);
        //Queen
        SpawnChessman(1, 4, 0, side);
        //Rooks
        SpawnChessman(2, 0, 0, side);
        SpawnChessman(2, 7, 0, side);

        //Bishop
        SpawnChessman(3, 2, 0, side);
        SpawnChessman(3, 5, 0, side);

        //Knights
        SpawnChessman(4, 1, 0, side);
        SpawnChessman(4, 6, 0, side);

        //pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(5, i, 1, side);
        }
    }
    private void SelectChessman(int x, int y)
    {
        for (int i = 0; i < gridblocksarray.Length; i++)
        {
            if (x == gridblocksarray[i].GridX && y == gridblocksarray[i].GridY)
            {
                if (gridblocksarray[i].pieceOn)
                {
                    if (gridblocksarray[i].chess.isWhite != isWhiteTurn)
                    {
                        Debug.Log("not your men to choose");
                        return;
                    }
                    selectedChessman = gridblocksarray[i].chess;
                    allowedMoves = selectedChessman.confirmation;
                    SID_BoardHighlight.Instance.HighLightAllowedMoves(allowedMoves);
                }
            }
        }
    }
    private void UpdateSelection()
    {
        if (!Camera.main)
            return;

        RaycastHit hit;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, LayerMask.GetMask("ChessPlane")))
        {
            SID_BoardGridSet sid = hit.collider.GetComponent<SID_BoardGridSet>();
            selectionX = sid.GridX;
            selectionY = sid.GridY;
            onBoard = true;
        }
        else
        {
            onBoard = false;
        }
    }
    private void BuildArmy()
    {
        if (buildModeOn && yo != null)
        {
            if (isWhiteTurn)
                FixedPosition(buildPos[0], buildPos[1], Vector3.left, Vector3.right);
            else
                FixedPosition(buildPos[3], buildPos[2], Vector3.left, Vector3.right);

            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (isWhiteTurn)
                    SpawnAllPieces(true);
                else
                    SpawnAllPieces(false);
                yo = null;
                isWhiteTurn = !isWhiteTurn;
                GameManager.turn++;
            }
            if (GameManager.turn >= 2)
            {
                setActive = true;
                M_eventmoment.Invoke();
                buildModeOn = false;
            }
        }
    }
    private void FixedPosition(Vector3 zoneOne, Vector3 zoneTwo, Vector3 dirOne, Vector3 dirTwo)
    {
        if (yo != null)
        {
            if (yo.transform.position.x > zoneOne.x)
            {
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    yo.transform.position += dirOne;
                }
            }
            if (yo.transform.position.x < zoneTwo.x)
            {
                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    yo.transform.position += dirTwo;
                }
            }
        }
    }
    private void SpawnChessman(int index, int x, int y, bool iswhite)
    {
        GameObject go;
        if (iswhite)
            go = Instantiate(chessmanPrefabs[index], GetTileCenter(x, y, 0), coordination) as GameObject;
        else
            go = Instantiate(chessmanPrefabs[index], GetTileCenter(x, y, 1), coordination) as GameObject;
        go.GetComponent<SID_Chessman>().isWhite = iswhite;
        go.transform.SetParent(transform);
        activeChessman.Add(go);
    }
    private Vector3 GetTileCenter(int x, int y, int spawn)
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

