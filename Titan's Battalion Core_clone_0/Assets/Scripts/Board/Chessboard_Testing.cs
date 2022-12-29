using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Board", menuName = "Setup/Boards")]
public class Chessboard_Testing : ScriptableObject
{
    [Header("Art Stuff")]
    public Material tileMaterial;
    //[SerializeField] private GameObject prefab;

    [Header("Tile Counts")]
    public int tileCountX = 8;
    public int tileCountY = 8;

    public GameObject[,] tiles;
    public Points[] removedTilePoints;

    [HideInInspector]
    public bool isArmy = false;
    [HideInInspector]
    public ArmySetup tieInArmy;

    [Header("Prefabs and Marterials")]
    [HideInInspector]  public string ArmyName;
    [Tooltip("Make sure the index number of the prefab matches the index number of the the army coordinates")]
    [HideInInspector] public List<GameObject> prefabs = new List<GameObject>();
    [Tooltip("Make sure the index number of the prefab matches the index number of the the army coordinates")]
    [HideInInspector] public List<Points> armyCoordinates = new List<Points>();

    [HideInInspector] public int mainBoardTileCountX = 0;
    [HideInInspector] public int mainBoardTileCountY = 0;

    public Dictionary<Points, GameObject> setupTiles = new Dictionary<Points, GameObject>(new Points.EqualityComparer());



    /*public void GenerateAllTiles(float tileSize, Transform transform, int playerNum = 0)
    {
        tiles = new GameObject[tileCountX, tileCountY];
        bool isSkippable = false;

        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                if (removedTilePoints.Length > 0)
                {
                    isSkippable = false;
                    foreach (Points points in removedTilePoints)
                    {
                        if (points.SingleEquals(x, y))
                            isSkippable = true;
                    }
                    //Debug.Log(isSkippable + ": " + x + ", " + y);
                    if (!isSkippable)
                        tiles[x, y] = GenerateSingleTile(tileSize, x, y, transform, playerNum);
                }
                else
                    tiles[x, y] = GenerateSingleTile(tileSize, x, y, transform, playerNum);
            }
        }
    } */

    /*private GameObject GenerateSingleTile(float tileSize, int x, int y, Transform transform, int playerNum = 0)
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
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertics = new Vector3[4];

        if (!isArmy)
        {
            //pieceSet.transform.position = new Vector3(x + .5f, 0 + .1f, y + .5f);
            vertics[0] = new Vector3(x * tileSize, 0, y * tileSize);
            vertics[1] = new Vector3(x * tileSize, 0, (y + 1) * tileSize);
            vertics[2] = new Vector3((x + 1) * tileSize, 0, y * tileSize);
            vertics[3] = new Vector3((x + 1) * tileSize, 0, (y + 1) * tileSize);
            x_R = x;
            y_R = y;
        }
        else
        {
            switch (playerNum)
            {
                case 1:
                    //pieceSet.transform.position = new Vector3(x + .5f, 0 + .1f, y + mainBoardTileCountY + .5f);
                    vertics[0] = new Vector3((x) * tileSize, 0, (y + mainBoardTileCountY) * tileSize);
                    vertics[1] = new Vector3((x) * tileSize, 0, ((y + mainBoardTileCountY) + 1) * tileSize);
                    vertics[2] = new Vector3(((x) + 1) * tileSize, 0, (y + mainBoardTileCountY) * tileSize);
                    vertics[3] = new Vector3(((x) + 1) * tileSize, 0, ((y + mainBoardTileCountY) + 1) * tileSize);
                    x_R = x;
                    y_R = y + mainBoardTileCountY;
                    break;
                case 2:
                    //pieceSet.transform.position = new Vector3(x + .5f, 0 + .1f, y + mainBoardTileCountY + .5f);
                    vertics[0] = new Vector3((x) * tileSize, 0, (y - mainBoardTileCountY + 5) * tileSize);
                    vertics[1] = new Vector3((x) * tileSize, 0, ((y - mainBoardTileCountY + 5) + 1) * tileSize);
                    vertics[2] = new Vector3(((x) + 1) * tileSize, 0, (y - mainBoardTileCountY + 5) * tileSize);
                    vertics[3] = new Vector3(((x) + 1) * tileSize, 0, ((y - mainBoardTileCountY + 5) + 1) * tileSize);
                    x_R = x;
                    y_R = y - mainBoardTileCountY + 5;
                    break;
                case 3:
                    break;
                case 4:
                    break;

            }
        }

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

    public Points LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                if (tiles[x, y] == hitInfo)
                    return new Points(x, y);
        return new Points(-1, -1);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Chessboard_Testing))]
public class Chessboard_Testing_Editor:Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Chessboard_Testing chessboard_Testing = (Chessboard_Testing)target;

        // draw checkbox for the bool
        chessboard_Testing.isArmy = EditorGUILayout.Toggle("Has Army?", chessboard_Testing.isArmy);
        if (chessboard_Testing.isArmy) // if is true show army
        {
            //chessboard_Testing.tieInArmy = EditorGUILayout.ObjectField("Add Army", chessboard_Testing.tieInArmy, typeof(ArmySetup), true) as ArmySetup;
            chessboard_Testing.ArmyName = EditorGUILayout.TextField("Name Army", chessboard_Testing.ArmyName);

            List<GameObject> pieces = chessboard_Testing.prefabs;
            List<Points> coordinates = chessboard_Testing.armyCoordinates;

            int size = Mathf.Max(0, EditorGUILayout.DelayedIntField("Size", pieces.Count));
            while(size > pieces.Count)
            {
                pieces.Add(null);
                coordinates.Add(new Points(0, 0));
            }

            while (size < pieces.Count)
            {
                pieces.RemoveAt(pieces.Count - 1);
                coordinates.RemoveAt(coordinates.Count - 1);
            }

            for (int i = 0; i < pieces.Count; i++)
            {
                EditorGUILayout.Space();
                pieces[i] = EditorGUILayout.ObjectField("Piece " + i, pieces[i], typeof(GameObject), true) as GameObject;

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Piece " + i + " Coordinates X", GUILayout.MaxWidth(150));
                coordinates[i].X = EditorGUILayout.IntField(coordinates[i].X);

                EditorGUILayout.LabelField("Piece " + i + " Coordinates Y", GUILayout.MaxWidth(150));
                coordinates[i].Y = EditorGUILayout.IntField(coordinates[i].Y);

                EditorGUILayout.EndHorizontal();
            }
        }
        if (GUI.changed)
            EditorUtility.SetDirty(chessboard_Testing);
    }
}
#endif

