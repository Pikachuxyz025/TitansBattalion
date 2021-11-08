using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using UnityEngine.Events;

public enum MoveInfo
{
    currentMoves,
    futureMoves
}

public enum CheckState
{
    Safe,
    inCheckZone,
    inCheck
}

public abstract class SID_Chessman_Mirror : NetworkBehaviour
{
    public int CurrentX, CurrentY;
    public int rig = 2, whiteInt;
    public bool isKing;
    [SyncVar]
    public PlayerInfo authorityPlayer;
    [SyncVar]
    public bool isWhite, curOnMainBoard;
    public static SID_Chessman_Mirror instance;
    //public float range;
    protected Dictionary<SID_BoardGridSet, Points> scouting = new Dictionary<SID_BoardGridSet, Points>();
    public Dictionary<Points, bool> confirmedMoves = new Dictionary<Points, bool>(new Points.EqualityComparer());
    public Dictionary<Points, bool> futureMoves = new Dictionary<Points, bool>(new Points.EqualityComparer());
    public Dictionary<Points, List<Points>> futureTies = new Dictionary<Points, List<Points>>(new Points.EqualityComparer());
    protected SID_BoardPieceManager PieceManager;
    public virtual void Awake()
    {
        //if (SID_BoardManager_Mirror.M_eventmoment == null)
        //{
        //    SID_BoardManager_Mirror.M_eventmoment = new UnityEvent();
        //    SID_BoardManager_Mirror.M_eventmoment.AddListener(Reset);
        //}
        //else
        SID_BoardManager_Mirror.M_eventmoment.AddListener(Reset);
        PieceManager = SID_BoardPieceManager.instance;
        if (isWhite)
            whiteInt = 0;
        else
            whiteInt = 1;
    }

    public virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y) && MetricGameManager.M_vp != null)
        {
            Debug.Log("surfaces");
            PieceManager = SID_BoardPieceManager.instance;
            MetricGameManager.M_vp.AddListener(Reset);
        }
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
            //Debug.Log("Raycast Not Working");
            Debug.DrawRay(this.transform.position, -transform.up, Color.red, 1.1f);
        }
        scouting = PieceManager.coordinates;
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
        StartCoroutine(CalculateCurrentMoves());
    }
    public virtual void CalculateFutureMoves()
    {
        StartCoroutine(FutureSight(CurrentX, CurrentY));
        List<List<Points>> list = ListChange.SplitList<Points>(new List<Points>(confirmedMoves.Keys), confirmedMoves.Count);
    }
    public virtual IEnumerator CalculateCurrentMoves()
    {
        yield return null;
    }
    public virtual IEnumerator FutureSight(int curX, int curY)
    {
        yield return null;
    }
}

public static class ListChange
{
    public static List<List<Points>> SplitList<Points>(this List<Points> me, int size)
    {
        var list = new List<List<Points>>();
        for (int i = 0; i < me.Count; i += size)
        {
            list.Add(me.GetRange(i, Mathf.Min(size, me.Count - i)));
        }
        return list;
    }
}