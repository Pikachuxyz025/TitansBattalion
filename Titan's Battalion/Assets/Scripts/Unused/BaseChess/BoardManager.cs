using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { set; get; }
    public enum Armies { BChess, Sun, TBChess };
    private bool[,] allowedmoves { set; get; }

    public Chessman[,] Chessmans { set; get; }
    private Chessman selectedChessman;

    private const float TileSize = 1.0f, TileOffset = 0.5f;
    private int selectionX = -1, selectionY = -1;
    private Quaternion coordination = Quaternion.Euler(0, 180, 0);

    public List<GameObject> chessmanPrefabs;
    private List<GameObject> activeChessman;

    public bool isWhiteTurn = true,onBoard;

    private void Start()
    {
        Instance = this;
        SpawnAllChessmans();
    }
    private void Update()
    {
        UpdateSelection();
        DrawChessBoard();
        //Debug.Log(selectedChessman);
        if (Input.GetMouseButtonDown(0))
        {
            if (selectionX >= 0 && selectionY >= 0)
            {
                
                if (selectedChessman == null)
                {
                    //select the chessman
                    SelectChessman(selectionX, selectionY);
                }
                else
                {
                    //move the chessman
                    MoveChessman(selectionX, selectionY);
                }
            }
        }
        Debug.Log(selectionX + ", " + selectionY);
    }
    
    private void SelectChessman(int x,int y)
    {
        if (Chessmans[x, y] == null)
            return;
        if (Chessmans[x, y].isWhite != isWhiteTurn)
            return;
        allowedmoves = Chessmans[x, y].PossibleMove();
        selectedChessman = Chessmans[x, y];
        BoardHighlights.Instance.HighLightAllowedMoves(allowedmoves);
    }
    private void MoveChessman(int x,int y)
    {
        if (allowedmoves[x,y])
        {
            Chessman c = Chessmans[x, y];

            if (c != null && c.isWhite != isWhiteTurn)
            {
               

                //If it is the king
                if (c.GetType() == typeof(King))
                {
                    //End of Game
                    //Return
                }
                //capture piece
                activeChessman.Remove(c.gameObject);
                Destroy(c.gameObject);
            }

            Chessmans[selectedChessman.CurrentX, selectedChessman.CurrentY] = null;
            selectedChessman.transform.position = GetTileCenter(x, y);
            selectedChessman.SetPosition(x, y);
            Chessmans[x, y] = selectedChessman;
            isWhiteTurn = !isWhiteTurn;
        }
        BoardHighlights.Instance.HideHighlights();
        selectedChessman = null;
        
    }
    
    private void UpdateSelection()
    {
        if (!Camera.main)
            return;

        RaycastHit hit;
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out hit, Mathf.Infinity, LayerMask.GetMask("ChessPlane")))
        {
            //Debug.Log(hit.point);
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
            onBoard = true;
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
            onBoard = false;
        }
    }
    
    private void SpawnChessman(int index, int x, int y)
    {
        GameObject go = Instantiate(chessmanPrefabs[index], GetTileCenter(x, y), coordination) as GameObject;
        go.transform.SetParent(transform);
        Chessmans[x, y] = go.GetComponent<Chessman>();
        Chessmans[x, y].SetPosition(x, y);
        activeChessman.Add(go);
    }
    private void SpawnAllChessmans()
    {
        activeChessman = new List<GameObject>();
        Chessmans = new Chessman[8, 8];
        //spawn white team

        //king
        SpawnChessman(0, 3, 0);
        //Queen
        SpawnChessman(1, 4, 0);
        //Rooks
        SpawnChessman(2, 0, 0);
        SpawnChessman(2, 7, 0);

        //Bishop
        SpawnChessman(3, 2, 0);
        SpawnChessman(3, 5, 0);

        //Knights
        SpawnChessman(4, 1, 0);
        SpawnChessman(4, 6, 0);

        //pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(5, i, 1);
        }

        //Spawn the Black team!

        //King
        SpawnChessman(6, 4, 7);

        //Queen
        SpawnChessman(7, 3, 7);

        //Rooks
        SpawnChessman(8, 0, 7);
        SpawnChessman(8, 7, 7);

        //Bishop
        SpawnChessman(9, 2, 7);
        SpawnChessman(9, 5, 7);

        //knights
        SpawnChessman(10, 1, 7);
        SpawnChessman(10, 6, 7);

        //pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(11, i, 6);
        }
    }
    private void DrawChessBoard()
    {
        Vector3 widthLine = Vector3.right * 8;
        Vector3 heightLine = Vector3.forward * 8;
        for (int i = 0; i <= 8; i++)
        {
            Vector3 start = Vector3.forward * i;
            Debug.DrawLine(start, start + widthLine);
            for (int j = 0; j <= 8; j++)
            {
                Vector3 go = Vector3.right * j;
                Debug.DrawLine(go, go + heightLine);
            }
        }
        //crossmark selection
        if (selectionX >= 0 && selectionY >= 0)
        {
            Debug.DrawLine(
                Vector3.forward * selectionY + Vector3.right * selectionX,
                Vector3.forward * (selectionY + 1) + Vector3.right * (selectionX + 1));

            Debug.DrawLine(
                Vector3.forward * (selectionY + 1) + Vector3.right * selectionX,
                Vector3.forward * selectionY + Vector3.right * (selectionX + 1));
        }
    }
    private Vector3 GetTileCenter(int x,int y)
    {
        Vector3 origin = Vector3.zero;
        origin.x += (TileSize * x) + TileOffset;
        origin.z += (TileSize * y) + TileOffset;
        return origin;
    }
}
