using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeTerritory:MonoBehaviour
{
    public Chessboard_Testing[] grave;
    public GameMode[] set;

    private Dictionary<GameMode, Chessboard_Testing> Territories()
    {
        Dictionary<GameMode, Chessboard_Testing> check = new Dictionary<GameMode, Chessboard_Testing>();
        for (int i = 0; i < set.Length; i++)
        {
            check.Add(set[i], grave[i]);
        }
        return check;
    }

    public Chessboard_Testing GetTerritory(GameMode mode)
    {
        if (Territories().ContainsKey(mode))
            return Territories()[mode];
        return null;
    }
}
