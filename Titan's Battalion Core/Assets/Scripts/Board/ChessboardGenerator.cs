using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ChessboardGenerator : ChessGenerator
{
    [SerializeField] ChessPieceManager pieceManager;
    public static ChessboardGenerator Instance;


    [SerializeField] private List<Chessboard_Testing> mainBoardList = new List<Chessboard_Testing>();

    public delegate void ChangeValueDelegate(int NewX, int NewY);
    public ChangeValueDelegate ChangeValue;


    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {

    }

    private void Start()
    {

        if (IsServer)
            SetupTerritoryServerRpc();

    }
    [ServerRpc]
    void SetupTerritoryServerRpc()
    {
        if (mainBoardList.Count >= DataSend.mainBoardId + 1)
            chessboard = mainBoardList[DataSend.mainBoardId];
        Generator();
    }

    public void Generator()
    {
        SetMainBoardClientRpc();
                GenerateAllTilesServerRpc(0);
    }

    [ClientRpc]
    private void SetMainBoardClientRpc()
    {
        ChangeValue?.Invoke(chessboard.tileCountX, chessboard.tileCountY);
    }


    protected override GameObject GenerateSingleTile(ref int x, ref int y)
    {
        GameObject tileObject = Instantiate(piece);
        setupTiles.Add(new Points(x, y), tileObject);
        return tileObject;
    }
}