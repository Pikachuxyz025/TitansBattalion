using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerSpawnSystem : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab = null;
    private Quaternion neutralCoordination = Quaternion.Euler(0, 0, 0), coordination = Quaternion.Euler(0, 180, 0);
    private int SpawnRot = 0;

    private NetworkManagerLobby room;

    private NetworkManagerLobby Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerLobby;
        }
    }

    [SyncVar]
    public int playerNum = 1;

    [SyncVar]
    public GameObject managerGameobject;

    //[HideInInspector]
    public static MirrorGameManager manager;

    public override void OnStartServer() => NetworkManagerLobby.OnServerReadied += SpawnPlayer;

    [ServerCallback]
    private void OnDestroy() => NetworkManagerLobby.OnServerReadied -= SpawnPlayer;

    [Server]
    public void SpawnPlayer(NetworkConnection conn)
    {
        //NetworkConnection conn = Room.GamePlayers[playerNum].connectionToClient;
        GameObject playerInstance = Instantiate(playerPrefab, new Vector3(0, 10, 0), Quaternion.Euler(0, SpawnRot, 0));
        manager.playerInfos[playerNum - 1] = playerInstance.GetComponent<PlayerInfo>();
        //NetworkServer.Destroy(conn.identity.gameObject);

        //NetworkServer.ReplacePlayerForConnection(conn, playerInstance);
        NetworkServer.Spawn(playerInstance, conn);
        playerInstance.GetComponent<PlayerInfo>().RpcSetInfo(playerNum, NetworkManagerLobby.PlayerArmy[playerNum - 1], NetworkManagerLobby.PlayerName[playerNum - 1], managerGameobject);

        SpawnRot += 180;
        playerNum++;
    }
}
