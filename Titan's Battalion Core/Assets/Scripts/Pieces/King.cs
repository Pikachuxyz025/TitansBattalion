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
        if (hasMoved)
            return false;

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

        if (rook.hasMoved)
            return false;



        // if king's regular movement puts him in check castling will be unavailable

        /*foreach (ChessGen_Test player in GameManager.instance.playerList)
        {
            if (player.teamNumber.Value != team)
            {
                foreach (GameObject op in player.spawnedObject)
                {
                    Chesspiece p = op.GetComponent<Chesspiece>();

                    if (p.GetAvailableMoves().Contains(new Points(currentX + a, currentY)))
                        break;
                }
            }
        }*/
        specialMove = SpecialMove.Castling;
        //setRooks.Add(rook, new Points(currentX + a, currentY));
        return b;
    }


    public bool IsInCheck()
    {
        bool b = false;
        ChessPieceConnection conn = ChessPieceManager.instance.GetChesspieceConnection(new Points(currentX, currentY));
        if(conn.inCheck.Count > 0)
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




    public override List<Points> GetSpecialMoves()
    {
        List<Points> result = new List<Points>();

        for (int i = 0; i < specialPoints.Count; i++)
        {
            Points p = new Points(currentX + specialPoints[i].X, currentY + specialPoints[i].Y);
            if(CanCastle(p))
                result.Add(p);
        }
        return result;
    }
}
