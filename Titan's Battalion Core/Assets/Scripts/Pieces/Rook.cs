using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum KingDirection
{
    Up,
    Down,
    Left,
    Right,
    None,
}

public class Rook : Chesspiece
{
    public King myKing;

    public void SetKing(King kingly)
    {
        myKing = kingly;
    }

    public bool InRangeCheckX(out KingDirection setup)
    {
        setup = KingDirection.None;
        if (myKing == null)
            return false;
        if (myKing.currentY != currentY)
            return false;
        if (currentGameMode.Value == GameMode.Chess && hasMoved)
            return false;

        Debug.Log(CheckCoordinateDistance(myKing.currentX, currentX));

        if (CheckCoordinateDistance(myKing.currentX, currentX) > 2 && CheckCoordinateDistance(myKing.currentX, currentX) <= 4)
        {
            // the rook is to the left of the king
            setup = KingDirection.Left;
            return true;
        }
        if (CheckCoordinateDistance(myKing.currentX, currentX) < -2 && CheckCoordinateDistance(myKing.currentX, currentX) >= -4)
        {
            // the rook is to the right of the king
            setup = KingDirection.Right;
            return true;
        }
        return false;
    }

    public bool InRangeCheckY(out KingDirection setup)
    {
        setup = KingDirection.None;
        if (myKing == null)
            return false;

        if (myKing.currentX != currentX)
            return false;

        if (CheckCoordinateDistance(myKing.currentY, currentY) > 2 && CheckCoordinateDistance(myKing.currentY, currentY) <= 4)
        {
            // the rook is below the king
            setup = KingDirection.Down;
            return true;
        }
        if (CheckCoordinateDistance(myKing.currentY, currentY) < -2 && CheckCoordinateDistance(myKing.currentY, currentY) >= -4)
        {
            // the rook is above the king
            setup = KingDirection.Up;
            return true;
        }
        return false;
    }
    
    int CheckCoordinateDistance(int X1,int X2)
    {
        return X1 - X2;
    }

}
