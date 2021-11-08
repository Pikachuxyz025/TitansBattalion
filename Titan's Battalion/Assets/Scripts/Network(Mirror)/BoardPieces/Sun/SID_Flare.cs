using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SID_Flare : SID_Chessman_Mirror
{
    public List<Points> siD;
    public List<Points> seD;
    public List<SID_Suns> suns;


    public override void Awake()
    {
        base.Awake();
        if (isWhite)
        {
            foreach (GameObject sun in SID_BoardManager_Mirror.Instance.p1chessmanPrefabs)
            {
                if (sun.GetComponent<SID_Chessman_Mirror>().GetType().ToString() == "SID_Suns")
                {
                    SID_Suns catchy = sun.GetComponent<SID_Chessman_Mirror>() as SID_Suns;
                    if (!suns.Contains(catchy))
                        suns.Add(catchy);
                }
            }
        }
        else
        {
            foreach (GameObject sun in SID_BoardManager_Mirror.Instance.p2chessmanPrefabs)
            {
                if (sun.GetComponent<SID_Chessman_Mirror>().GetType().ToString() == "SID_Suns")
                {
                    SID_Suns catchy = sun.GetComponent<SID_Chessman_Mirror>() as SID_Suns;
                    if (!suns.Contains(catchy))
                        suns.Add(catchy);
                }
            }
        }
    }
    public override void FindPossiblilties()
    {
        base.FindPossiblilties();
        CalculateOrbitMoves();
    }
    public override void Update()
    {
        base.Update();
        if (rig < 1)
        {
            FindPossiblilties();
        }
        siD = new List<Points>(confirmedMoves.Keys);
        seD = new List<Points>(futureMoves.Keys);
    }

    public void FlareMove(int x, int y, ref Dictionary<Points, bool> confirmation)
    {
        bool r = new bool();
        Points simple = new Points(x, y);
        PieceState pieceStateSimple = PieceManager.VaildatePieces(simple.X, simple.Y, this);

        if (pieceStateSimple == PieceState.Free)
            r = true;
        else if (pieceStateSimple == PieceState.Enemy)
        {
            r = true;
            SID_Chessman_Mirror chessPiece = PieceManager.FindChessman(simple.X, simple.Y);
            if (chessPiece.GetType().ToString() == "SID_King_Mirror")
            {
                Debug.Log("CheckPoint");
                SID_King_Mirror king = chessPiece as SID_King_Mirror;
                if (confirmation == confirmedMoves)
                    king.checkers = CheckState.inCheck;
                else if (confirmation == futureMoves)
                    king.checkers = CheckState.inCheckZone;
            }
        }
        if (!confirmation.ContainsKey(simple))
            confirmation.Add(simple, r);
    }

    public void OrbitMove(int x, int y, ref Dictionary<Points, bool> confirmation)
    {
        bool r = new bool();
        Points simple = new Points(x, y);
        PieceState pieceStateSimple = PieceManager.VaildatePieces(simple.X, simple.Y, this);

        if (pieceStateSimple == PieceState.Free)
            r = true;

        if (!confirmation.ContainsKey(simple))
            confirmation.Add(simple, r);
    }

    public override void CalculateFutureMoves()
    {
        futureMoves.Clear();
        foreach (Points coord in new List<Points>(confirmedMoves.Keys))
        {
            if (confirmedMoves[coord])
                StartCoroutine(FutureSight(coord.X, coord.Y));
        }
    }

    public void CalculateOrbitMoves()
    {
        foreach(SID_Suns sun in suns)
        {
            StartCoroutine(OrbitOptions(sun.CurrentX, sun.CurrentY, OrbitPathX(sun.CurrentX), OrbitPathY(sun.CurrentY)));
        }
    }

    public override IEnumerator CalculateCurrentMoves()
    {
        confirmedMoves.Clear();
        yield return new WaitForSeconds(.1f);
        FlareMove(CurrentX - 1, CurrentY + 2, ref confirmedMoves);
        FlareMove(CurrentX + 1, CurrentY + 2, ref confirmedMoves);
        FlareMove(CurrentX + 1, CurrentY - 2, ref confirmedMoves);
        FlareMove(CurrentX - 1, CurrentY - 2, ref confirmedMoves);
        FlareMove(CurrentX - 2, CurrentY + 1, ref confirmedMoves);
        FlareMove(CurrentX + 2, CurrentY - 1, ref confirmedMoves);
        FlareMove(CurrentX - 2, CurrentY - 1, ref confirmedMoves);
        FlareMove(CurrentX + 2, CurrentY + 1, ref confirmedMoves);
        FlareMove(CurrentX - 2, CurrentY - 2, ref confirmedMoves);
        FlareMove(CurrentX + 2, CurrentY - 2, ref confirmedMoves);
        FlareMove(CurrentX - 2, CurrentY + 2, ref confirmedMoves);
        FlareMove(CurrentX + 2, CurrentY + 2, ref confirmedMoves);
        FlareMove(CurrentX - 2, CurrentY - 2, ref confirmedMoves);
        FlareMove(CurrentX + 2, CurrentY - 2, ref confirmedMoves);
        FlareMove(CurrentX - 2, CurrentY + 2, ref confirmedMoves);
        FlareMove(CurrentX + 2, CurrentY + 2, ref confirmedMoves);
        FlareMove(CurrentX + 2, CurrentY, ref confirmedMoves);
        FlareMove(CurrentX - 2, CurrentY, ref confirmedMoves);
        FlareMove(CurrentX, CurrentY + 2, ref confirmedMoves);
        FlareMove(CurrentX, CurrentY - 2, ref confirmedMoves);
        CalculateFutureMoves();
        yield return rig += 1;
    }

    public override IEnumerator FutureSight(int curX, int curY)
    {
        yield return new WaitForSeconds(.1f);
        FlareMove(curX - 1, curY + 2, ref futureMoves);
        FlareMove(curX + 1, curY + 2, ref futureMoves);
        FlareMove(curX + 1, curY - 2, ref futureMoves);
        FlareMove(curX - 1, curY - 2, ref futureMoves);
        FlareMove(curX - 2, curY + 1, ref futureMoves);
        FlareMove(curX + 2, curY - 1, ref futureMoves);
        FlareMove(curX - 2, curY - 1, ref futureMoves);
        FlareMove(curX + 2, curY + 1, ref futureMoves);
        FlareMove(curX + 2, curY + 2, ref futureMoves);
        FlareMove(curX - 2, curY + 2, ref futureMoves);
        FlareMove(curX + 2, curY - 2, ref futureMoves);
        FlareMove(curX - 2, curY - 2, ref futureMoves);
        FlareMove(curX - 2, curY, ref futureMoves);
        FlareMove(curX + 2, curY, ref futureMoves);
        FlareMove(curX, curY - 2, ref futureMoves);
        FlareMove(curX, curY + 2, ref futureMoves);
    }

    public IEnumerator OrbitOptions(int sunCurX, int sunCurY, int curX, int curY)
    {
        yield return new WaitForSeconds(.1f);
        OrbitMove(sunCurX + curX, sunCurY + curY, ref confirmedMoves);
        OrbitMove(sunCurX - curX, sunCurY + curY, ref confirmedMoves);
        OrbitMove(sunCurX + curX, sunCurY - curY, ref confirmedMoves);
        OrbitMove(sunCurX - curX, sunCurY - curY, ref confirmedMoves);
        OrbitMove(sunCurX + curY, sunCurY + curX, ref confirmedMoves);
        OrbitMove(sunCurX - curY, sunCurY + curX, ref confirmedMoves);
        OrbitMove(sunCurX + curY, sunCurY - curX, ref confirmedMoves);
        OrbitMove(sunCurX - curY, sunCurY - curX, ref confirmedMoves);
    }
    public int OrbitPathX(int sotX)
    {
        int setX = 2;
        setX = Mathf.Abs(sotX - CurrentX);
        return setX;
    }
    public int OrbitPathY(int sotY)
    {
        int setY = 2;
        setY = Mathf.Abs(sotY - CurrentY);
        return setY;
    }
}
