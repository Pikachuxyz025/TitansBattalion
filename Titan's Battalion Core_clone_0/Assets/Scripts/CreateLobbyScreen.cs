using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

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

    public static event Action<LobbyData> LobbyCreated;

    private void Start()
    {
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
            MaxPlayers = int.Parse(_maxPlayersInput.text),
            MainBoard = mainBoardDropdown.value,
        };

        LobbyCreated?.Invoke(lobbyData);
    }
}
