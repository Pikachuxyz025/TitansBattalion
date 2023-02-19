using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Contants
{
    public const string JoinKey = "j";
    public const string DifficultyKey = "d";
    public const string GameTypeKey = "t";

    public static readonly List<string> GameModes = new() { "Chess Mode", "Battalion Mode" };
    public static readonly List<string> MainBoards = new() { "BasicBoard", "T2" };
    public static readonly List<string> Armies = new() {"Medieval", "Castling Test","5 X 10" };
}
