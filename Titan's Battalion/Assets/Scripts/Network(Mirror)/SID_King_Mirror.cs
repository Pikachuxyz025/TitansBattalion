using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class SID_King_Mirror : SID_Chessman_Mirror
{
    public List<Points> siD;
    public List<Points> seD;
    public List<bool> stD;
    public bool hasMoved = false;

    public CheckState checkers;

    public bool checkmate;

    private SID_Rook_Mirror leftRook = null, rightRook = null;

    public override void OnStartAuthority()
    {
        authorityPlayer.Setsking(this);
    }

    public override void Awake()
    {
        base.Awake();
        Debug.Log("who goes first: me");
    }
    public override void Reset()
    {
        checkers = CheckState.Safe;
        base.Reset();
    }
    private void OnDestroy()
    {
        if (authorityPlayer != null)
            authorityPlayer.Setsking(null);
    }
    public override void Update()
    {
        base.Update();
        if (rig < 1)
        {
            //AdditionalPossibilities();
            FindPossiblilties();
        }
        siD = new List<Points>(confirmedMoves.Keys);
        seD = new List<Points>(futureMoves.Keys);
        stD = new List<bool>(confirmedMoves.Values);
        checkmate = InCheckmate();
    }

    void AdditionalPossibilities()
    {
        // Right
        rightRook = GetRook(3, 1);

        // Left
        leftRook = GetRook(4, -1);
    }

    public void KingMove(int x, int y, ref Dictionary<Points, bool> confirmation)
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

    public void Castling(int x, int y)
    {
        // Left
        if (CastlingOpportunity(leftRook,x,y))
            leftRook.Castle();
        // Right
        if (CastlingOpportunity(rightRook,x,y))
            rightRook.Castle();
    }

    public bool CastlingOpportunity(SID_Rook_Mirror rookie, int x, int y)
    {
        if (rookie == null)
            return false;

        if (rookie.mCastlePoints != new Points(x, y))
            return false;

        return true;
    }

    public SID_Rook_Mirror GetRook(int count, int direction)
    {
        // Has the king moved
        if (hasMoved)
            return null;

        // Go through the board pieces
        for (int i = 1; i < count; i++)
        {
            int offset = CurrentX + (i * direction);
            PieceState pieceState = PieceManager.VaildatePieces(offset, CurrentY, this);

            if (pieceState != PieceState.Free)
                return null;
        }

        // Try and get rook
        SID_BoardGridSet rookPiece = PieceManager.reverseCoordinates[new Points(CurrentX + (count * direction), CurrentY)];
        SID_Rook_Mirror rook = null;

        // Cast
        if (rookPiece.chessM != null)
        {
            if (rookPiece.chessM is SID_Rook_Mirror)
                rook = (SID_Rook_Mirror)rookPiece.chessM;
        }

        // return if no rook
        if (rook == null)
            return null;

        // Check color and movemnt
        if (rook.isWhite != isWhite || rook.hasMoved)
            return null;

        // Add castle trigger to movement
        if (!confirmedMoves.ContainsKey(rook.mCastlePoints))
            confirmedMoves.Add(rook.mCastlePoints, !rook.hasMoved);

        return rook;
    }

    public override IEnumerator CalculateCurrentMoves()
    {
        confirmedMoves.Clear();
        yield return new WaitForSeconds(.1f);
        KingMove(CurrentX + 1, CurrentY, ref confirmedMoves); // up
        KingMove(CurrentX - 1, CurrentY, ref confirmedMoves); // down
        KingMove(CurrentX, CurrentY - 1, ref confirmedMoves); // left
        KingMove(CurrentX, CurrentY + 1, ref confirmedMoves); // right
        KingMove(CurrentX + 1, CurrentY - 1, ref confirmedMoves); // up left
        KingMove(CurrentX - 1, CurrentY - 1, ref confirmedMoves); // down left
        KingMove(CurrentX + 1, CurrentY + 1, ref confirmedMoves); // up right
        KingMove(CurrentX - 1, CurrentY + 1, ref confirmedMoves); // down right
        CalculateFutureMoves();
        yield return rig += 1;
    }

    public override IEnumerator FutureSight(int curX, int curY)
    {
        yield return new WaitForSeconds(.1f);
        KingMove(curX + 1, curY, ref futureMoves); // up
        KingMove(curX - 1, curY, ref futureMoves); // down
        KingMove(curX, curY - 1, ref futureMoves); // left
        KingMove(curX, curY + 1, ref futureMoves); // right
        KingMove(curX + 1, curY - 1, ref futureMoves); // up left
        KingMove(curX - 1, curY - 1, ref futureMoves); // down left
        KingMove(curX + 1, curY + 1, ref futureMoves); // up right
        KingMove(curX - 1, curY + 1, ref futureMoves); // down right
    }

    private bool InCheckmate()
    {
        List<bool> moveBool = new List<bool>(confirmedMoves.Values);
        if (checkers == CheckState.inCheck)
        {
            foreach (bool check in moveBool)
            {
                if (check)
                    return false;
            }
        }
        else
            return false;
        return true;
    }

}
