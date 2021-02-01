using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayer : MonoBehaviour
{
    [SerializeField] Text text;
    Player_Mirror player;

    public void SetPlayer(Player_Mirror _player)
    {
        this.player = _player;
        text.text = "Player " + player.playerIndex.ToString();
    }
}
