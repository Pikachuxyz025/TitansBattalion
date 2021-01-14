using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SID_King_Mirror : SID_Chessman_Mirror
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

    public void KingMove(int x, int y)//, ref bool rig)
    {
        bool r = new bool();
        Points simple = new Points(x, y);
        foreach (SID_BoardGridSet bgs in scouting.Keys)
        {
            if (SameCoord(simple,scouting[bgs]))
            {
                if (!bgs.pieceOn)
                {
                    r = true;
                }
                else if (isWhite != bgs.chessM.isWhite)
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
        KingMove(CurrentX + 1, CurrentY); // up
        KingMove(CurrentX - 1, CurrentY); // down
        KingMove(CurrentX, CurrentY - 1); // left
        KingMove(CurrentX, CurrentY + 1); // right
        KingMove(CurrentX + 1, CurrentY - 1); // up left
        KingMove(CurrentX - 1, CurrentY - 1); // down left
        KingMove(CurrentX + 1, CurrentY + 1); // up right
        KingMove(CurrentX - 1, CurrentY + 1); // down right
        yield return rig += 1;
    }
}
