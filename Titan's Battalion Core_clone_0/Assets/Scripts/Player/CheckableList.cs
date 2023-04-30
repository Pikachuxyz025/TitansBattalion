using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckableList
{
    // kingTargets are the pieces targeting the king, the pieces that currently have the king in check
    public List<Chesspiece> kingTargets { get; private set; } = new List<Chesspiece>();
    public List<Chesspiece> playerChesspieces = new List<Chesspiece>();

    public void AddKingTargets(Chesspiece targeterChesspiece) => kingTargets.Add(targeterChesspiece);

    public void AddKingTargets(List<Chesspiece> targeterChesspieces) => kingTargets.AddRange(targeterChesspieces);

    public void RemoveKingTargets(Chesspiece targeterChesspiece)
    {
        if (kingTargets.Contains(targeterChesspiece))
            kingTargets.Remove(targeterChesspiece);
    }

    public void ResetKingTargets() => kingTargets.Clear();

    private bool ContainsVaildMove(ref List<Points> moves, Points pointPos)
    {
        for (int i = 0; i < moves.Count; i++)
            if (Points.DualEquals(moves[i], pointPos))
                return true;
        return false;

    }

    public List<Points> GetDownMyKing(List<Points> currentAvailableMoves,Points kingPosition,Chesspiece currentlyDragging)
    {
        List<Points> resultingPoints = new List<Points>();
        ChessPieceManager chessManager = ChessPieceManager.instance;
        if (kingTargets.Count == 0)
            return currentAvailableMoves;

        for (int i = 0; i < kingTargets.Count; i++)
        {
            for (int j = 0; j < currentAvailableMoves.Count; j++)
            {
                if (currentAvailableMoves[j].ComparePoints(kingTargets[i].GetCurrentPosition()))
                {
                    if (!resultingPoints.Contains(kingTargets[i].GetCurrentPosition()))
                        resultingPoints.Add(kingTargets[i].GetCurrentPosition());

                    continue;
                }

                chessManager.GetChesspieceConnection(currentAvailableMoves[j]).SetOccupiedPiece(currentlyDragging);
                if (kingTargets[i].GetAvailableMoves().Contains(kingPosition))
                {
                    chessManager.GetChesspieceConnection(currentAvailableMoves[j]).SetOccupiedPiece(null);
                    continue;
                }

                if (!resultingPoints.Contains(currentAvailableMoves[j]))
                    resultingPoints.Add(currentAvailableMoves[j]);

                chessManager.GetChesspieceConnection(currentAvailableMoves[j]).SetOccupiedPiece(null);
            }
        }

        return resultingPoints;
    }
}
