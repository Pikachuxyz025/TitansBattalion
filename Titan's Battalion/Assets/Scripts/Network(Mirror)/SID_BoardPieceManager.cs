using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// State that show occupancy
public enum PieceState
{
    None,
    Free,
    Enemy,
    Friendly
}

public class SID_BoardPieceManager : MonoBehaviour
{
    public static SID_BoardPieceManager instance;
    [HideInInspector] public SID_BoardGridSet[] gridblocksarray;

    public Dictionary<SID_BoardGridSet, Points> coordinates = new Dictionary<SID_BoardGridSet, Points>();
    public Dictionary<Points, SID_BoardGridSet> reverseCoordinates = new Dictionary<Points, SID_BoardGridSet>(new Points.EqualityComparer());

    [HideInInspector] public GameObject orginPiece;

    private void Awake()
    {
        instance = this;
    }


    private void Update()
    {
        // keep track of all pieces available as they appear 
        gridblocksarray = FindObjectsOfType<SID_BoardGridSet>();

        GenerateCoordinates();

        OriginPieces();
    }


    // crates coordinates to set location for each piece on board
    private void GenerateCoordinates()
    {
        for (int i = 0; i < gridblocksarray.Length; i++)
        {
            Points coordinate = new Points(gridblocksarray[i].GridX, gridblocksarray[i].GridY);

            if (!coordinates.ContainsKey(gridblocksarray[i]))
                coordinates.Add(gridblocksarray[i], new Points(gridblocksarray[i].GridX, gridblocksarray[i].GridY));
            else
                coordinates[gridblocksarray[i]] = coordinate;

            //inverse
            if (!reverseCoordinates.ContainsKey(coordinate))
                reverseCoordinates.Add(coordinate, gridblocksarray[i]);
            else
                reverseCoordinates[coordinate] = gridblocksarray[i];
        }
    }

    // track whether each piece is occupied
    public PieceState VaildatePieces(int targetX, int targetY, SID_Chessman_Mirror chesspiece)
    {
        SID_BoardGridSet gridPiece = null;
        if (reverseCoordinates.ContainsKey(new Points(targetX, targetY)))
            gridPiece = reverseCoordinates[new Points(targetX, targetY)];
        if (gridPiece == null)
            return PieceState.None;

        if (gridPiece.chessM != null)
        {
            if (chesspiece.isWhite == gridPiece.chessM.isWhite)
                return PieceState.Friendly;
            if (chesspiece.isWhite != gridPiece.chessM.isWhite)
                return PieceState.Enemy;
        }
        return PieceState.Free;
    }

    // distigush between origin pieces
    void OriginPieces()
    {
        foreach (SID_BoardGridSet gridblock in gridblocksarray)
        {
            if (gridblock.startingPieceOrigin == BoardStartPoint.IsFirstPiece)
                orginPiece = gridblock.gameObject;
        }
    }

    public bool IsOnMainBoard(int targetX, int targetY)
    {
        SID_BoardGridSet gridPiece = null;
        if (reverseCoordinates.ContainsKey(new Points(targetX, targetY)))
            gridPiece = reverseCoordinates[new Points(targetX, targetY)];
        if (gridPiece != null)
            return gridPiece.isMainBoard;
        return false;
    }

    // give us a way to select pieces on the board
    public Points FindCoordinate(SID_BoardGridSet boardGridSet)
    {
        Points selectedPoint;
        if (coordinates.ContainsKey(boardGridSet))
            selectedPoint = coordinates[boardGridSet];
        else
            selectedPoint = new Points(0, 0);
        return selectedPoint;
    }

    public SID_BoardGridSet FindGridPiece(Points points)
    {
        SID_BoardGridSet gridSet;
        if (reverseCoordinates.ContainsKey(points))
            gridSet = reverseCoordinates[points];
        else
            gridSet = null;
        return gridSet;
    }

    public SID_Chessman_Mirror FindChessman(int targetX, int targetY)
    {
        SID_BoardGridSet gridPiece = null;
        if (reverseCoordinates.ContainsKey(new Points(targetX, targetY)))
            gridPiece = reverseCoordinates[new Points(targetX, targetY)];

            return gridPiece.chessM;
    }
}
