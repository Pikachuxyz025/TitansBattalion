using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SID_MoveManager : MonoBehaviour
{
    [HideInInspector] public Dictionary<Points, bool> allMoves = new Dictionary<Points, bool>(new Points.EqualityComparer());
    [HideInInspector] public Dictionary<Points, bool> allowedMoves = new Dictionary<Points, bool>(new Points.EqualityComparer());
    [HideInInspector] public Dictionary<Points, bool> futureMoves = new Dictionary<Points, bool>(new Points.EqualityComparer());
    public SID_Chessman_Mirror currentChessman;
    public bool playerOneKingInDanger, playerTwoKingInDanger;
    

    private void UpdateAllMoves()
    {
        if (allowedMoves.Count != 0)
        {
            foreach (Points pointone in allMoves.Keys.ToList<Points>())
            {
                foreach (Points pointtwo in allowedMoves.Keys.ToList<Points>())
                {
                    if (SameCoord(pointone, pointtwo))
                    {
                        if (allMoves[pointtwo] != allowedMoves[pointtwo])
                            allMoves[pointtwo] = allowedMoves[pointtwo];
                    }
                }
            }
        }
    }

    //Account for future moves
    //take the selected chessman
    //look at all the available moves
    //Go a step further an see what the available moves for each of those points would be
    //if any of those future moves have the king in sight, notify check opportunity


    private bool SameCoord(Points firstPoint, Points secondPoint)
    {
        if (firstPoint.X != secondPoint.X)
            return false;
        else if (firstPoint.Y != secondPoint.Y)
            return false;
        return true;
    }
}
