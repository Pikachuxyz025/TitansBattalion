using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct LobbyData
{
    public string Name;
    public int MaxPlayers;
    public int MainBoard;
}

public struct GameData
{
    public ulong playerId;
    public int MainBoard;
    public int ArmyBoard;
}

public class CreateLobbyScreen : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nameInput, _maxPlayersInput;
    [SerializeField] private TMP_Dropdown armyBoardDropdown, mainBoardDropdown;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI sliderText;
    public static event Action<LobbyData> LobbyCreated;

    private void Start()
    {
        slider.onValueChanged.AddListener((v) => sliderText.text = v.ToString("0"));
        SetOptions(mainBoardDropdown, Contants.MainBoards);
        void SetOptions(TMP_Dropdown dropdown, IEnumerable<string> values)
        {
            dropdown.options = values.Select(type => new TMP_Dropdown.OptionData { text = type }).ToList();
        }
    }

    public void OnCreateClicked()
    {
        var lobbyData = new LobbyData
        {
            Name = _nameInput.text,
            MaxPlayers = (int)slider.value, //int.Parse(_maxPlayersInput.text),
            MainBoard = mainBoardDropdown.value,
        };

        LobbyCreated?.Invoke(lobbyData);
    }
}
