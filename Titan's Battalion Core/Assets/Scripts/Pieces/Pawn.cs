using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class Pawn : Chesspiece
{
    [SerializeField] private List<Points> moveList = new List<Points>();
    public readonly Dictionary<Points, Pawn> pawns = new Dictionary<Points, Pawn>(new Points.EqualityComparer());
    [SerializeField] protected GameObject convertableQueen;

    [SerializeField] private List<Points> takeoverPoints = new List<Points>();
    private ChessPieceConnection enPassantPoint;
    [SerializeField] private bool isEnPassantActive;
    private int enPassantTimeOfMoveCount = 1464674684;
    private Points[] startingMoves = new Points[2];


    public List<Pawn> PassantPawns()
    {

        List<Pawn> possiblePawnTargets = new List<Pawn>();


        Points[] checkPoints = new Points[4];
        checkPoints[0] = new Points(currentX + 0, currentY + 1);
        checkPoints[1] = new Points(currentX + 0, currentY + -1);
        checkPoints[2] = new Points(currentX + -1, currentY + 0);
        checkPoints[3] = new Points(currentX + 1, currentY + 0);

        for (int i = 0; i < checkPoints.Length; i++)
        {
            Points currentCheckPoint = checkPoints[i];
            if (!chessManager.IsCoordinateInList(currentCheckPoint))
                continue;

            if (!chessManager.IsOccupied(currentCheckPoint))
                continue;


            Chesspiece foundChesspiece = chessManager.GetOccupiedPiece(currentCheckPoint);

            if (foundChesspiece is Pawn && foundChesspiece.team != team)
            {
                Pawn enemyPawn = foundChesspiece.gameObject.GetComponent<Pawn>();
                possiblePawnTargets.Add(enemyPawn);
            }
        }
        if (possiblePawnTargets.Count == 0)
            Debug.Log("no pawns were found");
        return possiblePawnTargets;
    }



    public void AddToMoveList()
    {
        Points currentPoint = new Points(currentX, currentY);
        moveList.Add(currentPoint);
        if (moveList.Count > enPassantTimeOfMoveCount && isEnPassantActive)
            isEnPassantActive = false;
    }

    private List<Points> PointsToAdd(List<Points> setupPositions)
    {
        List<Points> result = new List<Points>();
        int x = 0;
        int y = 0;
        SetupIndexesBasedOnTeam(out x, out y);
        result.Add(new Points(currentX + setupPositions[x].X, currentY + setupPositions[x].Y));
        result.Add(new Points(currentX + setupPositions[y].X, currentY + setupPositions[y].Y));
        return result;
    }

    private Points[] StartingPointsToAdd(List<Points> setupPositions)
    {
        Points[] result = new Points[2];
        int x = 0;
        int y = 0;
        SetupIndexesBasedOnTeam(out x, out y);
        result[0] = new Points(currentX + setupPositions[x].X, currentY + setupPositions[x].Y);
        result[1] = new Points(currentX + setupPositions[y].X, currentY + setupPositions[y].Y);
        return result;
    }

    private void SetupIndexesBasedOnTeam(out int x, out int y)
    {
        x = 0;
        y = 0;
        switch (team)
        {
            case 1:
                x = 0;
                y = 1;
                break;
            case 2:
                x = 2;
                y = 3;
                break;
            case 3:
                x = 4;
                y = 5;
                break;
            case 4:
                x = 6;
                y = 7;
                break;
        }
    }

    private List<Points> PointsToTakeover(List<Points> setupPositions)
    {
        int x = 0;
        int y = 0;
        SetupIndexesBasedOnTeam(out x, out y);

        List<Points> result = new List<Points>();
        List<Points> createdPoints = new List<Points>();
        createdPoints.Add(new Points(currentX + setupPositions[x].X, currentY + setupPositions[x].Y));
        createdPoints.Add(new Points(currentX + setupPositions[y].X, currentY + setupPositions[y].Y));

        foreach (Points createdPoint in createdPoints)
        {
            if (!chessManager.IsCoordinateInList(createdPoint))
                break;
            if (chessManager.IsOccupied(createdPoint))
            {
                if (chessManager.GetOccupiedPiece(createdPoint).team != team)
                {
                    AddInCheck(createdPoint);
                    result.Add(createdPoint);
                }
            }
        }

        return result;
    }

    private List<Points> BasicPointsSeptup(List<Points> setupPositions)
    {
        int x = 0;
        int y = 0;
        SetupIndexesBasedOnTeam(out x, out y);

        List<Points> result = new List<Points>();
        List<Points> createdPoints = new List<Points>();
        createdPoints.Add(new Points(currentX + setupPositions[x].X, currentY + setupPositions[x].Y));
        createdPoints.Add(new Points(currentX + setupPositions[y].X, currentY + setupPositions[y].Y));

        foreach (Points createdPoint in createdPoints)
        {
            if (!chessManager.IsCoordinateInList(createdPoint))
                continue;
            if (chessManager.IsOccupied(createdPoint))
                break;

            result.Add(createdPoint);
        }
        return result;
    }

    public void ActiveEnPassantPosition(Points choosenLocation)
    {

        Debug.Log("Choosen Move: (" + choosenLocation.X + ", " + choosenLocation.Y + ") | startingMoves: (" + startingMoves[1].X + ", " + startingMoves[1].Y + ")");

        if (choosenLocation.Equals(startingMoves[1]))
        {
            isEnPassantActive = true;
            enPassantPoint = chessManager.GetChesspieceConnection(startingMoves[0]);
            enPassantTimeOfMoveCount = moveList.Count + 1;
            Debug.Log("EnPassant should be active");
        }
    }

    public Points GetEnPassantPosition()
    {
        return enPassantPoint.GetChessboardPosition();
    }

    public bool IsEnPassantActive()
    { return isEnPassantActive; }

    public bool CanCovertToQueen()
    {
        ChessPieceConnection currentConn = chessManager.GetChesspieceConnection(new Points(currentX, currentY));
        if (currentConn.spawnTerritoryId.Value == team)
            return false;

        Points c;

        if (currentConn.spawnTerritoryId.Value > 0)
        {
            c = new Points(currentX + addedPoints[currentConn.spawnTerritoryId.Value - 1].X, currentY + addedPoints[currentConn.spawnTerritoryId.Value - 1].Y);
            if (!chessManager.IsCoordinateInList(c))
            {
                Debug.Log(currentConn.spawnTerritoryId.Value + ": " + c.X + ", " + c.Y);
                return true;
            }
        }
        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ConvertToQueenServerRpc()
    {
        if (CanCovertToQueen())
        {
            GameObject newQueen = Instantiate(convertableQueen);
            newQueen.GetComponent<NetworkObject>().Spawn();

            Chesspiece queenPiece = newQueen.GetComponent<Chesspiece>();
            ChessPieceConnection currentChessboardPiece = chessManager.GetChesspieceConnection(new Points(currentX, currentY));

            newQueen.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);
            currentChessboardPiece.SetOccupiedPiece(queenPiece);
            chessManager.PositionSinglePiece(queenPiece, currentChessboardPiece);

            Destroy(this.gameObject);
        }
    }

    public override List<Points> GetSpecialMoves()
    {
        List<Points> takeoverMoveList = PointsToAdd(takeoverPoints);
        List<Points> result = new List<Points>();
        List<Pawn> passantPawns = PassantPawns();
        foreach (Pawn pawn in passantPawns)
        {
            if (!pawn.IsEnPassantActive())
                continue;
            Debug.Log(pawn.gameObject.name + " is is in passant mode");
            if (enPassantPoint == null)
                continue;
            Debug.Log("There is a point to go to");
            if (!takeoverMoveList.Contains(pawn.GetEnPassantPosition()))
                continue;
            Debug.Log("If this is showing, we got through all of them. Here: " + pawn.GetEnPassantPosition().X + ", " + pawn.GetEnPassantPosition().Y);

            if (!result.Contains(pawn.GetEnPassantPosition()))
                result.Add(pawn.GetEnPassantPosition());
            if (!pawns.ContainsKey(pawn.GetEnPassantPosition()))
                pawns.Add(pawn.GetEnPassantPosition(), pawn);
        }
        specialMove = SpecialMove.EnPassant;
        return result;
    }

    public override List<Points> GetAvailableMoves()
    {
        if (!hasMoved)
        {
            List<Points> newMoves = new List<Points>();
            startingMoves = StartingPointsToAdd(firstMovePoints);

            newMoves.AddRange(BasicPointsSeptup(firstMovePoints));
            newMoves.AddRange(PointsToTakeover(takeoverPoints));
            return newMoves;
        }
        else
        {
            ChessPieceConnection currentConn = chessManager.GetChesspieceConnection(new Points(currentX, currentY));
            List<Points> newMoves = new List<Points>();

            // places to capture other pieces

            newMoves.AddRange(PointsToTakeover(takeoverPoints));
            switch (currentGameMode.Value)
            {
                case GameMode.Chess:
                    // basic chess rules
                    int x = 0;
                    switch (team)
                    {
                        case 1:
                            x = 1;
                            break;
                        case 2:
                            x = 0;
                            break;
                    }

                    Points nextAvailablePosition = new Points(currentX + addedPoints[x].X, currentY + addedPoints[x].Y);
                    if (!chessManager.IsCoordinateInList(nextAvailablePosition))
                        break;
                    if (chessManager.IsOccupied(nextAvailablePosition))
                        break;

                    newMoves.Add(nextAvailablePosition);

                    break;
                case GameMode.T2:
                    // places to go depending on what territory you're on
                    if (currentConn.spawnTerritoryId.Value == 0)
                    {
                        for (int i = 0; i < addedPoints.Count; i++)
                        {
                            if (i != team - 1)
                            {
                                nextAvailablePosition = new Points(currentX + addedPoints[i].X, currentY + addedPoints[i].Y);
                                if (!chessManager.IsCoordinateInList(nextAvailablePosition))
                                    continue;
                                if (chessManager.IsOccupied(nextAvailablePosition))
                                    continue;

                                newMoves.Add(nextAvailablePosition);
                            }
                        }
                    }
                    else
                    {
                        x = 0;
                        int y = 0;
                        switch (currentConn.spawnTerritoryId.Value)
                        {
                            case 1:
                                x = 1;
                                y = 0;
                                break;
                            case 2:
                                x = 0;
                                y = 1;
                                break;
                        }
                        nextAvailablePosition = currentConn.spawnTerritoryId.Value == team ?
                        new Points(currentX + addedPoints[x].X, currentY + addedPoints[x].Y)
                        : new Points(currentX + addedPoints[y].X, currentY + addedPoints[y].Y);
                        if (!chessManager.IsCoordinateInList(nextAvailablePosition))
                            break;
                        if (chessManager.IsOccupied(nextAvailablePosition))
                            break;

                        newMoves.Add(nextAvailablePosition);
                    }
                    break;
            }
            return newMoves;
        }
    }
}
