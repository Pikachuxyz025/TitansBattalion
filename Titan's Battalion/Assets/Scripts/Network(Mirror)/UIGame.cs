using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIGame : MonoBehaviour
{
    public Image checkpoint;
    public Text checkpointText, timerText, victoryText;
    public int endingTimer;
    public Button surrender, rematch, noRematch;
    public Toggle[] playerReady;
    public GameObject curGameUI, endGameUI;
    [SerializeField] MirrorGameManager MgM;
    [SerializeField] CheckState curKingState;
    [SerializeField] SID_King_Mirror playerKing;
    public PlayerInfo player;

    // Start is called before the first frame update
    private void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player.mirrorGameManager == null)
            return;
        MgM = player.mirrorGameManager;
        ActiveUI();
        if (player.myKing == null)
        {
            playerKing = null;
            return;
        }
        if (curGameUI.activeSelf)
        {
            playerKing = player.myKing;
            curKingState = playerKing.checkers;

            switch (curKingState)
            {
                case CheckState.Safe:
                    SafeZone();
                    break;
                case CheckState.inCheck:
                    CheckMateWarning();
                    break;
                case CheckState.inCheckZone:
                    CheckZoneWarning();
                    break;
            }
        }
    }

    public void GoodForRematch()
    {
        Debug.Log("Good for rematch");
    }

    public void NoRematch()
    {
        Debug.Log("no rematch");
    }

    void ActiveUI()
    {
        if (MgM.currentState == GameState.Ongoing || MgM.currentState == GameState.GracePeriod)
            curGameUI.SetActive(true);
        else
            curGameUI.SetActive(false);
        if (MgM.currentState == GameState.Stalemate || MgM.currentState == GameState.Check)
            endGameUI.SetActive(true);
        else
            endGameUI.SetActive(false);
    }

    public void RematchIndicationPlayer() => playerReady[1].isOn = false;
    public void NoRematchIndicationPlayer() => playerReady[0].isOn = false;
    /*public void NoRematchIndicationPlayerTwo() => playerReadyTwo[0].isOn = false;
    public void RematchIndicationPlayerTwo() => playerReadyTwo[1].isOn = false;*/

    public int RematchPlayer()
    {
        int setpiece;
        setpiece = 0;
        for (int i = 0; i < playerReady.Length; i++)
        {
            if (playerReady[0].isOn)
                setpiece = 1;
            else if (playerReady[1].isOn)
                setpiece = -2;
        }
        return setpiece;
    }

    void SafeZone()
    {
        StopAllCoroutines();
        checkpointText.text = "SAFE";
        checkpoint.color = Color.white;
    }
    void CheckZoneWarning()
    {
        checkpoint.color = Color.yellow;
        checkpointText.text = "IN CHECKZONE!";
        StartCoroutine(CheckWarningOne(.5f));
    }
    void CheckMateWarning()
    {
        checkpoint.color = Color.red;
        checkpointText.text = "CHECKMATE IMMINATE!";
        StartCoroutine(CheckWarningOne(.25f));
    }

    IEnumerator CheckWarningOne(float seting)
    {
        while (true)
        {
            switch (checkpoint.color.a.ToString())
            {
                case "0":
                    checkpoint.color = new Color(checkpoint.color.r, checkpoint.color.g, checkpoint.color.b, 1);
                    yield return new WaitForSeconds(seting);
                    break;
                case "1":
                    checkpoint.color = new Color(checkpoint.color.r, checkpoint.color.g, checkpoint.color.b, 0);
                    yield return new WaitForSeconds(seting);
                    break;
            }
        }
    }

    /*public int RematchPlayerTwo()
    {
        int setpiece;
        setpiece = 0;
        for (int i = 0; i < playerReadyOne.Length; i++)
        {
            if (playerReadyTwo[0].isOn)
                setpiece = 1;
            else if (playerReadyTwo[1].isOn)
                setpiece = -2;
        }
        return setpiece;
    }*/

}
