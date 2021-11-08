using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SID_Knight : SID_Chessman
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

    public void KnightMove(int x, int y)//, ref bool rig)
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
                    r = true;
                if (!confirmation.ContainsKey(scouting[bgs]))
                    confirmation.Add(scouting[bgs], r);
            }
        }
    }
    public override IEnumerator RemoveEnough()
    {
        confirmation.Clear();
        yield return new WaitForSeconds(.1f);
        KnightMove(CurrentX - 1, CurrentY + 2);
        KnightMove(CurrentX + 1, CurrentY + 2);
        KnightMove(CurrentX + 1, CurrentY - 2);
        KnightMove(CurrentX - 1, CurrentY - 2);
        KnightMove(CurrentX - 2, CurrentY + 1);
        KnightMove(CurrentX + 2, CurrentY - 1);
        KnightMove(CurrentX - 2, CurrentY - 1);
        KnightMove(CurrentX + 2, CurrentY + 1);
        yield return rig += 1;
    }
}
