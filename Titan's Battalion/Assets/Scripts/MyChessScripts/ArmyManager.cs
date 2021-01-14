using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ArmyManager : MonoBehaviour
{
    [SerializeField]
    private ArmyType[] allArmyTypes;
    [SerializeField]
    private FieldType[] allBoardTypes;

    [HideInInspector]
    public Dictionary<int, Army> allArmies = new Dictionary<int, Army>();
    public Dictionary<int, Field> allBoards = new Dictionary<int, Field>();

    private void Awake()
    {
        for (int i = 0; i < allArmyTypes.Length; i++)
        {
            ArmyType newArmyType = allArmyTypes[i];
            Army newArmy = new Army(i, newArmyType.ArmyOffset, newArmyType.ArmyName, newArmyType.gridMat, newArmyType.ArmyGrid, newArmyType.PiecesPrefabs);
            allArmies[i] = newArmy;
        }
        for (int i = 0; i < allBoardTypes.Length; i++)
        {
            FieldType newFieldType = allBoardTypes[i];
            Field newBoard = new Field(i, newFieldType.FieldName, newFieldType.FieldGrid);
            allBoards[i] = newBoard;
        }
    }
}

public class Army
{
    public int armyId, armyOffset;
    public string armyName;
    public Material armyMaterial;
    public GameObject armyGrid;
    public List<GameObject> piecesPrefabs = new List<GameObject>();

    public Army(int id, int offset, string name, Material mat, GameObject grid, List<GameObject> prefabs)
    {
        armyId = id;
        armyOffset = offset;
        armyName = name;
        armyMaterial = mat;
        armyGrid = grid;
        piecesPrefabs = prefabs;
    }
}
[Serializable]
public struct ArmyType
{
    public string ArmyName;
    public Material gridMat;
    public GameObject ArmyGrid;
    public int ArmyOffset;
    public List<GameObject> PiecesPrefabs;
}

public class Field
{
    public int fieldId;
    public string fieldName;
    public GameObject fieldgrid;

    public Field(int id, string name, GameObject grid)
    {
        fieldId = id;
        fieldName = name;
        fieldgrid = grid;
    }
}

[Serializable]
public struct FieldType
{
    public string FieldName;
    public GameObject FieldGrid;
}