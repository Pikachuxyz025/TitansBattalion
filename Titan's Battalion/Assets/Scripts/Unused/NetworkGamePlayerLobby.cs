using Mirror;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NetworkGamePlayerLobby : NetworkBehaviour
{
    [SyncVar]
    [SerializeField] private string displayName = "Loading...";

    [SyncVar]
    [SerializeField] private int armyId;

    private NetworkManagerLobby room;

    private NetworkManagerLobby Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerLobby;
        }
    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);

        Room.GamePlayers.Add(this);

        Debug.Log(displayName + " has " + armyId + " as an ID");
    }

    public override void OnStopServer()
    {
        Room.GamePlayers.Remove(this);
    }

    [Server]
    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }

    [Server]
    public void SetRoomInfo(int army)
    {
        this.armyId = army;
    }
}
