using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SID_Pawn_Mirror : SID_Chessman_Mirror
{
    public List<Points> siD;
    public List<bool> stD;
    //when the pawn has move forward twice for one turn it's available to be taken via En Passant
    public bool duoMovement;
    public Points enPassantCoordinates;

    //[ServerCallback]
    public override void Update()
    {
        base.Update();
        if (rig < 1)
        {
            if (SID_BoardManager_Mirror.Instance.isWhiteTurn == isWhite)
                duoMovement = false;
            FindPossiblilties();
        }
        siD = new List<Points>(confirmation.Keys);
        stD = new List<bool>(confirmation.Values);
    }

    void PawnMovementStraight(int x, int y)
    {
        Points simple = new Points(x, y);
        bool r = new bool();
        PieceState pieceStateSimple = PieceManager.VaildatePieces(simple.X, simple.Y, this);

        if (pieceStateSimple == PieceState.Free)
            r = true;

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
            }
            if (!confirmation.ContainsKey(scouting[bgs]))
            {
                confirmation.Add(scouting[bgs], r);
            }
        }*/
    }

    void PawnMovementDiagonal(int x, int y, bool extraOptions)
    {
        Points simple = new Points(x, y);
        Points sideOne = new Points(CurrentX + 1, CurrentY);
        Points sideTwo = new Points(CurrentX - 1, CurrentY);
        bool r = new bool();
        PieceState pieceStateSimple = PieceManager.VaildatePieces(simple.X, simple.Y, this);
        PieceState pieceStateSideOne = PieceManager.VaildatePieces(sideOne.X, sideOne.Y, this);
        PieceState pieceStateSideTwo = PieceManager.VaildatePieces(sideTwo.X, sideTwo.Y, this);

        if (pieceStateSimple == PieceState.Enemy)
            r = true;

        if (extraOptions)
        {
            if (pieceStateSideOne == PieceState.Enemy)
            {
                if (PieceManager.FindGridPiece(sideOne).chessM.GetType() == typeof(SID_Pawn_Mirror))
                {
                    Debug.Log("showing");
                    SID_Pawn_Mirror pawn = PieceManager.FindGridPiece(sideOne).chesspiece.GetComponent<SID_Pawn_Mirror>();
                    if (pawn.duoMovement)
                        r = true;
                    enPassantCoordinates = sideOne;
                }
            }
        }
        else
        {
            if (pieceStateSideTwo == PieceState.Enemy)
            {
                if (PieceManager.FindGridPiece(sideTwo).chessM.GetType() == typeof(SID_Pawn_Mirror))
                {
                    Debug.Log("growing");
                    SID_Pawn_Mirror pawn = PieceManager.FindGridPiece(sideTwo).chesspiece.GetComponent<SID_Pawn_Mirror>();
                    if (pawn.duoMovement)
                        r = true;
                    enPassantCoordinates = sideTwo;
                }
            }
        }

        if (!confirmation.ContainsKey(simple))
            confirmation.Add(simple, r);
        else
            confirmation[simple] = r;
        /*
         foreach (SID_BoardGridSet bgs in scouting.Keys)
        {
            if (SameCoord(simple, scouting[bgs]))
            {
                if (bgs.pieceOn && isWhite != bgs.chessM.isWhite)
                {
                    r = true;
                }

                if (!confirmation.ContainsKey(scouting[bgs]))
                {
                    confirmation.Add(scouting[bgs], r);
                }
            }

            if (extraOptions)
            {
                if (SameCoord(sideOne, scouting[bgs]))
                {
                    if (bgs.pieceOn && isWhite != bgs.chessM.isWhite)
                    {
                        if (bgs.chesspiece.GetComponent<SID_Pawn_Mirror>() != null)
                        {
                            SID_Pawn_Mirror pawn = bgs.chesspiece.GetComponent<SID_Pawn_Mirror>();
                            if (pawn.duoMovement)
                                r = true;
                            enPassantCoordinates = sideOne;
                        }
                    }

                    if (!confirmation.ContainsKey(scouting[bgs]))
                    {
                        confirmation.Add(scouting[bgs], r);
                    }
                }
            }
            else if (!extraOptions)
            {
                if (SameCoord(sideTwo, scouting[bgs]))
                {
                    if (bgs.pieceOn && isWhite != bgs.chessM.isWhite)
                    {
                        if (bgs.chesspiece.GetComponent<SID_Pawn_Mirror>() != null)
                        {
                            SID_Pawn_Mirror pawn = bgs.chesspiece.GetComponent<SID_Pawn_Mirror>();
                            if (pawn.duoMovement)
                                r = true;
                            enPassantCoordinates = sideTwo;
                        }
                    }

                    if (!confirmation.ContainsKey(scouting[bgs]))
                    {
                        confirmation.Add(scouting[bgs], r);
                    }
                }
            }
        }*/
    }

    public override IEnumerator RemoveEnough()
    {
        confirmation.Clear();
        yield return new WaitForSeconds(.1f);
        if (!curOnMainBoard)
        {
            if (isWhite)
            {
                PawnMovementStraight(CurrentX, CurrentY + 1);
                PawnMovementStraight(CurrentX, CurrentY + 2);
                PawnMovementDiagonal(CurrentX - 1, CurrentY + 1, false);
                PawnMovementDiagonal(CurrentX + 1, CurrentY + 1, true);
            }
            else
            {
                PawnMovementStraight(CurrentX, CurrentY - 1);
                PawnMovementStraight(CurrentX, CurrentY - 2);
                PawnMovementDiagonal(CurrentX - 1, CurrentY - 1, true);
                PawnMovementDiagonal(CurrentX + 1, CurrentY - 1, false);
            }
        }
        else
        {
            if (isWhite)
            {
                PawnMovementStraight(CurrentX, CurrentY + 1);
                PawnMovementDiagonal(CurrentX - 1, CurrentY + 1, false);
                PawnMovementDiagonal(CurrentX + 1, CurrentY + 1, true);
            }
            else
            {
                PawnMovementStraight(CurrentX, CurrentY - 1);
                PawnMovementDiagonal(CurrentX - 1, CurrentY - 1, true);
                PawnMovementDiagonal(CurrentX + 1, CurrentY - 1, false);
            }
        }
        yield return rig += 1;
    }
}
