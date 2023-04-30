using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeTerritory : MonoBehaviour
{
    public ChessboardTemplate[] chessboardSelection;
    public GameMode[] gameModeSelection;

    private Dictionary<GameMode, ChessboardTemplate> Territories()
    {
        Dictionary<GameMode, ChessboardTemplate> territories = new Dictionary<GameMode, ChessboardTemplate>();
        for (int i = 0; i < gameModeSelection.Length; i++)
        {
            territories.Add(gameModeSelection[i], chessboardSelection[i]);
        }
        return territories;
    }

    public ChessboardTemplate GetTerritory(GameMode mode)
    {
        if (Territories().ContainsKey(mode))
            return Territories()[mode];
        return null;
    }
}
