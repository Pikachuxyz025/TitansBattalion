using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class ChessPieceConnection : NetworkBehaviour
{

    public NetworkVariable<int> GridX = new NetworkVariable<int>(-1), GridY = new NetworkVariable<int>(-1);
    public List<Chesspiece> inCheck = new List<Chesspiece>();
    public bool isConnected = false;
    public Pawn SkippedPawnd = null;
    public GameObject pieceSetPoint;
    public Chesspiece occupiedChesspiece;
    [SerializeField] private MeshFilter setupMesh;
    public NetworkVariable<bool> n_isConnected = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> spawnTerritoryId = new NetworkVariable<int>(0);

    private void Awake()
    {
        n_isConnected.OnValueChanged += ChangeIsConnected;
    }

    private void ChangeIsConnected(bool previousValue, bool newValue)
    {
        isConnected = newValue;
    }
    public bool IsInCheck(int team)
    {
        bool b = false;
        if (inCheck.Count > 0)
        {
            for (int i = 0; i < inCheck.Count; i++)
            {
                if (inCheck[i].team == team)
                    continue;
                else
                    b = true;
            }
        }
        return b;
    }
    public void ConfigureBoard()
    {
        isConnected = true;
    }


    [ClientRpc]
    public void SwapLayersClientRpc(string c, ClientRpcParams rpc = default)
    {
        gameObject.layer = LayerMask.NameToLayer(c);
    }

    public void SwapLayers(string c) => gameObject.layer = LayerMask.NameToLayer(c);


    [ClientRpc]
    public void GenerateCoordinatesClientRpc(int x, int y)
    {
        gameObject.name = string.Format("X:{0}, Y:{1}", x, y);
        pieceSetPoint.transform.position = new Vector3(x + .5f, 0 + .1f, y + .5f);
        GetComponent<BoxCollider>().center = new Vector3(x + .5f, 0, y + .5f);

        Mesh mesh = new Mesh();

        setupMesh.mesh = mesh;
        Vector3[] vertics = new Vector3[4];

        vertics[0] = new Vector3(x * 1, 0, y * 1);
        vertics[1] = new Vector3(x * 1, 0, (y + 1) * 1);
        vertics[2] = new Vector3((x + 1) * 1, 0, y * 1);
        vertics[3] = new Vector3((x + 1) * 1, 0, (y + 1) * 1);

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertics;
        mesh.triangles = tris;

        mesh.RecalculateNormals();
        //ChessPieceManager.instance.AddPoints(x, y, gameObject);
    }

    public void ChangeGridValue(int x, int y, int id)
    {
        GridX.Value = x;
        GridY.Value = y;
        spawnTerritoryId.Value = id;
    }
    public void GenerateCoordinates(int x, int y)
    {
        GridX.Value = x;
        GridY.Value = y;
        gameObject.name = string.Format("X:{0}, Y:{1}", x, y);
        pieceSetPoint.transform.position = new Vector3(x + .5f, 0 + .1f, y + .5f);
        GetComponent<BoxCollider>().center = new Vector3(x + .5f, 0, y + .5f);

        Mesh mesh = new Mesh();

        setupMesh.mesh = mesh;
        Vector3[] vertics = new Vector3[4];

        vertics[0] = new Vector3(x * 1, 0, y * 1);
        vertics[1] = new Vector3(x * 1, 0, (y + 1) * 1);
        vertics[2] = new Vector3((x + 1) * 1, 0, y * 1);
        vertics[3] = new Vector3((x + 1) * 1, 0, (y + 1) * 1);

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertics;
        mesh.triangles = tris;

        mesh.RecalculateNormals();
        //ChessPieceManager.instance.AddPoints(x, y, gameObject);
    }

    public Points CurrentTilePoint()
    {
        return new Points(GridX.Value, GridY.Value);
    }

    public Chesspiece GetOccupiedPiece() { return occupiedChesspiece; }


    [ClientRpc]
    public void SetOccupiedPieceClientRpc(NetworkObjectReference target)
    {
        if (target.TryGet(out NetworkObject targetObject))
        {
            Chesspiece cp = targetObject.gameObject.GetComponent<Chesspiece>();
            occupiedChesspiece = cp;
        }
    }

    public void SetOccupiedPiece(Chesspiece cp) => occupiedChesspiece = cp;


    public void AlterGrid(int x, int y)
    {
        GridX.Value += x;
        GridY.Value += y;

        ChessPieceManager.instance.AdjustPoints(GridX.Value, GridY.Value, this.gameObject);
    }
}
