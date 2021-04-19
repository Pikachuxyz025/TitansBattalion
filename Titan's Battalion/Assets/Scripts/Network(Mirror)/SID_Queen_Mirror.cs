using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class SID_Queen_Mirror : SID_Chessman_Mirror
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
    public bool QueenMoves(int x, int y, ref Dictionary<Points, bool> confirmation)
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
        int x, y;
        x = CurrentX;
        y = CurrentY;
        do
        {
            x--;
            y++;
        } while (QueenMoves(x, y, ref confirmedMoves));

        x = CurrentX;
        y = CurrentY;
        do
        {
            x++;
            y++;
        } while (QueenMoves(x, y, ref confirmedMoves));

        x = CurrentX;
        y = CurrentY;
        do
        {
            x--;
            y--;
        } while (QueenMoves(x, y, ref confirmedMoves));

        x = CurrentX;
        y = CurrentY;
        do
        {
            x++;
            y--;
        } while (QueenMoves(x, y, ref confirmedMoves));

        x = CurrentX;
        do
        {
            x++;
        } while (QueenMoves(x, CurrentY, ref confirmedMoves));

        x = CurrentX;
        do
        {
            x--;
        } while (QueenMoves(x, CurrentY, ref confirmedMoves));

        x = CurrentY;
        do
        {
            x++;
        } while (QueenMoves(CurrentX, x, ref confirmedMoves));

        x = CurrentY;
        do
        {
            x--;
        } while (QueenMoves(CurrentX, x, ref confirmedMoves));

        CalculateFutureMoves();
        yield return rig += 1;
    }

    public override IEnumerator FutureSight(int curX, int curY)
    {
        yield return new WaitForSeconds(.1f);
        int x, y;
        x = curX;
        y = curY;
        do
        {
            x--;
            y++;
        } while (QueenMoves(x, y, ref futureMoves));

        x = curX;
        y = curY;
        do
        {
            x++;
            y++;
        } while (QueenMoves(x, y, ref futureMoves));

        x = curX;
        y = curY;
        do
        {
            x--;
            y--;
        } while (QueenMoves(x, y, ref futureMoves));

        x = curX;
        y = curY;
        do
        {
            x++;
            y--;
        } while (QueenMoves(x, y, ref futureMoves));

        x = curX;
        do
        {
            x++;
        } while (QueenMoves(x, curY, ref futureMoves));

        x = curX;
        do
        {
            x--;
        } while (QueenMoves(x, curY, ref futureMoves));

        x = curY;
        do
        {
            x++;
        } while (QueenMoves(curX, x, ref futureMoves));

        x = curY;
        do
        {
            x--;
        } while (QueenMoves(curX, x, ref futureMoves));
    }
}
