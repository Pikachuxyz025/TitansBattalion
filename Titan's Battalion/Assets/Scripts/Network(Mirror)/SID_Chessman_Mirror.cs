using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class SID_Chessman_Mirror : NetworkBehaviour
{
   public int CurrentX, CurrentY;
    [HideInInspector] public int rig = 2, whiteInt;
    [SyncVar]
    public bool isWhite, curOnMainBoard;
    //public float range;
    protected Dictionary<SID_BoardGridSet, Points> scouting = new Dictionary<SID_BoardGridSet, Points>();
    public Dictionary<Points, bool> confirmation = new Dictionary<Points, bool>(new Points.EqualityComparer());

    protected SID_BoardPieceManager PieceManager;
    public virtual void Awake()
    {
        SID_BoardManager_Mirror.M_eventmoment.AddListener(Reset);
        PieceManager = SID_BoardPieceManager.instance;
        if (isWhite)
            whiteInt = 0;
        else
            whiteInt = 1;
    }
    public virtual void Update()
    {
        scouting = PieceManager.coordinates;
        RaycastHit hit;
        if (Physics.Raycast(this.transform.position, -transform.up, out hit, 5f, LayerMask.GetMask("ChessPlane")))
        {
            //Debug.Log("Raycast Working");
            SID_BoardGridSet sis = hit.collider.GetComponent<SID_BoardGridSet>();
            CurrentX = sis.GridX;
            CurrentY = sis.GridY;
            if (sis.isMainBoard)
                curOnMainBoard = true;
            else
                curOnMainBoard = false;
        }
        else
        {
            Debug.DrawRay(this.transform.position, -transform.up, Color.red, 1.1f);
        }
    }
    public virtual void Reset()
    {
        rig = 0;
    }
    public virtual bool SameCoord(Points firstPoint, Points secondPoint)
    {
        if (firstPoint.X != secondPoint.X)
            return false;
        else if (firstPoint.Y != secondPoint.Y)
            return false;
        return true;
    }

    public virtual void FindPossiblilties()
    {
        StartCoroutine(RemoveEnough());
    }
    public virtual IEnumerator RemoveEnough()
    {
        yield return null;
    }
}