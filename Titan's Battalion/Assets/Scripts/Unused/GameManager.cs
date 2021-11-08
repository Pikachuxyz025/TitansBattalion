using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public SID_BoardManager SID_BM;
    public ArmyManager army;
    public GameObject BoardMenu,ArmyMenu;
    public Text playerturn;
    public Canvas can;
    public static int turn; 
    private Dictionary<int, Field> curBoard;
    private Dictionary<int, Army> curArmy;
    private Quaternion neutralCoordination = Quaternion.Euler(0, 0, 0), coordination = Quaternion.Euler(0, 180, 0);
    private bool AisSet, BisSet;

    //Start is called before the first frame update
    void Start()
    {
        curBoard = army.allBoards;
        curArmy = army.allArmies;
        turn = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (SID_BM.buildModeOn)
            ArmyMenu.SetActive(true);
        else
            ArmyMenu.SetActive(false);
        if (SID_BM.isWhiteTurn)
            playerturn.text = "Player 1";
        else
            playerturn.text = "Player 2";
    }
    
    public void SelectBoard(int boardId)
    {
        GameObject yo = Instantiate(curBoard[boardId].fieldgrid, new Vector3(0, 0, 0), neutralCoordination);
        SID_BM.SetupBoard();
        BoardMenu.SetActive(false);
        SID_BM.buildModeOn = true;
    }

    public void SelectArmy(int armyId)
    {
        GameObject yo;
        if (SID_BM.isWhiteTurn)
        {
            if (!AisSet)
            {
                yo = Instantiate(curArmy[armyId].armyGrid, SID_BM.buildPos[0], neutralCoordination) as GameObject;
                SID_BoardGridSet[] children = yo.GetComponentsInChildren<SID_BoardGridSet>();
                foreach (SID_BoardGridSet item in children)
                {
                    if (item.startingPieceOrigin == BoardStartPoint.StartingPiecePlayerTwo) 
                    {
                        SID_BM.originBoardPiece[0] = item.gameObject;
                        SID_BM.yo = yo;
                        AisSet = true;
                    }
                }
            }
        }
        else
        {
            if (!BisSet)
            {
                yo = Instantiate(curArmy[armyId].armyGrid, SID_BM.buildPos[2] + new Vector3(0, 0, curArmy[armyId].armyOffset), neutralCoordination) as GameObject;
                SID_BoardGridSet[] children = yo.GetComponentsInChildren<SID_BoardGridSet>();
                foreach (SID_BoardGridSet item in children)
                {
                    if (item.startingPieceOrigin == BoardStartPoint.StartingPiecePlayerTwo)
                    {
                        SID_BM.originBoardPiece[1] = item.gameObject;
                        SID_BM.yo = yo;
                        BisSet = true;
                    }
                }
            }
        }
    }
}