using System.Collections;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum SetMode
{
    SelectArmy,
    NotSpawned,
    FlipMode,
    Spawned,
    Set,
    GameOver,
}

public class Player : NetworkBehaviour
{
    [Header("Editor Filled Variables")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ChessPieceManager pieceManager;

    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private TextMeshProUGUI whoWin;
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private GameObject checkmateButton;
    [SerializeField] private List<ChessboardTemplate> armyBoardList = new List<ChessboardTemplate>();

    [Header("board check")]
    [SerializeField] private GameModeTerritory gameModeTerritory = new GameModeTerritory();

    [Header("Script Filled Variables")]
    [SerializeField] private Camera currentCamera;

    public CheckableList playerCheckableList { get; private set; } = new CheckableList();


    public NetworkVariable<int> teamNumber = new NetworkVariable<int>(0);


    public NetworkVariable<bool> NetworkRetryBool = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> NetworkEndBool = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> NetworkIsKingInCheck = new NetworkVariable<bool>(false);
    public King currentKing { get; private set; }

    [SerializeField] private Vector3[] setCameraTransform;
    [SerializeField] private Vector3[] setCameraRotation;
    public NetworkVariable<SetMode> currentSetModeNet = new NetworkVariable<SetMode>(SetMode.SelectArmy);
    public NetworkVariable<GameMode> currentGameMode = new NetworkVariable<GameMode>(GameMode.None);

    public NetworkVariable<bool> NetworkIsMyTurn = new NetworkVariable<bool>(false);

    [SerializeField] private Interaction interaction;
    [SerializeField] private PlayerUI _playerUI;
    [SerializeField] private PlayerTerritorySpawn playerTerritorySpawn;

    public bool isKingInCheck
    {
        get { return NetworkIsKingInCheck.Value; }
        private set { }
    }

    public void SetupVariables(GameMode _mode, int _teamNumber)
    {
        currentGameMode.Value = _mode;
        teamNumber.Value = _teamNumber;
    }

    public PlayerTerritorySpawn GetPlayerTerritorySpawn() { return playerTerritorySpawn; }


    [ServerRpc(RequireOwnership = false)]
    public void EnterTurnServerRpc(bool previous,bool current)
    {
        if (gameManager.HasGameStarted()&& current)
            ActiveCheckMateServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActiveCheckMateServerRpc() // player should keep this script to connect with game manager and player ui 
    {
        ClientRpcParams clientRpcParams;

        clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {

                TargetClientIds = new ulong[] { OwnerClientId }
            }
        };

        if (currentKing == null)
            return;
        if (currentKing.CompleteKingCheckmate() && !AnyMovesRemain())
        {
            gameManager.CheckGameOver(this);
            return;
        }

        if (currentKing.SetCheckableList())
        {
            ActivateCheckmateButtonClientRpc(true, clientRpcParams);
            NetworkIsKingInCheck.Value = true;
        }
        else
            ActivateCheckmateButtonClientRpc(false, clientRpcParams);

    }

    private bool AnyMovesRemain()
    {
        List<Chesspiece> remainingPieces = pieceManager.CurrentPlayerPieces(this);
        for (int i = 0; i < remainingPieces.Count; i++)
        {
            List<Points> availableMoves = new List<Points>();
            availableMoves.AddRange(playerCheckableList.GetDownMyKing(remainingPieces[i].GetAvailableMoves(), currentKing.GetCurrentPosition(), remainingPieces[i]));
            availableMoves.AddRange(playerCheckableList.GetDownMyKing(remainingPieces[i].GetSpecialMoves(), currentKing.GetCurrentPosition(), remainingPieces[i]));
            if (availableMoves.Count > 0)
                return true;
        }
        return false;
    }

    [ClientRpc]
    public void ActivateCheckmateButtonClientRpc(bool set, ClientRpcParams clientRpc = default) => checkmateButton.SetActive(set);


    [ServerRpc(RequireOwnership = false)]
    public void ExitTurnServerRpc() { }

    public void CameraSetup()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            interaction.SetCurrentCamera(currentCamera);
            if (IsOwner)
            {
                switch (teamNumber.Value)
                {
                    case 1:
                        currentCamera.transform.position = setCameraTransform[0];
                        currentCamera.transform.rotation = Quaternion.Euler(setCameraRotation[0].x, setCameraRotation[0].y, setCameraRotation[0].z);
                        break;
                    case 2:
                        currentCamera.transform.position = setCameraTransform[1];
                        currentCamera.transform.rotation = Quaternion.Euler(setCameraRotation[1].x, setCameraRotation[1].y, setCameraRotation[1].z);
                        break;
                    case 3:
                        currentCamera.transform.position = setCameraTransform[2];
                        currentCamera.transform.rotation = Quaternion.Euler(setCameraRotation[2].x, setCameraRotation[2].y, setCameraRotation[2].z);
                        break;
                    case 4:
                        currentCamera.transform.position = setCameraTransform[3];
                        currentCamera.transform.rotation = Quaternion.Euler(setCameraRotation[3].x, setCameraRotation[3].y, setCameraRotation[3].z);
                        break;
                }
            }
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        CameraSetup();
    }

    #region Starting The Game

    public override void OnNetworkSpawn()
    {
        NetworkIsMyTurn.OnValueChanged += interaction.OnMyTurnChanged;
        NetworkIsMyTurn.OnValueChanged += EnterTurnServerRpc;

        Setupd();

        if (currentGameMode.Value == GameMode.None)
            return;


        if (currentGameMode.Value != GameMode.Chess)
        {
            if (IsOwner)
                playerUI.SetActive(true);
        }
        else
        {
            SetChessIdServerRpc(currentGameMode.Value);
            SetModeChangeServerRpc(SetMode.NotSpawned);
        }

    }

    // Finds the gamemanager in scene (Server only)
    private void Setupd()
    {
        gameManager = GameManager.instance;
        pieceManager = ChessPieceManager.instance;
    }

    // Takes the dropdown value of the armyboard list and selects territory
    // Attached to button press in game
    public void SetupTerritory()
    {
        if (armyBoardList.Count >= dropdown.value)
        {
            SetChessIdServerRpc(dropdown.value);
        }
        else
            return;

        if (IsOwner)
            SetModeChangeServerRpc(SetMode.NotSpawned);
    }

    public List<ChessboardTemplate> GetArmyBoardList()
    {
        return armyBoardList;
    }


    [ServerRpc(RequireOwnership = false)]
    public void SetChessIdServerRpc(int id) => playerTerritorySpawn.SetChessboardTemplate(armyBoardList[id]);


    [ServerRpc(RequireOwnership = false)]
    public void SetChessIdServerRpc(GameMode mode) => playerTerritorySpawn.SetChessboardTemplate(gameModeTerritory.GetTerritory(mode));


    public void SetCurrentKing(King king)
    { currentKing = king; }
    #endregion


    #region Ending The Game

    [ServerRpc(RequireOwnership = false)]
    public void ChangeRetyBoolServerRpc(bool value)
    {
        NetworkRetryBool.Value = value;
        gameManager.GameRestart();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeEndBoolServerRpc(bool value)
    {
        NetworkEndBool.Value = value;
        gameManager.GameEnd();
    }

    [ServerRpc]
    public void OnCheckMateServerRpc() // it's tied to a button in game
    {
        gameManager.CheckGameOver(this);
    }

    [ClientRpc]
    public void CheckMateClientRpc(int team)
    {
        if (IsOwner)
            DisplayVictory(team);
    }

    private void DisplayVictory(int winningTeam)
    {
        checkmateButton.gameObject.SetActive(false);
        winScreen.SetActive(true);
        whoWin.text = new string("Player " + winningTeam + " Wins!");
    }
    #endregion

    [ServerRpc(RequireOwnership = false)]
    public void SetModeChangeServerRpc(SetMode set) => currentSetModeNet.Value = set;
}
