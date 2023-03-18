using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Chesspiece
{
    public List<Rook> rooks = new List<Rook>();
    public List<Chesspiece> targetingPieces = new List<Chesspiece>();

    bool CanCastle(Points pointer)
    {
        bool b = true;
        int a = 0;
        bool isleft = false;
        Rook rook = null;

        //if either rook or king has moved castling will be unavailable
        // if (hasMoved)
        //return false;

        if (rooks.Count == 0)
            return false;







        // account for multiple players based on team number
        // if Pointer.X is negative go with rook on right

        switch (team)
        {
            case 2:
                ChessPieceManager manager = ChessPieceManager.instance;
                if (pointer.X > currentX)
                {
                    a = 1;
                    isleft = false;
                    foreach (Rook rookie in rooks)
                    {
                        if (rookie.currentX > currentX)
                            rook = rookie;
                    }
                }
                // if Pointer.X is negative go with rook on the left
                if (pointer.X < currentX)
                {
                    a = -1;
                    isleft = true;
                    foreach (Rook rookie in rooks)
                    {
                        if (rookie.currentX < currentX)
                            rook = rookie;
                    }
                }
                //if anything is in between the rook or the king the move will be unavailable
                if (isleft)
                {
                    for (int i = currentX - 1; i > rook.currentX; i--)
                    {
                        Points p = new Points(i, currentY);
                        if (manager.IsOccupied(p))
                            return false;
                    }
                }
                else
                {
                    for (int i = currentX + 1; i < rook.currentX; i++)
                    {
                        Points p = new Points(i, currentY);
                        if (manager.IsOccupied(p))
                            return false;
                    }
                }

                if (manager.GetChesspieceConnection(new Points(currentX + a, currentY)).inCheck.Count > 1)
                    return false;
                break;
            case 1:
                manager = ChessPieceManager.instance;
                if (pointer.X < currentX)
                {
                    a = 1;
                    isleft = false;
                    foreach (Rook rookie in rooks)
                    {
                        if (rookie.currentX < currentX)
                            rook = rookie;
                    }
                }
                // if Pointer.X is negative go with rook on the left
                if (pointer.X > currentX)
                {
                    a = -1;
                    isleft = true;
                    foreach (Rook rookie in rooks)
                    {
                        if (rookie.currentX > currentX)
                            rook = rookie;
                    }
                }
                //if anything is in between the rook or the king the move will be unavailable
                if (isleft)
                {
                    for (int i = currentX + 1; i < rook.currentX; i++)
                    {
                        Points p = new Points(i, currentY);
                        if (manager.IsOccupied(p))
                            return false;
                    }
                }
                else
                {
                    for (int i = currentX - 1; i > rook.currentX; i--)
                    {
                        Points p = new Points(i, currentY);
                        if (manager.IsOccupied(p))
                            return false;
                    }
                }

                if (manager.GetChesspieceConnection(new Points(currentX + a, currentY)).inCheck.Count > 1)
                    return false;
                break;
            case 3:
                manager = ChessPieceManager.instance;
                break;
            case 4:
                manager = ChessPieceManager.instance;
                break;
        }

        if (rook == null)
            return false;


        specialMove = SpecialMove.Castling;
        //setRooks.Add(rook, new Points(currentX + a, currentY));
        return b;
    }


    bool CastlingCheck(Points p)
    {
        return IsInCheck() || (chessManager.GetChesspieceConnection(p).IsInCheck(team));
    }

    List<Chesspiece> PiecesThatHauntMe(Points p)
    {
        List<Chesspiece> chesses = new List<Chesspiece>();
        foreach (Chesspiece piece in chessManager.GetChesspieceConnection(p).inCheck)
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



    public override List<Points> GetAvailableMoves()
    {
        List<Points> newMoves = new List<Points>();
        targetingPieces.Clear();
        switch (type)
        {
            case SetType.Manual:

                for (int i = 0; i < addedPoints.Count; i++)
                {
                    Points c = new Points(currentX + addedPoints[i].X, currentY + addedPoints[i].Y);

                    if (!ChessPieceManager.instance.IsCoordinateInList(c))
                        continue;
                    if (chessManager.IsOccupied(c))
                        if (chessManager.GetOccupiedPiece(c).team == team)
                            continue;
                    if (CurrentPieceCheck(c))
                    {
                        if (PiecesThatHauntMe(c).Count != 0)
                        {
                            targetingPieces.AddRange(PiecesThatHauntMe(c));
                            continue;
                        }
                    }
                    AddInCheck(c);
                    newMoves.Add(c);
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

        if (currentGameMode.Value == GameMode.Chess && hasMoved)
            return result;

        foreach (Rook rook in rooks)
        {
            KingDirection set = new KingDirection();
            if (rook.InRangeCheckX(out set))
            {
                int a = 0;
                bool b = false;
                // if true is the rook on my left or on my right?
                switch (set)
                {
                    case KingDirection.Left:

                        a = -1;
                        b = false;
                        Points s = new Points(currentX + a, currentY);

                        if (CastlingCheck(s))
                            break;
                        Debug.Log("Yo Left");
                        // Make sure there's nothing in between us
                        for (int i = currentX - 1; i > rook.currentX; i--)
                        {
                            Points p = new Points(i, currentY);
                            if (chessManager.IsOccupied(p))
                            {
                                b = true;
                                break;
                            }
                            Debug.Log("Left " + p.X + " and" + p.Y + " are not breaking");
                        }
                        if (b) break;

                        Points c = new Points(currentX + specialPoints[0].X, currentY + specialPoints[0].Y);
                        if (CastlingCheck(c))
                            break;
                        result.Add(c);

                        specialMove = SpecialMove.Castling;
                        break;

                    case KingDirection.Right:

                        a = 1;
                        b = false;
                        s = new Points(currentX + a, currentY);

                        if (CastlingCheck(s))
                            break;
                        Debug.Log("Yo Right");
                        // Make sure there's nothing in between us
                        for (int i = currentX + 1; i < rook.currentX; i++)
                        {
                            Points p = new Points(i, currentY);
                            if (chessManager.IsOccupied(p))
                            {
                                b = true;
                                break;
                            }
                        }
                        if (b) break;

                        c = new Points(currentX + specialPoints[1].X, currentY + specialPoints[1].Y);
                        if (CastlingCheck(c))
                            break;
                        result.Add(c);

                        specialMove = SpecialMove.Castling;
                        break;
                }
            }

            if (rook.InRangeCheckY(out set) && currentGameMode.Value != GameMode.Chess)
            {
                int a = 0;
                bool b = false;
                // if true is the rook above me or below me?
                switch (set)
                {
                    case KingDirection.Up:
                        a = 1;
                        b = false;
                        Points s = new Points(currentX + a, currentY);

                        if (CastlingCheck(s))
                            break;

                        // Make sure there's nothing in between us
                        for (int i = currentY + 1; i < rook.currentY; i++)
                        {
                            Points p = new Points(currentX, i);
                            if (chessManager.IsOccupied(p))
                            {
                                b = true;
                                break;
                            }
                        }
                        if (b) break;

                        Points c = new Points(currentX + specialPoints[3].X, currentY + specialPoints[3].Y);
                        if (CastlingCheck(c))
                            break;
                        result.Add(c);
                        specialMove = SpecialMove.Castling;


                        break;

                    case KingDirection.Down:
                        a = -1;
                        b = false;
                        s = new Points(currentX + a, currentY);

                        if (CastlingCheck(s))
                            break;

                        // Make sure there's nothing in between us
                        for (int i = currentY - 1; i > rook.currentY; i--)
                        {
                            Points p = new Points(currentX, i);
                            if (chessManager.IsOccupied(p))
                            {
                                b = true;
                                break;
                            }
                        }
                        if (b) break;

                        c = new Points(currentX + specialPoints[2].X, currentY + specialPoints[2].Y);
                        if (CastlingCheck(c))
                            break;
                        result.Add(c);

                        specialMove = SpecialMove.Castling;
                        break;
                }
            }
        }

        return result;
    }
}
