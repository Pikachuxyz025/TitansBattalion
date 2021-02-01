using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SID_Rook_Mirror : SID_Chessman_Mirror
{
    public List<Points> siD;
    public bool hasMoved = false;
   // [HideInInspector] public Points mCastleTriggerPoints = null;
    public Points mCastlePoints = null;

    public override void Update()
    {
        base.Update();
        if (rig < 1)
        {
            FindPossiblilties();
        }
        siD = new List<Points>(confirmation.Keys);
    }

    void AdditionalPlacement()
    {
        //int triggerOffset = !PieceManager.reverseCoordinates.ContainsKey(new Points(CurrentX - 1, CurrentY)) ? 2 : -1;
        //mCastleTriggerPoints = SetPoint(triggerOffset);

        int castleOffset = !PieceManager.reverseCoordinates.ContainsKey(new Points(CurrentX - 1, CurrentY)) ? 3 : -2;
        mCastlePoints = SetPoint(castleOffset);
    }

    bool RookMove(int x, int y)
    {
        bool r = new bool();
        Points simple = new Points(x, y);
        PieceState pieceStateSimple = PieceManager.VaildatePieces(simple.X, simple.Y, this);

        if (pieceStateSimple == PieceState.Free)
            r = true;
        else if (pieceStateSimple == PieceState.Enemy)
        {
            if (!confirmation.ContainsKey(simple))
                confirmation.Add(simple, true);
            r = false;
            return r;
        }
        if (!confirmation.ContainsKey(simple))
            confirmation.Add(simple, r);

        /*foreach (SID_BoardGridSet bgs in scouting.Keys)
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
        }*/
        return r;
    }

    public void Castle()
    {
        //Set our target
        Vector3 targetPoint = SID_BoardManager_Mirror.Instance.GetTileCenter(mCastlePoints.X, mCastlePoints.Y, this.whiteInt);

        // Move
        gameObject.transform.position = targetPoint;
        Reset();
        hasMoved = true;
    }

    public Points SetPoint(int offset)
    {
        // New Position
        Points newPosition = new Points(CurrentX, CurrentY);
        newPosition.X += offset;

        // Return
        return newPosition;
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
