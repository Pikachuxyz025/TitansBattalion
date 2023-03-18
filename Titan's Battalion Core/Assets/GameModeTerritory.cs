using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeTerritory:MonoBehaviour
{
    public ChessboardTemplate[] grave;
    public GameMode[] set;

    private Dictionary<GameMode, ChessboardTemplate> Territories()
    {
        Dictionary<GameMode, ChessboardTemplate> check = new Dictionary<GameMode, ChessboardTemplate>();
        for (int i = 0; i < set.Length; i++)
        {
            check.Add(set[i], grave[i]);
        }
        return check;
    }

    public ChessboardTemplate GetTerritory(GameMode mode)
    {
        if (Territories().ContainsKey(mode))
            return Territories()[mode];
        return null;
    }
}
