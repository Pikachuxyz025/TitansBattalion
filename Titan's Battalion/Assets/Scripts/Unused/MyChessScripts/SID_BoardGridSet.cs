using UnityEngine;
using Mirror;

public enum BoardStartPoint
{
    None,
    IsFirstPiece,
    StartingPiecePlayerOne,
    StartingPiecePlayerTwo
}

public class SID_BoardGridSet : NetworkBehaviour
{
    RaycastHit hit;
    public float grdSiz;
    public BoardStartPoint startingPieceOrigin;
    public int GridX = -1, GridY = -1;
    public bool isMainBoard, pieceOn;
    public bool Down, Left, Right, Up;
    public bool connected;
    SID_BoardGridSet GBSone, GBStwo, GBSthree, GBSfour;
    public SID_Chessman chess;
    public SID_Chessman_Mirror chessM;
    public GameObject chesspiece;

    enum PieceState { Empty, Full }
    private void Awake()
    {
        GridX = -1;
        GridY = -1;
    }

    // Update is called once per frame
    void Update()
    {
        ConfigureGrid();
        FindMainBoard(Vector3.back, ref Down, ref GBSone);
        FindMainBoard(Vector3.forward, ref Up, ref GBStwo);
        FindMainBoard(Vector3.left, ref Left, ref GBSthree);
        FindMainBoard(Vector3.right, ref Right, ref GBSfour);
        if (Physics.Raycast(this.transform.position + (new Vector3(.5f, .5f, -.5f)), transform.up, out hit, grdSiz, LayerMask.GetMask("Pieces")))
        {
            pieceOn = true;
            chess = hit.collider.GetComponent<SID_Chessman>();
            chessM = hit.collider.GetComponent<SID_Chessman_Mirror>();
            chesspiece = hit.collider.gameObject;
        }
        else
        {
            pieceOn = false;
        }
        if (!pieceOn)
        {
            chess = null;
            chessM = null;
            chesspiece = null;
        }
    }
    //uses raycast to locate fellow blocks
    void FindMainBoard(Vector3 dir, ref bool avail, ref SID_BoardGridSet GBS)
    {
        if (Physics.Raycast(this.transform.position + (new Vector3(.5f, .5f, -.5f)), dir, out hit, grdSiz, LayerMask.GetMask("ChessPlane")))
        {
            if (hit.collider != null)
            {
                avail = true;
                GBS = hit.collider.gameObject.GetComponent<SID_BoardGridSet>();
                //Debug.Log("We hit this piece: "+hit.collider.gameObject);
            }
            else
            {
                avail = false;
                //Debug.Log("We shouldn't be here");
            }
        }
        else
        {
            avail = false;
        }

    }

    void ConfigureGrid()
    {
        if (startingPieceOrigin==BoardStartPoint.IsFirstPiece) //origin piece
        {
            GridX = 0;
            GridY = 0;
            if (Right) { GBSfour.GridX = 1; GBSfour.GridY = 0; GBSfour.connected = true; }
            if (Left) { GBSthree.GridX = -1; GBSthree.GridY = 0; GBSthree.connected = true; }
            if (Up) { GBStwo.GridX = 0; GBStwo.GridY = 1; GBStwo.connected = true; }
            if (Down) { GBSone.GridY = -1; GBSone.GridX = 0; GBSone.connected = true; }
            connected = true;
        }
        else
        {
            if (connected)
            {
                #region Testing Lines
                if (GridX == 0 && GridY >= 1)
                {
                    if (Up && !GBStwo.connected)
                    {
                        GBStwo.GridY = GridY + 1;
                        GBStwo.GridX = GridX;
                        GBStwo.connected = true;
                    }
                }

                if (GridX == 0 && GridY <= -1)
                {
                    if (Down && !GBSone.connected)
                    {
                        GBSone.GridY = GridY - 1;
                        GBSone.GridX = GridX;
                        GBSone.connected = true;
                    }
                }

                if (GridX < 0 && GridY == 0)
                {
                    if (Left && !GBSthree.connected)
                    {
                        GBSthree.GridX = GridX - 1;
                        GBSthree.GridY = GridY;
                        GBSthree.connected = true;
                    }
                }

                if (GridX > 0 && GridY == 0)
                {
                    if (Right && !GBSfour.connected)
                    {
                        GBSfour.GridX = GridX + 1;
                        GBSfour.GridY = GridY;
                        GBSfour.connected = true;
                    }
                }

                #endregion

                #region Quadrants
                //quadrant (+,+)
                if ((GridY >= 0 && GridX >= 0))
                {
                    if (Up && startingPieceOrigin != BoardStartPoint.IsFirstPiece)
                    {
                        if (GBStwo.GridY != GridY + 1)
                        {
                            GBStwo.GridY = GridY + 1;
                        }
                        else
                        {
                            if (GBStwo.GridX != GridX)
                            {
                                GBStwo.GridX = GridX;
                            }
                            else
                            {
                                GBStwo.connected = true;
                            }
                        }
                    }

                    if (Right && startingPieceOrigin != BoardStartPoint.IsFirstPiece && !GBSfour.connected)
                    {
                        if (GBSfour.GridX != GridX + 1)
                        {
                            GBSfour.GridX = GridX + 1;
                        }
                        else
                        {
                            if (GBSfour.GridY != GridY)
                            {
                                GBSfour.GridY = GridY;
                            }
                            else
                            {
                                GBSfour.connected = true;
                            }
                        }
                    }
                }


                //quadrant (-,+)
                if ((GridY >= 0 && GridX <= 0))
                {
                    if (Up && startingPieceOrigin != BoardStartPoint.IsFirstPiece)
                    {
                        if (GBStwo.GridY != GridY + 1)
                        {
                            GBStwo.GridY = GridY + 1;
                        }
                        else
                        {
                            if (GBStwo.GridX != GridX)
                            {
                                GBStwo.GridX = GridX;
                            }
                            else
                            {
                                GBStwo.connected = true;
                            }
                        }
                    }

                    if (Left && startingPieceOrigin != BoardStartPoint.IsFirstPiece)// && !GBSthree.connected)
                    {
                        if (GBSthree.GridX != GridX - 1)
                        {
                            GBSthree.GridX = GridX - 1;
                        }
                        else
                        {
                            if (GBSthree.GridY != GridY)
                            {
                                GBSthree.GridY = GridY;
                            }
                            else
                            {
                                GBSthree.connected = true;
                            }
                        }
                    }
                }
                //quadrant (+,-)
                if ((GridY <= 0 && GridX >= 0))
                {
                    if (Right && startingPieceOrigin != BoardStartPoint.IsFirstPiece)
                    {
                        if (GBSfour.GridX != GridX + 1)
                        {
                            GBSfour.GridX = GridX + 1;
                        }
                        else
                        {
                            if (GBSfour.GridY != GridY)
                            {
                                GBSfour.GridY = GridY;
                            }
                            else
                            {
                                GBSfour.connected = true;
                            }
                        }
                    }
                    if (Down && startingPieceOrigin != BoardStartPoint.IsFirstPiece)// && !GBSone.connected)
                    {
                        if (GBSone.GridY != GridY - 1)
                        {
                            GBSone.GridY = GridY - 1;
                        }
                        else
                        {
                            if (GBSone.GridX != GridX)
                            {
                                GBSone.GridX = GridX;
                            }
                            else
                            {
                                GBSone.connected = true;
                            }
                        }
                    }
                }
                //everything else
                if (GridY <= 0 && GridX <= 0)
                {
                    if (Left && startingPieceOrigin != BoardStartPoint.IsFirstPiece)// && !GBSthree.connected)
                    {
                        if (GBSthree.GridX != GridX - 1)
                        {
                            GBSthree.GridX = GridX - 1;
                        }
                        else
                        {
                            if (GBSthree.GridY != GridY)
                            {
                                GBSthree.GridY = GridY;
                            }
                            else
                            {
                                GBSthree.connected = true;
                            }
                        }

                    }

                    if (Down && startingPieceOrigin != BoardStartPoint.IsFirstPiece)// && !GBSone.connected)
                    {
                        if (GBSone.GridY != GridY - 1)
                        {
                            GBSone.GridY = GridY - 1;
                        }
                        else
                        {
                            if (GBSone.GridX != GridX)
                            {
                                GBSone.GridX = GridX;
                            }
                            else
                            {
                                GBSone.connected = true;
                            }
                        }
                    }
                }
                #endregion

                #region Testing Quadrants
                //quadrant (+,+)
                if ((GridY >= 0 && GridX >= 0))
                {
                    if (Up && !GBStwo.connected)
                    {
                        GBStwo.GridY = GridY + 1;
                        GBStwo.GridX = GridX;
                        GBStwo.connected = true;
                    }

                    if (Right && !GBSfour.connected)
                    {
                        GBSfour.GridX = GridX + 1;
                        GBSfour.GridY = GridY;
                        GBSfour.connected = true;
                    }
                }

                //quadrant (-,+)
                if ((GridY >= 0 && GridX <= 0))
                {
                    if (Up && !GBStwo.connected)
                    {
                        GBStwo.GridY = GridY + 1;
                        GBStwo.GridX = GridX;
                        GBStwo.connected = true;
                    }

                    if (Left && !GBSthree.connected)
                    {
                        GBSthree.GridX = GridX - 1;
                        GBSthree.GridY = GridY;
                        GBSthree.connected = true;
                    }
                }
                //quadrant (+,-)
                if ((GridY <= 0 && GridX >= 0))
                {
                    if (Right && !GBSfour.connected)
                    {
                        GBSfour.GridX = GridX + 1;
                        GBSfour.GridY = GridY;
                        GBSfour.connected = true;
                    }
                    if (Down && !GBSone.connected)// && !GBSone.connected)
                    {
                        GBSone.GridY = GridY - 1;
                        GBSone.GridX = GridX;
                        GBSone.connected = true;
                    }
                }

                //quadrant (-,-)
                if (GridY <= 0 && GridX <= 0)
                {
                    if (Left && !GBSthree.connected)
                    {
                        GBSthree.GridX = GridX - 1;
                        GBSthree.GridY = GridY;
                        GBSthree.connected = true;
                    }

                    if (Down && !GBSone.connected)
                    {
                        GBSone.GridY = GridY - 1;
                        GBSone.GridX = GridX;
                        GBSone.connected = true;
                    }
                }

                //everything else
                if (Left && !GBSthree.connected)
                {
                    GBSthree.GridX = GridX - 1;
                    GBSthree.GridY = GridY;
                    GBSthree.connected = true;
                }
                if (Down && !GBSone.connected)
                {
                    GBSone.GridY = GridY - 1;
                    GBSone.GridX = GridX;
                    GBSone.connected = true;
                }
                if (Right && !GBSfour.connected)
                {
                    GBSfour.GridX = GridX + 1;
                    GBSfour.GridY = GridY;
                    GBSfour.connected = true;
                }
                if (Up && !GBStwo.connected)
                {
                    GBStwo.GridY = GridY + 1;
                    GBStwo.GridX = GridX;
                    GBStwo.connected = true;
                }
                #endregion
            }
        }
    }
}
