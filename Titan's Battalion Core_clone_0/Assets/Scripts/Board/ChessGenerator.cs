using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChessGenerator : NetworkBehaviour
{
    public Chessboard_Testing chessboard;
    [SerializeField] protected GameObject piece;
    public Dictionary<Points, GameObject> setupTiles = new Dictionary<Points, GameObject>(new Points.EqualityComparer());
    public GameObject[,] tiles;
    private GameObject show;

    [ServerRpc]
    protected void GenerateAllTilesServerRpc(int id)
    {
        if (chessboard == null) return;
        bool isSkippable = false;

        for (int x = 0; x < chessboard.tileCountX; x++)
        {
            for (int y = 0; y < chessboard.tileCountY; y++)
            {
                if (chessboard.removedTilePoints.Length > 0)
                {
                    isSkippable = false;
                    foreach (Points points in chessboard.removedTilePoints)
                    {
                        if (points.SingleEquals(x, y))
                            isSkippable = true;
                    }
                    //Debug.Log(isSkippable + ": " + x + ", " + y);
                    if (!isSkippable)
                    {
                        GenerateTile(x, y,id);
                    }
                }
                else
                {
                    GenerateTile(x, y,id);
                }
            }
        }
    }

    void GenerateTile(int x, int y,int id)
    {
        int xz = x;
        int yz = y;
        show = GenerateSingleTile(ref xz, ref yz);
        show.GetComponent<NetworkObject>().Spawn();

        ChessPieceConnection connection = null;
        if (show.GetComponent<ChessPieceConnection>() != null)
            connection = show.GetComponent<ChessPieceConnection>();

        show.transform.parent = transform;

        connection.ChangeGridValue(xz, yz,id);
        connection.GenerateCoordinatesClientRpc(xz, yz);
        ChessPieceManager.instance.AddPoints(xz, yz, show);
        if (transform.GetComponent<IMainBoardInfo>() != null)
            transform.GetComponent<IMainBoardInfo>().CreatePieceList(connection);
        else
            connection.n_isConnected.Value = true;
    }

    
    private void Set(int x, int y)
    {

    }



    public void SetChessboard(Chessboard_Testing newChess) => chessboard = newChess;

    protected virtual GameObject GenerateSingleTile( ref int x, ref int y)
    {
        int x_R = 0;
        int y_R = 0;
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));

        return tileObject;
    }


    public Points LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < chessboard.tileCountX; x++)
            for (int y = 0; y < chessboard.tileCountY; y++)
                if (tiles[x, y] == hitInfo)
                    return new Points(x, y);
        return new Points(-1, -1);
    }
}
