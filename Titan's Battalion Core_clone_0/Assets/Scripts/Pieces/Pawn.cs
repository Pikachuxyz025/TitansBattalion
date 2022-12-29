using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;



public class Pawn : Chesspiece
{
    public List<Points> moveList = new List<Points>();
    public Dictionary<Points, Pawn> pawns = new Dictionary<Points, Pawn>(new Points.EqualityComparer());
    [SerializeField] protected GameObject convertableQueen;

    public List<Points> takeoverPoints = new List<Points>();


    public Dictionary<Pawn, int> Which()
    {

        Dictionary<Pawn, int> crow = new Dictionary<Pawn, int>();
        ChessPieceManager chessManager = ChessPieceManager.instance;


        Points[] checkPoints = new Points[4];
        checkPoints[0] = new Points(currentX + 0, currentY + 1);
        checkPoints[1] = new Points(currentX + 0, currentY + -1);
        checkPoints[2] = new Points(currentX + -1, currentY + 0);
        checkPoints[3] = new Points(currentX + 1, currentY + 0);

        for (int i = 0; i < checkPoints.Length; i++)
        {
            Points point = checkPoints[i];
            if (!chessManager.IsCoordinateInList(point))
                continue;

            if (!chessManager.IsOccupied(point))
                continue;


            Chesspiece piece = chessManager.GetOccupiedPiece(point);

            if (piece is Pawn && piece.team != team)
            {
                Pawn enemyPawn = piece.gameObject.GetComponent<Pawn>();
                if (enemyPawn.moveList.Count > 0)
                {
                    crow.Add(enemyPawn, i + 1);
                }
            }
        }

        return crow;
    }

    public void GetPointOutput(out int x, out int y)
    {
        x = 0;
        y = 0;
        if (moveList.Count > 1)
        {
            x = moveList[moveList.Count - 1].X - moveList[moveList.Count - 2].X;
            y = moveList[moveList.Count - 1].Y - moveList[moveList.Count - 2].Y;
        }
    }



    public void AddToMoveList()
    {
        Points p = new Points(currentX, currentY);
        moveList.Add(p);
    }

    public override List<Points> GetSpecialMoves()
    {
        List<Points> result = new List<Points>();
        Dictionary<Pawn, int> crow = Which();
        ChessPieceManager chessManager = ChessPieceManager.instance;
        foreach (Pawn item in crow.Keys)
        {
            int x = 0;
            int y = 0;
            item.GetPointOutput(out x, out y);

            if (Mathf.Abs(x) < 2 && Mathf.Abs(y) < 2)
                break;

            switch (crow[item])
            {
                case 1:
                    result.Add(PrintOut(x, 0, 1));
                    if (!pawns.ContainsKey(PrintOut(x, 0, 1)))
                        pawns.Add(PrintOut(x, 0, 1), item);
                    break;
                case 2:
                    result.Add(PrintOut(x, 2, 3));
                    if (!pawns.ContainsKey(PrintOut(x, 2, 3)))
                        pawns.Add(PrintOut(x, 2, 3), item);
                    break;
                case 3:
                    result.Add(PrintOut(y, 2, 0));
                    if (!pawns.ContainsKey(PrintOut(y, 2, 0)))
                        pawns.Add(PrintOut(y, 2, 0), item);
                    break;
                case 4:
                    result.Add(PrintOut(y, 3, 1));
                    if (!pawns.ContainsKey(PrintOut(y, 3, 1)))
                        pawns.Add(PrintOut(y, 3, 1), item);
                    break;
            }
        }
        specialMove = SpecialMove.EnPassant;
        return result;
    }

    public bool CanCovertToQueen()
    {
        ChessPieceConnection currentConn = chessManager.GetChesspieceConnection(new Points(currentX, currentY));
        if (currentConn.spawnTerritoryId.Value == team)
            return false;
        Points c;

        switch (currentConn.spawnTerritoryId.Value)
        {
            case 1:
                c = new Points(currentX + addedPoints[0].X, currentY + addedPoints[0].Y);
                if (!chessManager.IsCoordinateInList(c))
                {
                    Debug.Log("1: " + c.X + ", " + c.Y);
                    return true;
                }
                break;
            case 2:
                c = new Points(currentX + addedPoints[1].X, currentY + addedPoints[1].Y);
                if (!chessManager.IsCoordinateInList(c))
                {
                    Debug.Log("2: " + c.X + ", " + c.Y);
                    return true;
                }
                break;
            case 3:
                c = new Points(currentX + addedPoints[2].X, currentY + addedPoints[2].Y);
                if (!chessManager.IsCoordinateInList(c))
                {
                    Debug.Log("3: " + c.X + ", " + c.Y);
                    return true;
                }
                break;
            case 4:
                c = new Points(currentX + addedPoints[3].X, currentY + addedPoints[3].Y);
                if (!chessManager.IsCoordinateInList(c))
                {
                    Debug.Log("4: " + c.X + ", " + c.Y);
                    return true;
                }
                break;
        }
        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ConvertToQueenServerRpc()
    {
        if (CanCovertToQueen())
        {
            GameObject q = Instantiate(convertableQueen);
            q.GetComponent<NetworkObject>().Spawn();

            Chesspiece cp = q.GetComponent<Chesspiece>();
            ChessPieceManager chessManager = ChessPieceManager.instance;
            ChessPieceConnection cc = chessManager.GetChesspieceConnection(new Points(currentX, currentY));

            q.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);
            cc.SetOccupiedPiece(cp);
            chessManager.PositionSinglePiece(cp, cc);

            Destroy(this.gameObject);
        }
    }


    Points PrintOut(int s, int sp, int ap)
    {
        ChessPieceManager chessManager = ChessPieceManager.instance;
        Points c = new Points(0, 0);
        if (s == 2)
        {
            c = new Points(currentX + specialPoints[sp].X, currentY + specialPoints[sp].Y);
            if (chessManager.IsCoordinateInList(c))
                return c;
        }
        else if (s == -2)
        {
            c = new Points(currentX + specialPoints[ap].X, currentY + specialPoints[ap].Y);
            if (chessManager.IsCoordinateInList(c))
                return c;
        }
        return c;
    }

    public override List<Points> GetAvailableMoves()
    {
        if (!hasMoved)
        {
            List<Points> newMoves = new List<Points>();

            Points[] points = new Points[2];
            points[0] = new Points(0, 0);
            points[1] = new Points(0, 0);

            switch (team)
            {
                case 1:
                    points[0] = new Points(currentX + firstMovePoints[0].X, currentY + firstMovePoints[0].Y);
                    points[1] = new Points(currentX + firstMovePoints[1].X, currentY + firstMovePoints[1].Y);

                    foreach (Points p in points)
                    {
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                            break;

                        newMoves.Add(p);
                    }
                    break;
                case 2:
                    points[0] = new Points(currentX + firstMovePoints[2].X, currentY + firstMovePoints[2].Y);
                    points[1] = new Points(currentX + firstMovePoints[3].X, currentY + firstMovePoints[3].Y);

                    foreach (Points p in points)
                    {
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                            break;

                        newMoves.Add(p);
                    }
                    break;
                case 3:
                    points[0] = new Points(currentX + firstMovePoints[4].X, currentY + firstMovePoints[4].Y);
                    points[1] = new Points(currentX + firstMovePoints[5].X, currentY + firstMovePoints[5].Y);

                    foreach (Points p in points)
                    {
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                            break;

                        newMoves.Add(p);
                    }
                    break;
                case 4:
                    points[0] = new Points(currentX + firstMovePoints[6].X, currentY + firstMovePoints[6].Y);
                    points[1] = new Points(currentX + firstMovePoints[7].X, currentY + firstMovePoints[7].Y);

                    foreach (Points p in points)
                    {
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                            break;

                        newMoves.Add(p);
                    }
                    break;
            }

            Points[] pointsR = new Points[2];
            switch (team)
            {
                case 1:
                    pointsR[0] = new Points(currentX + takeoverPoints[0].X, currentY + takeoverPoints[0].Y);
                    pointsR[1] = new Points(currentX + takeoverPoints[1].X, currentY + takeoverPoints[1].Y);
                    foreach (Points p in pointsR)
                    {
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                        {
                            if (chessManager.GetOccupiedPiece(p).team != team)
                            {
                                AddInCheck(p);
                                newMoves.Add(p);
                            }
                        }
                    }
                    break;

                case 2:
                    pointsR[0] = new Points(currentX + takeoverPoints[2].X, currentY + takeoverPoints[2].Y);
                    pointsR[1] = new Points(currentX + takeoverPoints[3].X, currentY + takeoverPoints[3].Y);
                    foreach (Points p in pointsR)
                    {
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                        {
                            if (chessManager.GetOccupiedPiece(p).team != team)
                            {
                                AddInCheck(p);
                                newMoves.Add(p);
                            }
                        }
                    }
                    break;
            }

            return newMoves;
        }
        else
        {
            ChessPieceConnection currentConn = chessManager.GetChesspieceConnection(new Points(currentX, currentY));
            List<Points> newMoves = new List<Points>();

            Points c = new Points(0, 0);

            Points[] points = new Points[2];


            // if the pawn is on a player territory

            // if the pawn is on the battlefield




            switch (team) // these are for capturing other pieces
            {
                case 1:
                    points[0] = new Points(currentX + takeoverPoints[0].X, currentY + takeoverPoints[0].Y);
                    points[1] = new Points(currentX + takeoverPoints[1].X, currentY + takeoverPoints[1].Y);
                    foreach (Points p in points)
                    {
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                        {
                            if (chessManager.GetOccupiedPiece(p).team != team)
                            {
                                AddInCheck(p);
                                newMoves.Add(p);
                            }
                        }
                    }
                    break;

                case 2:
                    points[0] = new Points(currentX + takeoverPoints[2].X, currentY + takeoverPoints[2].Y);
                    points[1] = new Points(currentX + takeoverPoints[3].X, currentY + takeoverPoints[3].Y);
                    foreach (Points p in points)
                    {
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                        {
                            if (chessManager.GetOccupiedPiece(p).team != team)
                            {
                                AddInCheck(p);
                                newMoves.Add(p);
                            }
                        }
                    }
                    break;
            }


            /*if (currentConn.spawnTerritoryId.Value == team)
            {
                for (int i = 0; i < addedPoints.Count; i++)
                {
                    if (i != team - 1)
                    {
                        c = new Points(currentX + addedPoints[i].X, currentY + addedPoints[i].Y);
                        if (!chessManager.IsCoordinateInList(c))
                            break;
                        if (chessManager.IsOccupied(c))
                            break;

                        newMoves.Add(c);
                    }
                }
                return newMoves;
            }*/

            switch (currentConn.spawnTerritoryId.Value)
            {
                case 0:
                    for (int i = 0; i < addedPoints.Count; i++)
                    {
                        if (i != team - 1)
                        {
                            c = new Points(currentX + addedPoints[i].X, currentY + addedPoints[i].Y);
                            if (!chessManager.IsCoordinateInList(c))
                                break;
                            if (chessManager.IsOccupied(c))
                                break;

                            newMoves.Add(c);
                        }
                    }
                    break;
                case 1:
                    c = new Points(currentX + addedPoints[1].X, currentY + addedPoints[1].Y);
                    if (!chessManager.IsCoordinateInList(c))
                        break;
                    if (chessManager.IsOccupied(c))
                        break;

                    newMoves.Add(c);
                    break;
                case 2:
                    c = new Points(currentX + addedPoints[0].X, currentY + addedPoints[0].Y);
                    if (!chessManager.IsCoordinateInList(c))
                        break;
                    if (chessManager.IsOccupied(c))
                        break;

                    newMoves.Add(c);
                    break;
                case 3:
                    c = new Points(currentX + addedPoints[2].X, currentY + addedPoints[2].Y);
                    if (!chessManager.IsCoordinateInList(c))
                        break;
                    if (chessManager.IsOccupied(c))
                        break;

                    newMoves.Add(c);
                    break;
                case 4:
                    c = new Points(currentX + addedPoints[3].X, currentY + addedPoints[3].Y);
                    if (!chessManager.IsCoordinateInList(c))
                        break;
                    if (chessManager.IsOccupied(c))
                        break;

                    newMoves.Add(c);
                    break;
            }
            return newMoves;
        }
    }
}
