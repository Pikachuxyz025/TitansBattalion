using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SID_Rook_Mirror : SID_Chessman_Mirror
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
    bool RookMove(int x, int y)
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
                else if (isWhite != bgs.chessM.isWhite)
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
        int x;
        x = CurrentX;
        do
        {
            x++;
        } while (RookMove(x, CurrentY));
        x = CurrentX;
        do
        {
            x--;
        } while (RookMove(x, CurrentY));
        x = CurrentY;
        do
        {
            x++;
        } while (RookMove(CurrentX, x));
        x = CurrentY;
        do
        {
            x--;
        } while (RookMove(CurrentX, x));
        yield return rig += 1;
    }
}
