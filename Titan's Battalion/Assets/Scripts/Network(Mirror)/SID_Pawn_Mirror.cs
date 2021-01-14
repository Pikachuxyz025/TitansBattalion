using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SID_Pawn_Mirror : SID_Chessman_Mirror
{
    public List<Points> siD;

    //[ServerCallback]
    public override void Update()
    {
        base.Update();
        if (rig < 1)
        {
                FindPossiblilties();
        }
        siD = new List<Points>(confirmation.Keys);
    }
    void PawnMovementStraight(int x, int y)
    {
        Points simple = new Points(x, y);
        bool r = new bool();
        foreach (SID_BoardGridSet bgs in scouting.Keys)
        {
            if (SameCoord(simple, scouting[bgs]))
            {
                if (!bgs.pieceOn)
                {
                    r = true;
                }
                if (!confirmation.ContainsKey(scouting[bgs]))
                {
                    confirmation.Add(scouting[bgs], r);
                }
            }
        }
    }

    void PawnMovementDiagonal(int x, int y)
    {
        Points simple = new Points(x, y);
        bool r = new bool();
        foreach (SID_BoardGridSet bgs in scouting.Keys)
        {
            if (SameCoord(simple, scouting[bgs]))
            {
                if (bgs.pieceOn && isWhite != bgs.chessM.isWhite)
                {
                    r = true;
                }
                if (!confirmation.ContainsKey(scouting[bgs]))
                {
                    confirmation.Add(scouting[bgs], r);
                }
            }
        }
    }
    public override IEnumerator RemoveEnough()
    {
        confirmation.Clear();
        yield return new WaitForSeconds(.1f);
        if (!curOnMainBoard)
        {
            if (isWhite)
            {
                PawnMovementStraight(CurrentX, CurrentY + 1);
                PawnMovementStraight(CurrentX, CurrentY + 2);
                PawnMovementDiagonal(CurrentX - 1, CurrentY + 1);
                PawnMovementDiagonal(CurrentX + 1, CurrentY + 1);
            }
            else
            {
                PawnMovementStraight(CurrentX, CurrentY - 1);
                PawnMovementStraight(CurrentX, CurrentY - 2);
                PawnMovementDiagonal(CurrentX - 1, CurrentY - 1);
                PawnMovementDiagonal(CurrentX + 1, CurrentY - 1);
            }
        }
        else
        {
            if (isWhite)
            {
                PawnMovementStraight(CurrentX, CurrentY + 1);
                PawnMovementDiagonal(CurrentX - 1, CurrentY + 1);
                PawnMovementDiagonal(CurrentX + 1, CurrentY + 1);
            }
            else
            {
                PawnMovementStraight(CurrentX, CurrentY - 1);
                PawnMovementDiagonal(CurrentX - 1, CurrentY - 1);
                PawnMovementDiagonal(CurrentX + 1, CurrentY - 1);
            }
        }
        yield return rig += 1;
    }
}
