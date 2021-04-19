using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class SID_Knight_Mirror : SID_Chessman_Mirror
{
    public List<Points> siD;
    public List<Points> seD;
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

    public void KnightMove(int x, int y, ref Dictionary<Points, bool> confirmation)
    {
        bool r = new bool();
        Points simple = new Points(x, y);
        PieceState pieceStateSimple = PieceManager.VaildatePieces(simple.X, simple.Y, this);

        if (pieceStateSimple == PieceState.Free)
            r = true;
        else if (pieceStateSimple == PieceState.Enemy)
        {
            r = true;
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
        }

        if (!confirmation.ContainsKey(simple))
            confirmation.Add(simple, r);
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
        KnightMove(CurrentX - 1, CurrentY + 2, ref confirmedMoves);
        KnightMove(CurrentX + 1, CurrentY + 2, ref confirmedMoves);
        KnightMove(CurrentX + 1, CurrentY - 2, ref confirmedMoves);
        KnightMove(CurrentX - 1, CurrentY - 2, ref confirmedMoves);
        KnightMove(CurrentX - 2, CurrentY + 1, ref confirmedMoves);
        KnightMove(CurrentX + 2, CurrentY - 1, ref confirmedMoves);
        KnightMove(CurrentX - 2, CurrentY - 1, ref confirmedMoves);
        KnightMove(CurrentX + 2, CurrentY + 1, ref confirmedMoves);
        CalculateFutureMoves();
        yield return rig += 1;
    }

    public override IEnumerator FutureSight(int curX, int curY)
    {
        yield return new WaitForSeconds(.1f);
        KnightMove(curX - 1, curY + 2, ref futureMoves);
        KnightMove(curX + 1, curY + 2, ref futureMoves);
        KnightMove(curX + 1, curY - 2, ref futureMoves);
        KnightMove(curX - 1, curY - 2, ref futureMoves);
        KnightMove(curX - 2, curY + 1, ref futureMoves);
        KnightMove(curX + 2, curY - 1, ref futureMoves);
        KnightMove(curX - 2, curY - 1, ref futureMoves);
        KnightMove(curX + 2, curY + 1, ref futureMoves);
    }
}
