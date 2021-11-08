using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayer : MonoBehaviour
{
    [SerializeField] Text text;
    Player_Mirror player;
    string playerUsername;
    public Image userImage;
    public Color defaultColor;
    Color ready = new Color(97, 209, 72);

    public void SetPlayer(Player_Mirror _player)
    {
        this.player = _player;
        text.text = "Player " + player.playerIndex.ToString();
    }

    public void SetReady() => userImage.color = ready;

    public void SetNotReady() => userImage.color = defaultColor;
}
