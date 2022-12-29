using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ChessPieceManager : NetworkBehaviour
{
    public static ChessPieceManager instance;
    public List<Points> PointList = new List<Points>();
    public List<Chesspiece> activeChesspieces = new List<Chesspiece>();
    public Dictionary<GameObject, Points> Pointers = new Dictionary<GameObject, Points>();
    public Dictionary<Points, GameObject> Tiles = new Dictionary<Points, GameObject>(new Points.EqualityComparer());
    public Dictionary<Points, Chesspiece> occupiedTiles = new Dictionary<Points, Chesspiece>(new Points.EqualityComparer());

    public Dictionary<SpecialMove, Chesspiece> spiecalPiecePoint = new Dictionary<SpecialMove, Chesspiece>();
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

    public void AddPoints(int x, int y, GameObject chesspiece)
    {
        Points newPoint = new Points(x, y);
        if (!Pointers.ContainsKey(chesspiece))
            Pointers.Add(chesspiece, newPoint);
        if (!Tiles.ContainsKey(newPoint))
            Tiles.Add(newPoint, chesspiece);
        //PointList.Add(newPoint);
    }

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

    public void CreatePointList()
    {
        PointList = new List<Points>(Pointers.Values);
    }

    public void AdjustPoints(int x, int y, GameObject chesspiece)
    {
        Points newPoint = new Points(x, y);
        if (!Pointers.ContainsKey(chesspiece))
            return;
        if (!Tiles.ContainsKey(newPoint))
        {
            Tiles.Remove(Pointers[chesspiece]);
            Tiles.Add(newPoint, chesspiece);
            Debug.Log(Tiles.Count);
        }
        else
            Tiles[newPoint] = chesspiece;
        Pointers[chesspiece] = newPoint;
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
            return;
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