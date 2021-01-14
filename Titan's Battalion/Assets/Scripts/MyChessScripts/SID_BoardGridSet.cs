using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class SID_BoardGridSet : NetworkBehaviour
{
    RaycastHit hit;
    public float grdSiz;
    public int GridX = -1, GridY = -1;
    public bool isFirstPiece, startingPieceone, startingPiecetwo, isMainBoard,pieceOn;
    public bool Down, Left, Right, Up;
    //[HideInInspector]
    public bool connected;
    SID_BoardGridSet GBSone,GBStwo,GBSthree,GBSfour;
    public SID_Chessman chess;
    public SID_Chessman_Mirror chessM;
    private void Awake()
    {
        GridX = -1;
        GridY = -1;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
  
        ConfigureGrid();
        FindMainBoard(Vector3.back/*-transform.forward*/, ref Down, ref GBSone);
        FindMainBoard(Vector3.forward/*transform.forward*/, ref Up, ref GBStwo);
        FindMainBoard(Vector3.left/*-transform.right*/, ref Left, ref GBSthree);
        FindMainBoard(Vector3.right/*transform.right*/, ref Right, ref GBSfour);
        if (Physics.Raycast(this.transform.position + (new Vector3(.5f, .5f, -.5f)), transform.up, out hit, grdSiz, LayerMask.GetMask("Pieces")))
        {
            pieceOn = true;
            chess = hit.collider.GetComponent<SID_Chessman>();
            chessM = hit.collider.GetComponent<SID_Chessman_Mirror>();
        }
        else
        {
            pieceOn = false;
        }
        if (!pieceOn)
        {
            chess = null;
            chessM = null;
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
        if (isFirstPiece) //origin piece
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
                #region Lines
                if (GridX == 0 && GridY >= 1)
                {
                    if (Up && !GBStwo.isFirstPiece)
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
                }
                if (GridX == 0 && GridY <= -1)
                {
                    if (Down && !GBSone.isFirstPiece)
                    {
                        if (GBSone.GridY != GridY - 1)
                        {
                            //Debug.Log("grid origin not funcitonal");
                            GBSone.GridY = GridY - 1;
                        }
                        else
                        {
                            if (GBSone.GridX != 0)
                            {
                                GBSone.GridX = 0;
                            }
                            else
                            {
                                GBSone.connected = true;
                            }
                        }
                    }
                }
                if (GridX < 0 && GridY == 0)
                {
                    if (Left && !GBSthree.isFirstPiece)
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
                if (GridX > 0 && GridY == 0)
                {
                    if (Right && !GBSfour.isFirstPiece)
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
                #endregion

                #region Quadrants
                //quadrant (+,+)
                if ((GridY >= 0 && GridX >= 0))
                {
                    if (Up && !GBStwo.isFirstPiece)
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

                    if (Right && !GBSfour.isFirstPiece && !GBSfour.connected)
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
                    if (Up && !GBStwo.isFirstPiece)
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

                    if (Left && !GBSthree.isFirstPiece)// && !GBSthree.connected)
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
                    if (Right && !GBSfour.isFirstPiece)
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
                    if (Down && !GBSone.isFirstPiece)// && !GBSone.connected)
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
                    if (Left && !GBSthree.isFirstPiece)// && !GBSthree.connected)
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

                    if (Down && !GBSone.isFirstPiece)// && !GBSone.connected)
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
            }
        }
    }
}
