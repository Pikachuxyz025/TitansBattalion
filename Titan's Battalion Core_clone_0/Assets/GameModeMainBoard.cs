using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeMainBoard:MonoBehaviour
{
    public ChessboardTemplate[] grave;
    public GameMode[] set;

    private Dictionary<GameMode, ChessboardTemplate> Battlefields()
    {
        Dictionary<GameMode, ChessboardTemplate> check = new Dictionary<GameMode, ChessboardTemplate>();
        for (int i = 0; i < set.Length; i++)
        {
            check.Add(set[i], grave[i]);
        }
        return check;
    }

    public ChessboardTemplate GetBattlefields(GameMode mode)
    {
        if (Battlefields().ContainsKey(mode))
            return Battlefields()[mode];
        return null;
    }
}
