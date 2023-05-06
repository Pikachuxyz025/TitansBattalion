using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Chesspiece
{
    public List<Rook> rooks = new List<Rook>();
    public List<Chesspiece> targetingPieces = new List<Chesspiece>();

    bool IsCastlingPointInCheck(Points p)
    {
        return IsInCheck() || (chessManager.GetChesspieceConnection(p).IsInCheck(team));
    }

    List<Chesspiece> PiecesThatHauntMe(Points p)
    {
        List<Chesspiece> chesses = new List<Chesspiece>();
        foreach (Chesspiece piece in chessManager.GetChesspieceConnection(p).piecesThatHaveUsInCheck)
        {
            if (piece != this && piece.team != team)
                chesses.Add(piece);
        }
        return chesses;
    }

    bool IsThereAWayOut()
    {
        bool b = true;
        GetAvailableMoves();
        if (targetingPieces.Count > 0)
        {
            foreach (Chesspiece piece in targetingPieces)
            {
                if (!piece.IsInCheck())
                    b = false;
            }
        }

        return b;
    }

    public bool CompleteKingCheckmate()
    {
        bool isCheckmateOfficial = true;
        if ((GetAvailableMoves().Count > 0))
            isCheckmateOfficial = false;
        return isCheckmateOfficial && IsInCheck() && !IsThereAWayOut();
    }

    public bool SetCheckableList()
    {
        List<Chesspiece> dangerousChesspieces = new List<Chesspiece>();
        if (IsInCheck(out dangerousChesspieces))
        {
            if (dangerousChesspieces.Count > 0)

                Debug.Log("We're being targeted by " + dangerousChesspieces.Count);
            playersCheckableList.AddKingTargets(dangerousChesspieces);

        }
        else
            playersCheckableList.ResetKingTargets();

        return IsInCheck();
    }

    public override List<Points> GetAvailableMoves()
    {
        List<Points> newMoves = new List<Points>();
        targetingPieces.Clear();
        switch (type)
        {
            case SetType.Manual:

                for (int i = 0; i < addedPoints.Count; i++)
                {
                    Points accessablePoint = new Points(currentX + addedPoints[i].X, currentY + addedPoints[i].Y);

                    if (!ChessboardTileManager.instance.IsCoordinateInList(accessablePoint))
                        continue;
                    if (chessManager.IsOccupied(accessablePoint))
                        if (chessManager.GetOccupiedPiece(accessablePoint).team == team)
                            continue;
                    if (CurrentPieceCheck(accessablePoint))
                    {
                        if (PiecesThatHauntMe(accessablePoint).Count != 0)
                        {
                            targetingPieces.AddRange(PiecesThatHauntMe(accessablePoint));
                            continue;
                        }
                    }
                    AddInCheck(accessablePoint);
                    newMoves.Add(accessablePoint);
                }
                break;
            case SetType.Open:
                if (allStraight)
                {
                    // UP
                    for (int i = currentY + 1; i > currentY; i++)
                    {
                        Points p = new Points(currentX, i);
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                        {
                            if (chessManager.GetOccupiedPiece(p).team != team)
                            {
                                AddInCheck(p);
                                newMoves.Add(p);
                            }
                            break;
                        }
                        AddInCheck(p);
                        newMoves.Add(p);
                    }

                    //Down
                    for (int i = currentY - 1; i < currentY; i--)
                    {
                        Points p = new Points(currentX, i);
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                        {
                            if (chessManager.GetOccupiedPiece(p).team != team)
                            {
                                AddInCheck(p);
                                newMoves.Add(p);
                            }
                            break;
                        }
                        AddInCheck(p);
                        newMoves.Add(p);
                    }

                    //Left
                    for (int i = currentX - 1; i < currentX; i--)
                    {
                        Points p = new Points(i, currentY);
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                        {
                            if (chessManager.GetOccupiedPiece(p).team != team)
                            {
                                AddInCheck(p);
                                newMoves.Add(p);
                            }
                            break;
                        }
                        AddInCheck(p);
                        newMoves.Add(p);
                    }

                    //Right
                    for (int i = currentX + 1; i > currentX; i++)
                    {
                        Points p = new Points(i, currentY);
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                        {
                            if (chessManager.GetOccupiedPiece(p).team != team)
                            {
                                AddInCheck(p);
                                newMoves.Add(p);
                            }
                            break;
                        }
                        AddInCheck(p);
                        newMoves.Add(p);
                    }
                }
                if (allDiagonal)
                {

                    for (int x = currentX + 1, y = currentY + 1; x > currentX && y > currentY; x++, y++)
                    {
                        Points p = new Points(x, y);
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                        {
                            if (chessManager.GetOccupiedPiece(p).team != team)
                            {
                                AddInCheck(p);
                                newMoves.Add(p);
                            }
                            break;
                        }
                        AddInCheck(p);
                        newMoves.Add(p);
                    }

                    for (int x = currentX - 1, y = currentY + 1; x < currentX && y > currentY; x--, y++)
                    {
                        Points p = new Points(x, y);
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                        {
                            if (chessManager.GetOccupiedPiece(p).team != team)
                            {
                                AddInCheck(p);
                                newMoves.Add(p);
                            }
                            break;
                        }
                        AddInCheck(p);
                        newMoves.Add(p);
                    }

                    for (int x = currentX - 1, y = currentY - 1; x < currentX && y < currentY; x--, y--)
                    {
                        Points p = new Points(x, y);
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                        {
                            if (chessManager.GetOccupiedPiece(p).team != team)
                            {
                                AddInCheck(p);
                                newMoves.Add(p);
                            }
                            break;
                        }
                        AddInCheck(p);
                        newMoves.Add(p);
                    }

                    for (int x = currentX + 1, y = currentY - 1; x > currentX && y < currentY; x++, y--)
                    {
                        Points p = new Points(x, y);

                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                        {
                            if (chessManager.GetOccupiedPiece(p).team != team)
                            {
                                AddInCheck(p);
                                newMoves.Add(p);
                            }
                            break;
                        }
                        AddInCheck(p);
                        newMoves.Add(p);

                    }
                }
                break;
        }
        return newMoves;
    }


    public override List<Points> GetSpecialMoves()
    {
        List<Points> result = new List<Points>();
        ChessboardTile specialPiece = new ChessboardTile();
        if (currentGameMode.Value == GameMode.Chess && hasMoved)
            return result;

        foreach (Rook rook in rooks)
        {
            KingDirection currentLookDirection = new KingDirection();
            if (rook.InRangeCheckX(out currentLookDirection))
            {
                // if true is the rook on my left or on my right?
                switch (currentLookDirection)
                {
                    case KingDirection.Left:
                        specialPiece = SpecialChesspiece(0, true, rook, -1);
                        if (!specialPiece)
                            break;
                        Debug.Log("Found Left Piece");
                        result.Add(specialPiece.CurrentTilePoint());

                        specialMove = SpecialMove.Castling;
                        break;

                    case KingDirection.Right:
                        specialPiece = SpecialChesspiece(1, true, rook, 1);
                        if (!specialPiece)
                            break;
                        Debug.Log("Found Right Piece");
                        result.Add(specialPiece.CurrentTilePoint());
                        specialMove = SpecialMove.Castling;
                        break;
                }
            }

            if (rook.InRangeCheckY(out currentLookDirection) && currentGameMode.Value != GameMode.Chess)
            {
                // if true is the rook above me or below me?
                switch (currentLookDirection)
                {
                    case KingDirection.Up:
                        specialPiece = SpecialChesspiece(3, false, rook, 1);
                        if (!specialPiece)
                            break;
                        result.Add(specialPiece.CurrentTilePoint());
                        specialMove = SpecialMove.Castling;
                        break;

                    case KingDirection.Down:
                        specialPiece = SpecialChesspiece(2, false, rook, -1);
                        if (!specialPiece)
                            break;
                        result.Add(specialPiece.CurrentTilePoint());
                        specialMove = SpecialMove.Castling;
                        break;
                }
            }
        }

        return result;
    }


    /// <summary>
    /// ----------------ChessPieceConnection SpecialChesspiece----------------
    /// The SpecialChesspiece method allows me to get the tile used for castling without using 
    /// the same function over and over again in GetSpecialMoves.
    /// Multiple local booleans and ints where created to account for the various directions the for loops 
    /// go through to see if any of the tiles are occupied
    /// </summary>
    /// <param name="specialPointIndex"> This index is dependent on the list of specialPoints. </param>
    /// <param name="XIsTrueYIsFalse"> Determines if currentX or currentY is used for calculation. </param>
    /// <param name="currentRook"> The rook we're castling with</param>
    /// <param name="kingOffset"> Determines if we're position or negative for increments. </param>
    /// <returns> Should return a ChessPieceConnection if the route isn't covered and castlingPoint isn't in check. </returns>
    private ChessboardTile SpecialChesspiece(int specialPointIndex, bool XIsTrueYIsFalse, Rook currentRook, int kingOffset)
    {
        bool isRouteCovered = false;
        bool isPositiveOrNegative = kingOffset > 0 ? true : false;
        int combinedOffset = XIsTrueYIsFalse ? currentX + kingOffset : currentY + kingOffset;
        Points moveablePosition = XIsTrueYIsFalse ? new Points(combinedOffset, currentY) : new Points(currentX, combinedOffset);
        int rookCurrentIntXY = XIsTrueYIsFalse ? currentRook.currentX : currentRook.currentY;

        if (IsCastlingPointInCheck(moveablePosition))
            return null;

        // Make sure there's nothing in between us
        for (int i = combinedOffset; isPositiveOrNegative ? i < rookCurrentIntXY : i > rookCurrentIntXY; i += kingOffset)
        {
            Points newPoint = XIsTrueYIsFalse ? new Points(i, currentY) : new Points(currentX, i);
            if (chessManager.IsOccupied(newPoint))
            {
                isRouteCovered = true;
                break;
            }
        }

        if (isRouteCovered) return null;

        Points specialNewMove = new Points(currentX + specialPoints[specialPointIndex].X, currentY + specialPoints[specialPointIndex].Y);
        if (IsCastlingPointInCheck(specialNewMove))
            return null;
        return chessManager.GetChesspieceConnection(specialNewMove);
    }
}
