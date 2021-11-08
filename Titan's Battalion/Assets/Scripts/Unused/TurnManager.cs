using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TurnManager : NetworkBehaviour
{
    List<Player_Mirror> players = new List<Player_Mirror>();
    public void AddPlayer(Player_Mirror _player)
    {
        players.Add(_player);
    }
}
