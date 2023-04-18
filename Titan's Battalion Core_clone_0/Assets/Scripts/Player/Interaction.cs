using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
public class Interaction : NetworkBehaviour
{
    [SerializeField] private Camera currentCamera;
    [SerializeField] private Player controllingPlayer;
    [SerializeField] private ChessPieceManager pieceManager;
    [SerializeField] private GameManager gameManager;

    [SerializeField] private Chesspiece currentlyDragging;
    [SerializeField] private Points currentHover = new Points(1984987, 51684);

    [SerializeField] private float yoffset = .2f;
    [SerializeField] private float dragOffset = 1.5f;

    [SerializeField] private List<Points> availableMoves = new List<Points>();
    [SerializeField] private List<Points> availableSpecialMoves = new List<Points>();
    [SerializeField] private List<Points> unavailableMoves = new List<Points>();

    private bool isMoving = false;
    private bool setCurrentDrag = false;


    public void SetCurrentCamera(Camera cam) => currentCamera = cam;
    public override void OnNetworkSpawn()
    {
        gameManager = GameManager.instance;
        pieceManager = ChessPieceManager.instance;
    }

    [ServerRpc]
    public void ResetCurrentHoverServerRpc()
    {
        if (pieceManager.IsCoordinateInList(currentHover))
        {
            pieceManager.SwapLayerServerRpc(currentHover.X, currentHover.Y, "Tile", OwnerClientId);
            currentHover = new Points(1984987, 51684);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
            return;
        if (controllingPlayer.currentSetModeNet.Value == SetMode.Set && controllingPlayer.NetworkIsMyTurn.Value)
            Interact();
        else if (controllingPlayer.currentSetModeNet.Value == SetMode.Set && !controllingPlayer.NetworkIsMyTurn.Value)
            ResetCurrentHoverServerRpc();
    }

    #region Interact with board
    private void Interact()
    {

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0) && !isMoving)
            SetCurrentDragBoolServerRpc(true);
        else if (Input.GetMouseButtonUp(0))
            SetCurrentDragBoolServerRpc(false);

        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight","Special")))
        {
            ChessPieceConnection connectionContact = info.transform.gameObject.GetComponent<ChessPieceConnection>();

            // Get the indexes of the tile i've hit
            Points hitPosition = connectionContact.CurrentTilePoint();
            SetCurrentHoverServerRpc(currentHover.X, currentHover.Y, hitPosition.X, hitPosition.Y);
        }
        else
        {
            OutsideBoardServerRpc();
        }


        DraggingServerRpc(ray);
    }

    [ServerRpc]
    void DraggingServerRpc(Ray ray)
    {
        if (currentlyDragging)
        {
            Plane horizonPlane = new Plane(Vector3.up, Vector3.up * yoffset);
            float distance = 0.0f;
            if (horizonPlane.Raycast(ray, out distance))
                currentlyDragging.ReturnPositionServerRpc(ray.GetPoint(distance) + Vector3.up * dragOffset);
        }
    }
    [ServerRpc]
    void SetCurrentHoverServerRpc(int cX, int cY, int hX, int hY)
    {
        Points cHover = new Points(cX, cY);
        Points hHover = new Points(hX, hY);


        if (!pieceManager.IsCoordinateInList(currentHover))
        {
            currentHover = hHover;
            pieceManager.SwapLayerServerRpc(currentHover.X, currentHover.Y, "Hover", OwnerClientId);
        }

        // If we were already hovering a tile, change the previous one
        if (!Points.DualEquals(currentHover, hHover))
        {
            if (ContainsVaildMove(ref availableMoves, currentHover) || ContainsVaildMove(ref availableSpecialMoves, currentHover))
                pieceManager.SwapLayerServerRpc(currentHover.X, currentHover.Y, "Highlight", OwnerClientId);
            else
                pieceManager.SwapLayerServerRpc(currentHover.X, currentHover.Y, "Tile", OwnerClientId);

            currentHover = hHover;
            pieceManager.SwapLayerServerRpc(hHover.X, hHover.Y, "Hover", OwnerClientId);

        }
        //if we press down on the mouse
        if (setCurrentDrag)//Input.GetMouseButtonDown(0))
        {
            if (currentlyDragging == null)
                CurrentDrag(hHover.X, hHover.Y);
        }

        //if we are releasing the mouse
        if (currentlyDragging != null && !setCurrentDrag)//Input.GetMouseButtonUp(0))
        {
            ReleaseDrag(hHover.X, hHover.Y);
        }
    }

    [ServerRpc]
    void SetCurrentDragBoolServerRpc(bool t)
    {
        setCurrentDrag = t;
    }

    [ServerRpc]
    void OutsideBoardServerRpc()
    {
        // that the current hover position and set the tag back to the original tag
        if (pieceManager.IsCoordinateInList(currentHover))
        {
            // whether that tag is highlight
            if (ContainsVaildMove(ref availableMoves, currentHover) || ContainsVaildMove(ref availableSpecialMoves, currentHover))
                pieceManager.SwapLayerServerRpc(currentHover.X, currentHover.Y, "Highlight", OwnerClientId);
            else
                pieceManager.SwapLayerServerRpc(currentHover.X, currentHover.Y, "Tile", OwnerClientId); // or the tag is Tile
            // identify current hover out the range of the board
            currentHover = new Points(1984987, 51684);
        }

        // If we aren't carrying the object anymore or if we let go of the object we're holding
        if (currentlyDragging != null && !setCurrentDrag)
        {
            // the position the object was originally at
            Points priorPosition = new Points(currentlyDragging.currentX, currentlyDragging.currentY);
            // move the object back to that position
            currentlyDragging.ReturnPositionServerRpc(pieceManager.GetNewPiecePosition(priorPosition));
            currentlyDragging = null;
            RemoveHighlightTilesServerRpc();
        }
    }



    void ReleaseDrag(int x, int y)
    {
        Points newPosition = new Points(x, y);
        Points priorPosition = new Points(currentlyDragging.currentX, currentlyDragging.currentY);
        bool validMove = MoveTo(currentlyDragging, newPosition);
        if (!validMove)
            currentlyDragging.ReturnPositionServerRpc(pieceManager.GetNewPiecePosition(priorPosition));

        currentlyDragging = null;
        RemoveHighlightTilesServerRpc();
        isMoving = false;
    }

    void CurrentDrag(int x, int y)
    {
        Points newPosition = new Points(x, y);
        if (pieceManager.GetChesspieceGameObject(newPosition) != null)
        {
            Chesspiece conn = pieceManager.GetChesspieceConnection(newPosition).GetOccupiedPiece();
            // Is it our turn
            if (true && conn != null)
            {
                if (conn.OwnerClientId == OwnerClientId)
                    currentlyDragging = conn;
                if (currentlyDragging != null)
                {
                    // Get List of where I can go, highlight list as well
                    availableMoves = currentlyDragging.GetAvailableMoves();
                    // if (currentlyDragging.GetSpecialMoves().Count > 0)
                    availableSpecialMoves = currentlyDragging.GetSpecialMoves();
                    if (controllingPlayer.isKingInCheck && !currentlyDragging.isKing)
                    {
                        Debug.Log(currentlyDragging.gameObject.name+" Isn't King");
                        availableMoves = controllingPlayer.playerCheckableList.GetDownMyKing(availableMoves);
                        availableSpecialMoves = controllingPlayer.playerCheckableList.GetDownMyKing(availableSpecialMoves);
                    }
                }
                HighlightTilesServerRpc();
            }
        }
    }

    private bool MoveTo(Chesspiece movingChesspiece, Points hitPosition)
    {
        isMoving = true;
        Points previousPosition = new Points(movingChesspiece.currentX, movingChesspiece.currentY);

        if (ContainsVaildMove(ref availableSpecialMoves, hitPosition))
        {
            Debug.Log("special move Set");
            switch (movingChesspiece.specialMove)
            {
                case SpecialMove.EnPassant:
                    Debug.Log("this is en passant");
                    if (!movingChesspiece.gameObject.GetComponent<Pawn>())
                        break;
                    Pawn movingPawnPiece = movingChesspiece.gameObject.GetComponent<Pawn>();
                    if (movingPawnPiece.pawns.ContainsKey(hitPosition))
                        Destroy(movingPawnPiece.pawns[hitPosition].gameObject);

                    pieceManager.GetChesspieceConnection(hitPosition).SetOccupiedPiece(movingChesspiece);
                    pieceManager.GetChesspieceConnection(previousPosition).SetOccupiedPiece(null);
                    movingChesspiece.hasMoved = true;
                    pieceManager.PositionSinglePiece(movingChesspiece, hitPosition);


                    break;
                case SpecialMove.Castling:
                    Debug.Log("this is castling");
                    Rook rook = null;
                    if (!movingChesspiece.gameObject.GetComponent<King>())
                        break;

                    int pointOffset = 0;
                    King kingPiece = movingChesspiece.gameObject.GetComponent<King>();

                    if (hitPosition.X > previousPosition.X)
                    {
                        pointOffset = 1;
                        foreach (Rook rookie in kingPiece.rooks)
                        {
                            if (rookie.currentX > movingChesspiece.currentX && rookie.currentY == movingChesspiece.currentY)
                                rook = rookie;
                        }
                        Points newKingPosition = new Points(kingPiece.currentX + pointOffset, kingPiece.currentY);
                        Points newRookPosition = new Points(rook.currentX, rook.currentY);
                        SetKingRook(ref movingChesspiece, hitPosition, previousPosition, ref rook, newKingPosition, newRookPosition);
                    }
                    else if (hitPosition.X < previousPosition.X)
                    {
                        pointOffset = -1;
                        foreach (Rook rookie in kingPiece.rooks)
                        {
                            if (rookie.currentX < movingChesspiece.currentX && rookie.currentY == movingChesspiece.currentY)
                                rook = rookie;
                        }
                        Points newKingPosition = new Points(kingPiece.currentX + pointOffset, kingPiece.currentY);
                        Points newRookPosition = new Points(rook.currentX, rook.currentY);
                        SetKingRook(ref movingChesspiece, hitPosition, previousPosition, ref rook, newKingPosition, newRookPosition);
                    }


                    if (hitPosition.Y < previousPosition.Y)
                    {
                        pointOffset = -1;
                        foreach (Rook rookie in kingPiece.rooks)
                        {
                            if (rookie.currentY < movingChesspiece.currentY && rookie.currentX == movingChesspiece.currentX)
                                rook = rookie;
                        }
                        Points newKingPosition = new Points(kingPiece.currentX, kingPiece.currentY + pointOffset);
                        Points newRookPosition = new Points(rook.currentX, rook.currentY);
                        SetKingRook(ref movingChesspiece, hitPosition, previousPosition, ref rook, newKingPosition, newRookPosition);
                    }
                    else if (hitPosition.Y > previousPosition.Y)
                    {
                        pointOffset = 1;
                        foreach (Rook rookie in kingPiece.rooks)
                        {
                            if (rookie.currentY > movingChesspiece.currentY && rookie.currentX == movingChesspiece.currentX)
                                rook = rookie;
                        }
                        Points newKingPosition = new Points(kingPiece.currentX, kingPiece.currentY + pointOffset);
                        Points newRookPosition = new Points(rook.currentX, rook.currentY);
                        SetKingRook(ref movingChesspiece, hitPosition, previousPosition, ref rook, newKingPosition, newRookPosition);
                    }
                    break;

            }
            gameManager.SetPlayerTurnServerRpc();
            return true;
        }


        if (!ContainsVaildMove(ref availableMoves, hitPosition) && !ContainsVaildMove(ref availableSpecialMoves, hitPosition))
        {
            Debug.Log("Hitpostion: (" + hitPosition.X + ", " + hitPosition.Y + ") isn't valid");
            return false;
        }

        if (pieceManager.IsOccupied(hitPosition))

        {
            Chesspiece occupiedPiece = pieceManager.GetChesspieceConnection(hitPosition).occupiedChesspiece;

            // if the teams are the same return false
            if (occupiedPiece.team == movingChesspiece.team)
                return false;

            // If it's the enemy
            //if (occupiedPiece.isKing)
            //CheckMate(teamNumber.Value); 
            // Temporary Delete
            // Remove From List

            Destroy(occupiedPiece.gameObject);
        }

        // Self Capture 
        // Might need an enum for types of piece


        pieceManager.GetChesspieceConnection(hitPosition).SetOccupiedPiece(movingChesspiece);
        pieceManager.GetChesspieceConnection(previousPosition).SetOccupiedPiece(null);

        // Let's just try this:
        Pawn activeEnPassantPawn = null;
        if (movingChesspiece is Pawn)
            activeEnPassantPawn = movingChesspiece.gameObject.GetComponent<Pawn>();


        if (activeEnPassantPawn != null)
            activeEnPassantPawn.ActiveEnPassantPosition(hitPosition);


        if (!movingChesspiece.hasMoved)
            movingChesspiece.hasMoved = true;

        pieceManager.PositionSinglePiece(movingChesspiece, hitPosition);


        gameManager.SetPlayerTurnServerRpc();
        return true;
    }

    private void SetKingRook(ref Chesspiece kingChesspiece, Points hitPosition, Points previousPosition, ref Rook rook, Points newKingPosition, Points newRookPosition)
    {
        pieceManager.GetChesspieceConnection(hitPosition).SetOccupiedPiece(kingChesspiece);
        pieceManager.GetChesspieceConnection(previousPosition).SetOccupiedPiece(null);
        kingChesspiece.hasMoved = true;
        pieceManager.PositionSinglePiece(kingChesspiece, hitPosition);

        pieceManager.GetChesspieceConnection(newKingPosition).SetOccupiedPiece(rook);
        pieceManager.GetChesspieceConnection(newRookPosition).SetOccupiedPiece(null);
        rook.hasMoved = true;
        pieceManager.PositionSinglePiece(rook, newKingPosition);
    }


    [ServerRpc]
    void HighlightTilesServerRpc()
    {
        for (int i = 0; i < availableMoves.Count; i++)
            pieceManager.SwapLayerServerRpc(availableMoves[i].X, availableMoves[i].Y, "Highlight", OwnerClientId);

        for (int i = 0; i < availableSpecialMoves.Count; i++)
            pieceManager.SwapLayerServerRpc(availableSpecialMoves[i].X, availableSpecialMoves[i].Y, "Special", OwnerClientId);

        //for (int i = 0; i < unavailableMoves.Count; i++)
            //pieceManager.SwapLayerServerRpc(unavailableMoves[i].X, unavailableMoves[i].Y, "Unavailable", OwnerClientId);
    }

    [ServerRpc]
    void RemoveHighlightTilesServerRpc()
    {
        for (int i = 0; i < availableMoves.Count; i++)
            pieceManager.SwapLayerServerRpc(availableMoves[i].X, availableMoves[i].Y, "Tile", OwnerClientId);

        for (int i = 0; i < availableSpecialMoves.Count; i++)
            pieceManager.SwapLayerServerRpc(availableSpecialMoves[i].X, availableSpecialMoves[i].Y, "Tile", OwnerClientId);

        //for (int i = 0; i < unavailableMoves.Count; i++)
            //pieceManager.SwapLayerServerRpc(unavailableMoves[i].X, unavailableMoves[i].Y, "Tile", OwnerClientId);

        availableMoves.Clear();
        availableSpecialMoves.Clear();
        //unavailableMoves.Clear();
    }


    private bool ContainsVaildMove(ref List<Points> moves, Points pointPos)
    {
        for (int i = 0; i < moves.Count; i++)
            if (Points.DualEquals(moves[i], pointPos))
                return true;
        return false;

    }

    #endregion
}
