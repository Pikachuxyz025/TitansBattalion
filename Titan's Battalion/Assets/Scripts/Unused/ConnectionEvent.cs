using UnityEngine;
using Mirror;
using UnityEngine.Events;


public class ConnectionChangedEvent : UnityEvent<ConnectionEvent> { }

public class ConnectionEvent  
{
    public NetworkConnection conn;
    public int playerNum, armyNo;
    public string username;
    public GameObject player;

    public ConnectionEvent(NetworkConnection _conn, int _player, int _army, string _user, GameObject _played)
    {
        conn = _conn;
        playerNum = _player;
        armyNo = _army;
        username = _user;
        player = _played;
    }
}


