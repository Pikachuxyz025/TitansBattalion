using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Chesspiece
{
public List<Rook> rooks=new List<Rook>();
    public Dictionary<Rook,Points> setRooks=new Dictionary<Rook,Points>();


    bool CanCastle(Points pointer)
    {
        bool b = true;
        int a = 0;
        bool isleft = false;
        Rook rook = null;

        //if either rook or king has moved castling will be unavailable
       // if (hasMoved)
            //return false;

        if(rooks.Count == 0)
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




    public bool IsInCheck()
    {
        bool b = false;
        if (chessManager == null)
        {
            Debug.Log("faulty");
            return b;
        }
        ChessPieceConnection conn = chessManager.GetChesspieceConnection(new Points(currentX, currentY));
        if (conn == null)
            return b;
        if (conn.inCheck.Count > 0)
        {
            for (int i = 0; i < conn.inCheck.Count; i++)
            {
                if (conn.inCheck[i] == team)
                    continue;
                else
                    b = true;
            }
        }

        return b;
    }


    bool CastlingCheck(Points p)
    {
        return IsInCheck() || (chessManager.GetChesspieceConnection(p).inCheck.Count > 1);
    }

    bool KingCheck(Points p)
    {
        return (chessManager.GetChesspieceConnection(p).inCheck.Count > 1);
    }

    public bool CompleteKingCheckmate()
    {
        bool isCheckmateOfficial = true;
        foreach (Points moves in GetAvailableMoves())
        {
            if(!KingCheck(moves))
                isCheckmateOfficial = false;
        }
        return isCheckmateOfficial && IsInCheck();
    } 


    public override List<Points> GetSpecialMoves()
    {
        List<Points> result = new List<Points>();

        /*for (int i = 0; i < specialPoints.Count; i++)
        {
            Points p = new Points(currentX + specialPoints[i].X, currentY + specialPoints[i].Y);
            if (CanCastle(p))
                result.Add(p);
        }*/

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

                        // Make sure there's nothing in between us
                        for (int i = currentX - 1; i > rook.currentX; i--)
                        {
                            Points p = new Points(i, currentY);
                            if (chessManager.IsOccupied(p))
                            {
                                b = true;
                                break;
                            }
                            Debug.Log("Left "+p.X + " and" + p.Y + " are not breaking");
                        }
                        if (b) break;
                        specialMove = SpecialMove.Castling;
                        result.Add(new Points(currentX + specialPoints[0].X, currentY + specialPoints[0].Y));
                        break;

                    case KingDirection.Right:
                        a = 1;
                        b = false;
                        s = new Points(currentX + a, currentY);

                        if (CastlingCheck(s))
                            break;

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
                        specialMove = SpecialMove.Castling;
                        result.Add(new Points(currentX + specialPoints[1].X, currentY + specialPoints[1].Y));
                        break;
                }
            }

            if (rook.InRangeCheckY(out set))
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
                        specialMove = SpecialMove.Castling;
                        result.Add(new Points(currentX + specialPoints[3].X, currentY + specialPoints[3].Y));
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
                        specialMove = SpecialMove.Castling;
                        result.Add(new Points(currentX + specialPoints[2].X, currentY + specialPoints[2].Y));
                        break;
                }
            }
        }

        return result;
    }
}
