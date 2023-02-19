using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    
    public TMP_Dropdown armySelection;
    public GameObject _readyButton;
    public static GameUIManager instance;

    private void Awake()
    {
        instance = this;
        SetOptions(armySelection, Contants.Armies);
    }

    public void SetOptions(TMP_Dropdown dropdown, IEnumerable<string> values)
    {
        dropdown.options = values.Select(type => new TMP_Dropdown.OptionData { text = type }).ToList();
    }

    public int GrabDropDownIndex()
    {
        return armySelection.value;
    }
}
