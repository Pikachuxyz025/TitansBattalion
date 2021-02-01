using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SID_King_Mirror : SID_Chessman_Mirror
{
    public List<Points> siD;
    public bool hasMoved = false;

    private SID_Rook_Mirror leftRook = null, rightRook = null;

    public override void Update()
    {
        base.Update();
        if (rig < 1)
        {
            AdditionalPossibilities();
            FindPossiblilties();
        }
        siD = new List<Points>(confirmation.Keys);
    }

    void AdditionalPossibilities()
    {
        // Right
        rightRook = GetRook(3, 1);

        // Left
        leftRook = GetRook(4, -1);
    }

    public void KingMove(int x, int y)
    {
        bool r = new bool();
        Points simple = new Points(x, y);
        PieceState pieceStateSimple = PieceManager.VaildatePieces(simple.X, simple.Y, this);

        if (pieceStateSimple == PieceState.Free)
            r = true;

        if (!confirmation.ContainsKey(simple))
            confirmation.Add(simple, r);
        /*foreach (SID_BoardGridSet bgs in scouting.Keys)
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
        }*/
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
        if (!confirmation.ContainsKey(rook.mCastlePoints))
            confirmation.Add(rook.mCastlePoints, !rook.hasMoved);

        return rook;
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
