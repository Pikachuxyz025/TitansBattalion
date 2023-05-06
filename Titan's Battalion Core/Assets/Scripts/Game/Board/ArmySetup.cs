using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "New Army", menuName = "Setup/Armies")]
public class ArmySetup : ScriptableObject
{

    [Header("Prefabs and Marterials")]
    public string ArmyName;
    [Tooltip("Make sure the index number of the prefab matches the index number of the the army coordinates")]
    public List<GameObject> prefabs = new List<GameObject>();
    [Tooltip("Make sure the index number of the prefab matches the index number of the the army coordinates")]
    public List<Points> armyCoordinates = new List<Points>();
    [SerializeField] private Material[] materials;

    public Dictionary<GameObject, Points> armySet()
    {
        Dictionary<GameObject, Points> army = new Dictionary<GameObject, Points>();
        for (int i = 0; i < prefabs.Count; i++)
        {
            army.Add(prefabs[i], armyCoordinates[i]);
        }
        return army;
    }
}


public enum Basic
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}