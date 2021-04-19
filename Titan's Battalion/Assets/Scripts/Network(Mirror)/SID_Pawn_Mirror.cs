using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using System.Linq;


public class SID_Pawn_Mirror : SID_Chessman_Mirror
{
    public List<Points> siD;
    //public List<bool> stD;
    public List<Points> seD;
    public List<string> stG = new List<string>();
    //when the pawn has move forward twice for one turn it's available to be taken via En Passant
    public bool duoMovement;
    public Points enPassantCoordinates;

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
            /*if (SID_BoardManager_Mirror.Instance.isWhiteTurn == isWhite)
                duoMovement = false;*/
            FindPossiblilties();
        }

        
        siD = new List<Points>(confirmedMoves.Keys);
        //stD = new List<bool>(confirmedMoves.Values);
        seD = new List<Points>(futureMoves.Keys);

        //PieceManager.VaildatePieces(futureTies[new Points(2, 1)][3].X, futureTies[new Points(2, 1)][3].Y, this);
    }



    void PawnMovementStraight(int x, int y, ref Dictionary<Points, bool> confirmation)
    {
        Points simple = new Points(x, y);
        bool r = new bool();
        PieceState pieceStateSimple = PieceManager.VaildatePieces(simple.X, simple.Y, this);

        if (pieceStateSimple == PieceState.Free)
            r = true;

        if (!confirmation.ContainsKey(simple))
            confirmation.Add(simple, r);
    }

    List<Points> PawnMovementStraights(int x, int y, ref Dictionary<Points, bool> confirmation)
    {
        Points simple = new Points(x, y);
        bool r = new bool();
        List<Points> controlled = new List<Points>();
        PieceState pieceStateSimple = PieceManager.VaildatePieces(simple.X, simple.Y, this);

        if (pieceStateSimple == PieceState.Free)
            r = true;

        if (!confirmation.ContainsKey(simple))
            confirmation.Add(simple, r);
        if ((!controlled.Contains(simple)))
            controlled.Add(simple);


        return controlled;
    }

    void PawnMovementDiagonal(int x, int y, bool extraOptions, ref Dictionary<Points, bool> confirmation)
    {
        Points simple = new Points(x, y);
        Points sideOne = new Points(CurrentX + 1, CurrentY);
        Points sideTwo = new Points(CurrentX - 1, CurrentY);
        bool r = new bool();
        PieceState pieceStateSimple = PieceManager.VaildatePieces(simple.X, simple.Y, this);
        PieceState pieceStateSideOne = PieceManager.VaildatePieces(sideOne.X, sideOne.Y, this);
        PieceState pieceStateSideTwo = PieceManager.VaildatePieces(sideTwo.X, sideTwo.Y, this);

        if (pieceStateSimple == PieceState.Enemy)
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
    }

    List<Points> PawnMovementDiagonals(int x, int y, bool extraOptions, ref Dictionary<Points, bool> confirmation)
    {
        Points simple = new Points(x, y);
        Points sideOne = new Points(CurrentX + 1, CurrentY);
        Points sideTwo = new Points(CurrentX - 1, CurrentY);
        bool r = new bool();
        List<Points> controlled = new List<Points>();
        PieceState pieceStateSimple = PieceManager.VaildatePieces(simple.X, simple.Y, this);
        PieceState pieceStateSideOne = PieceManager.VaildatePieces(sideOne.X, sideOne.Y, this);
        PieceState pieceStateSideTwo = PieceManager.VaildatePieces(sideTwo.X, sideTwo.Y, this);

        if (pieceStateSimple == PieceState.Enemy)
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

        if (!controlled.Contains(simple))
            controlled.Add(simple);
        return controlled;
    }

    public override void CalculateFutureMoves()
    {
        futureMoves.Clear();
        futureTies.Clear();
        foreach (Points coord in new List<Points>(confirmedMoves.Keys))
        {
            if (confirmedMoves[coord])
            {
                if (!stG.Contains(coord.X + ", " + coord.Y))
                    stG.Add(coord.X + ", " + coord.Y);
                StartCoroutine(FutureSight(coord.X, coord.Y));
            }
        }
    }

    public override IEnumerator CalculateCurrentMoves()
    {
        confirmedMoves.Clear();
        yield return new WaitForSeconds(.1f);
        if (!curOnMainBoard)
        {
            if (isWhite)
            {
                PawnMovementStraight(CurrentX, CurrentY + 1, ref confirmedMoves);
                PawnMovementStraight(CurrentX, CurrentY + 2, ref confirmedMoves);
                PawnMovementDiagonal(CurrentX - 1, CurrentY + 1, false, ref confirmedMoves);
                PawnMovementDiagonal(CurrentX + 1, CurrentY + 1, true, ref confirmedMoves);
            }
            else
            {
                PawnMovementStraight(CurrentX, CurrentY - 1, ref confirmedMoves);
                PawnMovementStraight(CurrentX, CurrentY - 2, ref confirmedMoves);
                PawnMovementDiagonal(CurrentX - 1, CurrentY - 1, true, ref confirmedMoves);
                PawnMovementDiagonal(CurrentX + 1, CurrentY - 1, false, ref confirmedMoves);
            }
        }
        else
        {
            if (isWhite)
            {
                PawnMovementStraight(CurrentX, CurrentY + 1, ref confirmedMoves);
                PawnMovementDiagonal(CurrentX - 1, CurrentY + 1, false, ref confirmedMoves);
                PawnMovementDiagonal(CurrentX + 1, CurrentY + 1, true, ref confirmedMoves);
            }
            else
            {
                PawnMovementStraight(CurrentX, CurrentY - 1, ref confirmedMoves);
                PawnMovementDiagonal(CurrentX - 1, CurrentY - 1, true, ref confirmedMoves);
                PawnMovementDiagonal(CurrentX + 1, CurrentY - 1, false, ref confirmedMoves);
            }
        }
        CalculateFutureMoves();
        yield return rig += 1;
    }

    public override IEnumerator FutureSight(int curX, int curY)
    {
        yield return new WaitForSeconds(.1f);
        if (!PieceManager.IsOnMainBoard(curX, curY))
        {
            if (isWhite)
            {
                /*pointers.Union(*/PawnMovementStraight(curX, curY + 1, ref futureMoves);
                /*pointers.Union(*/PawnMovementStraight(curX, curY + 2, ref futureMoves);
                /*pointers.Union(*/PawnMovementDiagonal(curX - 1, curY + 1, false, ref futureMoves);
                /*pointers.Union(*/PawnMovementDiagonal(curX + 1, curY + 1, true, ref futureMoves);
            }
            else
            {
                /*pointers.Union(*/PawnMovementStraight(curX, curY - 1, ref futureMoves);
                /*pointers.Union(*/PawnMovementStraight(curX, curY - 2, ref futureMoves);
                /*pointers.Union(*/PawnMovementDiagonal(curX - 1, curY - 1, true, ref futureMoves);
                /*pointers.Union(*/PawnMovementDiagonal(curX + 1, curY - 1, false, ref futureMoves);
            }
        }
        else
        {
            if (isWhite)
            {
                /*pointers.Union(*/PawnMovementStraight(curX, curY + 1, ref futureMoves);
                /*pointers.Union(*/PawnMovementDiagonal(curX - 1, curY + 1, false, ref futureMoves);
                /*pointers.Union(*/PawnMovementDiagonal(curX + 1, curY + 1, true, ref futureMoves);
            }
            else
            {
                /*pointers.Union(*/PawnMovementStraight(curX, curY - 1, ref futureMoves);
                /*pointers.Union(*/PawnMovementDiagonal(curX - 1, curY - 1, true, ref futureMoves);
                /*pointers.Union(*/PawnMovementDiagonal(curX + 1, curY - 1, false, ref futureMoves);
            }
        }
        /*if (!futureTies.ContainsKey(doom))
            futureTies.Add(doom, pointers);*/
    }
}
