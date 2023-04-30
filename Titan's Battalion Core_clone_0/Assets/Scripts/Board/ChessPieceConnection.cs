using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class ChessPieceConnection : NetworkBehaviour
{

    public NetworkVariable<int> GridX = new NetworkVariable<int>(-1), GridY = new NetworkVariable<int>(-1);
    public List<Chesspiece> piecesThatHaveUsInCheck = new List<Chesspiece>();
    public GameObject pieceSpawnPoint;
    public Chesspiece occupiedChesspiece;
    [SerializeField] private MeshFilter setupMesh;
    public NetworkVariable<int> spawnTerritoryId = new NetworkVariable<int>(0);

    public bool IsInCheck(int team)
    {
        bool b = false;
        if (piecesThatHaveUsInCheck.Count > 0)
        {
            for (int i = 0; i < piecesThatHaveUsInCheck.Count; i++)
            {
                if (piecesThatHaveUsInCheck[i].team == team)
                    continue;
                else
                    b = true;
            }
        }
        return b;
    }

    [ClientRpc]
    public void SwapLayersClientRpc(string layerName, ClientRpcParams rpc = default)
    {
        gameObject.layer = LayerMask.NameToLayer(layerName);
    }

    [ClientRpc]
    public void GenerateCoordinatesClientRpc(int x, int y)
    {
        gameObject.name = string.Format("X:{0}, Y:{1}", x, y);
        pieceSpawnPoint.transform.position = new Vector3(x + .5f, 0 + .1f, y + .5f);
        GetComponent<BoxCollider>().center = new Vector3(x + .5f, 0, y + .5f);

        Mesh mesh = new Mesh();

        setupMesh.mesh = mesh;
        Vector3[] vertics = new Vector3[4];

        vertics[0] = new Vector3(x, 0, y);
        vertics[1] = new Vector3(x, 0, (y + 1));
        vertics[2] = new Vector3((x + 1), 0, y);
        vertics[3] = new Vector3((x + 1), 0, (y + 1));

        int[] triangles = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertics;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }

    public void ChangeGridValue(int x, int y, int id)
    {
        GridX.Value = x;
        GridY.Value = y;
        spawnTerritoryId.Value = id;
    }

    public Points CurrentTilePoint()
    {
        return new Points(GridX.Value, GridY.Value);
    }

    public Chesspiece GetOccupiedPiece() { return occupiedChesspiece; }

    public void SetOccupiedPiece(Chesspiece cp) => occupiedChesspiece = cp;


    public void AlterGrid(int x, int y)
    {
        GridX.Value += x;
        GridY.Value += y;

        ChessPieceManager.instance.AdjustPoints(GridX.Value, GridY.Value, this.gameObject);
    }
}
