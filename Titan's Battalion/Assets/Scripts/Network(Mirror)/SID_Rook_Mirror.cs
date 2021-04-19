using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using Mirror;

public class SID_Rook_Mirror : SID_Chessman_Mirror
{
    public List<Points> siD;
    public List<Points> seD;
    public bool hasMoved = false;
   // [HideInInspector] public Points mCastleTriggerPoints = null;
    public Points mCastlePoints = null;
    private UnityEvent futureScouter;

    public override void Awake()
    {
        base.Awake();
        if (futureScouter == null)
            futureScouter = new UnityEvent();
    }
    public override void Update()
    {
        base.Update();
        if (rig < 1)
        {
            FindPossiblilties();
        }
        siD = new List<Points>(confirmedMoves.Keys);
        seD = new List<Points>(futureMoves.Keys);
    }

    void AdditionalPlacement()
    {
        //int triggerOffset = !PieceManager.reverseCoordinates.ContainsKey(new Points(CurrentX - 1, CurrentY)) ? 2 : -1;
        //mCastleTriggerPoints = SetPoint(triggerOffset);

        int castleOffset = !PieceManager.reverseCoordinates.ContainsKey(new Points(CurrentX - 1, CurrentY)) ? 3 : -2;
        mCastlePoints = SetPoint(castleOffset);
    }

    bool RookMove(int x, int y, ref Dictionary<Points, bool> confirmation)
    {
        bool r = new bool();
        Points simple = new Points(x, y);
        PieceState pieceStateSimple = PieceManager.VaildatePieces(simple.X, simple.Y, this);

        if (pieceStateSimple == PieceState.Free)
            r = true;
        else if (pieceStateSimple == PieceState.Enemy)
        {
            SID_Chessman_Mirror chessPiece = PieceManager.FindChessman(simple.X, simple.Y);
            if (chessPiece.GetType().ToString() == "SID_King_Mirror")
            {
                Debug.Log("CheckPoint");
                SID_King_Mirror king = chessPiece as SID_King_Mirror;
                if (confirmation == confirmedMoves)
                    king.checkers = CheckState.inCheck;
                else if (confirmation == futureMoves)
                    king.checkers = CheckState.inCheckZone;
            }

            if (!confirmation.ContainsKey(simple))
                confirmation.Add(simple, true);

            r = false;
            return r;
        }
        if (!confirmation.ContainsKey(simple))
            confirmation.Add(simple, r);
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

    public override void CalculateFutureMoves()
    {
        futureMoves.Clear();
        foreach (Points coord in new List<Points>(confirmedMoves.Keys))
        {
            if (confirmedMoves[coord])
                StartCoroutine(FutureSight(coord.X, coord.Y));
        }
    }

    public override IEnumerator CalculateCurrentMoves()
    {
        confirmedMoves.Clear();
        yield return new WaitForSeconds(.1f);
        int x;
        x = CurrentX;
        do
        {
            x++;
        } while (RookMove(x, CurrentY, ref confirmedMoves));
        x = CurrentX;
        do
        {
            x--;
        } while (RookMove(x, CurrentY, ref confirmedMoves));
        x = CurrentY;
        do
        {
            x++;
        } while (RookMove(CurrentX, x, ref confirmedMoves));
        x = CurrentY;
        do
        {
            x--;
        } while (RookMove(CurrentX, x, ref confirmedMoves));
        CalculateFutureMoves();
        yield return rig += 1;
    }

    public override IEnumerator FutureSight(int curX, int curY)
    {
        yield return new WaitForSeconds(.1f);
        int x;
        x = curX;
        do
        {
            x++;
        } while (RookMove(x, curY, ref futureMoves));
        x = curX;
        do
        {
            x--;
        } while (RookMove(x, curY, ref futureMoves));
        x = curY;
        do
        {
            x++;
        } while (RookMove(curX, x, ref futureMoves));
        x = curY;
        do
        {
            x--;
        } while (RookMove(curX, x, ref futureMoves));
    }
}
