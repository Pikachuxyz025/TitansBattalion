using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChessGenerator : NetworkBehaviour
{
    public ChessboardTemplate chessboard;
    [SerializeField] protected GameObject piece;
    public Dictionary<Points, GameObject> setupTiles = new Dictionary<Points, GameObject>(new Points.EqualityComparer());



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
                    if (!isSkippable)
                    {
                        GenerateTile(x, y, id);
                    }
                }
                else
                {
                    GenerateTile(x, y, id);
                }
            }
        }
    }

    private void GenerateTile(int x, int y, int id)
    {
        int xz = x;
        int yz = y;
        Transform thisObjectTransform = this.transform;
        GameObject generatedChessTile = GenerateSingleTile(ref xz, ref yz);
        generatedChessTile.GetComponent<NetworkObject>().Spawn();

        ChessPieceConnection connection = null;
        if (generatedChessTile.GetComponent<ChessPieceConnection>() != null)
            connection = generatedChessTile.GetComponent<ChessPieceConnection>();

        generatedChessTile.transform.parent = thisObjectTransform;

        connection.ChangeGridValue(xz, yz, id);
        connection.GenerateCoordinatesClientRpc(xz, yz);
        ChessPieceManager.instance.AddPoints(xz, yz, generatedChessTile);
        if (thisObjectTransform.GetComponent<IMainBoardInfo>() != null)
            thisObjectTransform.GetComponent<IMainBoardInfo>().CreatePieceList(connection);
    }



    public void SetChessboard(ChessboardTemplate newChess) => chessboard = newChess;

    protected virtual GameObject GenerateSingleTile(ref int x, ref int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));

        return tileObject;
    }
}
