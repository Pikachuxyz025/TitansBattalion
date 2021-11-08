using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SID_Chessman : MonoBehaviour
{
    public int CurrentX, CurrentY;
    public int rig;
    public bool isWhite, curOnMainBoard;
    //public float range;
    protected Dictionary<SID_BoardGridSet,Points> scouting = new Dictionary<SID_BoardGridSet,Points>();
    public Dictionary<Points, bool> confirmation = new Dictionary<Points, bool>(new Points.EqualityComparer());

    public virtual void Start()
    {
        SID_BoardManager.M_eventmoment.AddListener(Reset);
    }
    public virtual void Update()
    {
        scouting = SID_BoardManager.coordinates;
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