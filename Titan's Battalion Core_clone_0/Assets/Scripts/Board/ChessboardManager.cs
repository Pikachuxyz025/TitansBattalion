using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessboardManager : MonoBehaviour
{
    [SerializeField] private List<Chessboard_Testing> mainBoardList = new List<Chessboard_Testing>();
    [SerializeField] private List<Chessboard_Testing> armyBoardList = new List<Chessboard_Testing>();
    public GameObject[,] tiles;
    public static ChessboardManager instance;


    /*void GenerateAllTiles(Chessboard_Testing chessboard, float tileSize, Transform transform)
    {
        tiles = new GameObject[chessboard.tileCountX, chessboard.tileCountY];
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
                        tiles[x, y] = GenerateSingleTile(tileSize, x, y, transform);
                }
                else
                    tiles[x, y] = GenerateSingleTile(tileSize, x, y, transform);
            }
        }
    }*/

    private void Awake() => instance = this;


    /*public GameObject GenerateSingleTile(float tileSize, int x, int y, Transform transform)
    {
        int x_R = 0;
        int y_R = 0;
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        GameObject pieceSet = new GameObject(string.Format("PieceSpawnPoint"));
        pieceSet.transform.parent = tileObject.transform;
        tileObject.transform.parent = transform;

        // Add to setup tiles to setup piece placement later
        setupTiles.Add(new Points(x, y), tileObject);

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = chessboard.tileMaterial;

        Vector3[] vertics = new Vector3[4];

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertics;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();
        pieceSet.transform.position = tileObject.GetComponent<BoxCollider>().center;
        ChessPieceManager.instance.AddPoints(x_R, y_R, tileObject);

        ChessPieceConnection connection = tileObject.AddComponent<ChessPieceConnection>();
        connection.GenerateCoordinates(x_R, y_R);
        if (transform.GetComponent<IMainBoardInfo>() != null)
            transform.GetComponent<IMainBoardInfo>().CreatePieceList(connection);
        else
            connection.isConnected = true;

        connection.pieceSetPoint = pieceSet;
        return tileObject;
    }*/
}
