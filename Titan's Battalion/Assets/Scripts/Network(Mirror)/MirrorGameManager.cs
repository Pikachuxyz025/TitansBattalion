using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using UnityEngine.UI;

public class MirrorGameManager : NetworkBehaviour
{
    public SID_BoardManager_Mirror SID_BM;
    public ArmyManager army;
    [HideInInspector] public static Dictionary<int, Field> curBoard;
    [HideInInspector] public static Dictionary<int, Army> curArmy;
    [HideInInspector] public Quaternion neutralCoordination = Quaternion.Euler(0, 0, 0), coordination = Quaternion.Euler(0, 180, 0);
    //[SerializeField] private GameObject player;
    [SyncVar]
    public bool armyIsSet;
    [SyncVar]//(hook = nameof(Board))]
    public GameObject varin;

    public BoardLocation BoLo;
    public Vector3[] buildPos = new Vector3[4];

    public PlayerInfo[] playerInfos = new PlayerInfo[2];

    public static UnityEvent M_Setup;
    [SyncVar]
    private int startup = 0;

    [SyncVar]
    [SerializeField] private bool SetupActive = true;

    //Start is called before the first frame update
    public override void OnStartServer()
    {
        curBoard = army.allBoards;
        curArmy = army.allArmies;
        RpcSetupCurInfo();
    }

    [ClientRpc]
    public void RpcSetupCurInfo()
    {
        curBoard = army.allBoards;
        curArmy = army.allArmies;
    }

    [Server]
    public void SelectBoard(int boardId)
    {
        //RpcSetupCurInfo();

        GameObject yo = Instantiate(curBoard[boardId - 1].fieldgrid, new Vector3(0, 0, 0), neutralCoordination);
        NetworkServer.Spawn(yo, connectionToClient);

        varin = yo;

        SID_BM.buildModeOn = true;
    }

    private void Update()
    {

        if (isServer)
        {
            RpcSetupCurInfo();

            if (SetupActive)
                ReadyToGo();
            if (armyIsSet == true)
            {
                SID_BM.buildModeOn = false;
                SID_BM.setActive = true;
                if (startup < 1)
                {
                    SID_BoardManager_Mirror.M_eventmoment.Invoke();
                    startup++;
                }
            }
        }
        //Debug.Log(SetupActive);
    }


    public void ReadyToGo()
    {
        armyIsSet = AllBoardsAreSet();
    }

    private bool AllBoardsAreSet()
    {
        for (int i = 0; i < playerInfos.Length; i++)
        {
            if (playerInfos[i].setToMatch == false)
                return false;
        }
        return true;
    }
}
