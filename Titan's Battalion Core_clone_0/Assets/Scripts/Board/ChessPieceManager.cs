using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class ChessPieceManager : NetworkBehaviour
{
    public static ChessPieceManager instance;
    public List<Points> PointList = new List<Points>();
    public List<GameObject> ObjectList=new List<GameObject> ();
    public List<Chesspiece> activeChesspieces = new List<Chesspiece>();
    public Dictionary<GameObject, Points> Pointers = new Dictionary<GameObject, Points>();
    public Dictionary<Points, GameObject> Tiles = new Dictionary<Points, GameObject>(new Points.EqualityComparer());
    public Dictionary<Points, Chesspiece> occupiedTiles = new Dictionary<Points, Chesspiece>(new Points.EqualityComparer());
    // Start is called before the first frame update
    void Awake()
    {
        //Debug.Log("Show me tis");
        instance = this;
        //Instanced();
    }

    public override void OnNetworkSpawn()
    {
        //Debug.Log("Show me tid");
        instance = this;
        //Instanced();
    }

    public void Instanced()
    {
        //Debug.Log("Show me tisd");
        instance = this;
    }

    #region Create Coordinate System

    public void AddPoints(int x, int y, GameObject chesspiece)
    {
        Points newPoint = new Points(x, y);
        if (!Pointers.ContainsKey(chesspiece))
            Pointers.Add(chesspiece, newPoint);
    }

    public void AdjustPoints(int x, int y, GameObject chesspiece)
    {
        Points newPoint = new Points(x, y);
        #region Previous Iteration
        //basic setup didn't work all the way
        /*if (Tiles.ContainsKey(newPoint))
        {
            Debug.Log(chesspiece.name + "'s changed location: " + newPoint.X + ", " + newPoint.Y);
            Tiles[newPoint] = chesspiece;
        }
        else if (!Tiles.ContainsKey(newPoint))
        {
            Debug.Log("Before " + Tiles.Count);
            Tiles.Remove(Pointers[chesspiece]);
            Debug.Log(chesspiece.name + "'s new location: " + newPoint.X + ", " + newPoint.Y);
            Tiles.Add(newPoint, chesspiece);
            Debug.Log("Same " + Tiles.Count);
        }*/

        // reversal effect didn't work
        /*Tiles.Clear();
        Tiles = Pointers.Reverse();
        Debug.Log("Same " + Tiles.Count);
        PointList = new List<Points>(Tiles.Keys);
        ObjectList=new List<GameObject>(Tiles.Values);*/
        #endregion
        Pointers[chesspiece] = newPoint;
    }

    public void SetupTiles()
    {
        foreach (GameObject tile in Pointers.Keys)
        {
            if (!Tiles.ContainsKey(Pointers[tile]))
                Tiles.Add(Pointers[tile], tile);
        }
    }

    #endregion

    public GameObject GetChesspieceGameObject(Points currentPoint)
    {
        // check if tile is here
        if (!Tiles.ContainsKey(currentPoint))
            return null;
        return Tiles[currentPoint];
    }

    public ChessPieceConnection GetChesspieceConnection(Points currentPoint)
    {
        // check if tile is here
        if (!Tiles.ContainsKey(currentPoint))
            return null;
        return Tiles[currentPoint].GetComponent<ChessPieceConnection>();
    }

    public Chesspiece GetOccupiedPiece(Points currentPoint)
    {
        return GetChesspieceConnection(currentPoint).occupiedChesspiece;
    }

    public bool IsOccupied(Points currentPoint)
    {
        if (!Tiles.ContainsKey(currentPoint))
            return false;

        ChessPieceConnection connection = GetChesspieceConnection(currentPoint);
        if (connection.occupiedChesspiece != null)
            return true;
        return false;
    }

    public Vector3 GetNewPiecePosition(Points currentPoint)
    {
        if (!Tiles.ContainsKey(currentPoint))
            return Vector3.zero;
        Vector3 newPosition = Tiles[currentPoint].GetComponent<ChessPieceConnection>().pieceSetPoint.transform.position;
        return newPosition;
    }

    public bool IsCoordinateInList(Points currentPoint)
    {
        return Tiles.ContainsKey(currentPoint);
    }



    public void SetActiveMoveList()
    {
        foreach (Chesspiece cp in activeChesspieces)
        {
            if (cp is Pawn)
            {
                Pawn pawnd = cp.GetComponent<Pawn>();
                pawnd.AddToMoveList();
            }
        }
    }

    public void SetTilesInCheck()
    {
        ResetTiles();

        foreach (Chesspiece cp in activeChesspieces)
        {
            cp.GetAvailableMoves();
        }
    }

    public void ResetTiles()
    {
        foreach (GameObject conn in Tiles.Values)
            conn.GetComponent<ChessPieceConnection>().inCheck.Clear();

    }

    [ServerRpc(RequireOwnership = false)]
    public void SwapLayerServerRpc(int x, int y, string c, ulong s = 0)
    {
        ClientRpcParams clientRpcParams;
        if (s != 0)
        {
            clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {

                    TargetClientIds = new ulong[] { s }
                }
            };
        }
        else
        {
            clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {

                    TargetClientIds = new ulong[] { default }
                }
            };
        }

        Points currentPoint = new Points(x, y);
        if (!Tiles.ContainsKey(currentPoint))
        {
            Debug.Log(currentPoint.X + ", " + currentPoint.Y + " isn't here");
            return;
        }
        GetChesspieceConnection(currentPoint).SwapLayersClientRpc(c, clientRpcParams);
    }

    public GameObject SpawnSinglePiece(GameObject reference, int team)
    {
        GameObject _spawnedObject = Instantiate(reference);
        Chesspiece chesspiece = _spawnedObject.GetComponent<Chesspiece>();

        chesspiece.team = team;

        return _spawnedObject;
    }

    public void PositionSinglePiece(Chesspiece cp, ChessPieceConnection dp)
    {
        cp.currentY = dp.GridY.Value;
        cp.currentX = dp.GridX.Value;
        Vector3 pos = dp.pieceSetPoint.transform.position;

        cp.ReturnPositionServerRpc(pos);
    }

    public void PositionSinglePiece(Chesspiece cp, Points pd)
    {
        ChessPieceConnection dp = GetChesspieceConnection(pd);
        cp.currentY = dp.GridY.Value;
        cp.currentX = dp.GridX.Value;
        Vector3 pos = dp.pieceSetPoint.transform.position;

        if (cp is Pawn)
        {
            Pawn pawnd = cp.GetComponent<Pawn>();
            pawnd.ConvertToQueenServerRpc();
        }
        cp.ReturnPositionServerRpc(pos);
    }
}

public static class Extensions
{
    public static Dictionary<V, K> Reverse<K, V>(this IDictionary<K, V> dict)
    {
        return dict.ToDictionary(x => x.Value, x => x.Key);
    }
}