using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SID_Queen : SID_Chessman
{
    public List<Points> siD;
    public override void Update()
    {
        base.Update();
        if (rig < 1)
        {
            FindPossiblilties();
        }
        siD = new List<Points>(confirmation.Keys);
    }
    public bool QueenMoves(int x, int y)
    {
        bool r = new bool();
        Points simple = new Points(x, y);
        foreach (SID_BoardGridSet bgs in scouting.Keys)
        {
            if (SameCoord(simple, scouting[bgs]))
            {
                if (!bgs.pieceOn)
                {
                    r = true;
                }
                else if (isWhite != bgs.chess.isWhite)
                {
                    if (!confirmation.ContainsKey(scouting[bgs]))
                        confirmation.Add(scouting[bgs], true);
                    r = false;
                    return r;
                }
                if (!confirmation.ContainsKey(scouting[bgs]))
                    confirmation.Add(scouting[bgs], r);
            }
        }
        return r;
    }

    public override IEnumerator RemoveEnough()
    {
        confirmation.Clear();
        yield return new WaitForSeconds(.1f);
        int x, y;
        x = CurrentX;
        y = CurrentY;
        do
        {
            x--;
            y++;
        } while (QueenMoves(x, y));

        x = CurrentX;
        y = CurrentY;
        do
        {
            x++;
            y++;
        } while (QueenMoves(x, y));

        x = CurrentX;
        y = CurrentY;
        do
        {
            x--;
            y--;
        } while (QueenMoves(x, y));

        x = CurrentX;
        y = CurrentY;
        do
        {
            x++;
            y--;
        } while (QueenMoves(x, y));

        x = CurrentX;
        do
        {
            x++;
        } while (QueenMoves(x, CurrentY));

        x = CurrentX;
        do
        {
            x--;
        } while (QueenMoves(x, CurrentY));

        x = CurrentY;
        do
        {
            x++;
        } while (QueenMoves(CurrentX, x));

        x = CurrentY;
        do
        {
            x--;
        } while (QueenMoves(CurrentX, x));

        yield return rig += 1;
    }
}
