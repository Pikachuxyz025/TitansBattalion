using System.Collections;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : NetworkBehaviour
{
    [SerializeField] private GameObject playerUI;
    [SerializeField] private Player controllingPlayer;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private TextMeshProUGUI whoWin;
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private GameObject checkmateButton;
    [SerializeField]    private GameManager gameManager;

    public NetworkVariable<bool> retryBool = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> endBool = new NetworkVariable<bool>(false);

    public void SetupTerritory()
    {
        if (controllingPlayer.GetArmyBoardList().Count >= dropdown.value)
            controllingPlayer.SetChessIdServerRpc(dropdown.value);
        else
            return;

        if (IsOwner)
            controllingPlayer.SetModeChangeServerRpc(SetMode.NotSpawned);
    }

    [ClientRpc]
    public void ActivateCheckmateButtonClientRpc(bool set, ClientRpcParams clientRpc = default) => checkmateButton.SetActive(set);



    [ServerRpc(RequireOwnership = false)]
    public void ChangeRetyBoolServerRpc(bool value)
    {
        retryBool.Value = value;
        gameManager.GameRestart();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeEndBoolServerRpc(bool value)
    {
        endBool.Value = value;
        gameManager.GameEnd();
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

    [ServerRpc]
    public void OnCheckMateServerRpc()
    {
        gameManager.CheckGameOver(controllingPlayer);
    }
}
