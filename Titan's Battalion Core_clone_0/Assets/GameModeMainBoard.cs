using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeMainBoard:MonoBehaviour
{
    public Chessboard_Testing[] grave;
    public GameMode[] set;

    private Dictionary<GameMode, Chessboard_Testing> Battlefields()
    {
        Dictionary<GameMode, Chessboard_Testing> check = new Dictionary<GameMode, Chessboard_Testing>();
        for (int i = 0; i < set.Length; i++)
        {
            check.Add(set[i], grave[i]);
        }
        return check;
    }

    public Chessboard_Testing GetBattlefields(GameMode mode)
    {
        if (Battlefields().ContainsKey(mode))
            return Battlefields()[mode];
        return null;
    }
}
