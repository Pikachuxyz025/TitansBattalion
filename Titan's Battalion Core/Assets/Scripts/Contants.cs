using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Contants
{
    public const string JoinKey = "j";
    public const string DifficultyKey = "d";
    public const string GameTypeKey = "t";

    public static readonly List<string> MainBoards = new() { "Battle Royal", "Capture The Flag", "Creative" };
    public static readonly List<string> Armies = new() { "None", "Testing", "Medium", "Hard" };
}
