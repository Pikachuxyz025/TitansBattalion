using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Board", menuName = "Setup/Boards")]
public class ChessboardTemplate : ScriptableObject
{
    [Header("Art Stuff")]
    public Material tileMaterial;
    //[SerializeField] private GameObject prefab;

    [Header("Tile Counts")]
    public int tileCountX = 8;
    public int tileCountY = 8;

    public GameObject[,] tiles;
    public Points[] removedTilePoints;
    public Points[] TileEdgedPoints;

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
[CustomEditor(typeof(ChessboardTemplate))]
public class Chessboard_Testing_Editor:Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ChessboardTemplate chessboard_Testing = (ChessboardTemplate)target;

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

