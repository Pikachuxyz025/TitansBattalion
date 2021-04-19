using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerSpawnSystem : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab = null;
    private Quaternion neutralCoordination = Quaternion.Euler(0, 0, 0), coordination = Quaternion.Euler(0, 180, 0);
    private int SpawnRot = 0;
    public static PlayerSpawnSystem spawnSystem;
    /*private NetworkManagerLobby room;
    private NetworkManagerLobby Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerLobby;
        }
    }*/
    [SyncVar]
    public int playerNum = 1;

    [SyncVar]
    public GameObject managerGameobject = null;

    [SyncVar]
    public MirrorGameManager manager;

    /*public override void OnStartServer()
    {
        NetworkManagerLobby.OnServerReadied += SpawnPlayer;
        Debug.Log("devestation");
    }

    [ServerCallback]
    private void OnDestroy()
    {
        NetworkManagerLobby.OnServerReadied -= SpawnPlayer;
    }*/

    public void SpawnSystemSetup()
    {
        //TargetDebug("before");
        spawnSystem = this;
        Rpcspawn();
        //managerGameobject = mirrorManager;
        //manager = managerGameobject.GetComponent<MirrorGameManager>();
    }
    [ClientRpc]
    void Rpcspawn() => spawnSystem = this;

    [TargetRpc]
    void TargetDebug(string deg)
    {
        Debug.Log(deg);
    }

    [Server]
    public void SpawnPlayer(NetworkConnection conn)
    {
        //NetworkConnection conn = Room.GamePlayers[playerNum].connectionToClient;
        GameObject playerInstance = Instantiate(playerPrefab, new Vector3(0, 10, 0), Quaternion.Euler(0, SpawnRot, 0));
        manager.playerInfos[playerNum - 1] = playerInstance.GetComponent<PlayerInfo>();
        //NetworkServer.Destroy(conn.identity.gameObject);

        //NetworkServer.ReplacePlayerForConnection(conn, playerInstance);
        NetworkServer.Spawn(playerInstance, conn);
        playerInstance.GetComponent<PlayerInfo>().SetInfo(playerNum, NetworkManagerLobby.PlayerArmy[playerNum - 1], NetworkManagerLobby.PlayerName[playerNum - 1], managerGameobject,manager.varin);

        SpawnRot += 180;
        playerNum++;
    }

    //[Server]
    public void SpawnMultiPlayer(/*ConnectionChangedEventArgs args*/NetworkConnection conn, int play, int armyNo, string username, Player_Mirror sum, System.Guid matchID)
    {
        if (play == 1)
            SpawnRot = 0;
        else if (play == 2)
            SpawnRot = 180;
        GameObject playerInstance = Instantiate(playerPrefab, new Vector3(0, 10, 0), Quaternion.Euler(0, SpawnRot, 0));

        playerInstance.GetComponent<NetworkMatchChecker>().matchId = matchID;
        NetworkServer.Spawn(playerInstance, /*args.*/conn);
        manager.playerHosts[play - 1] = sum;
        manager.RpcSetupPlayerHosts(play, sum);
        manager.playerInfos[play - 1] = playerInstance.GetComponent<PlayerInfo>();
        manager.RpcSetupPlayerHost(play, playerInstance.GetComponent<PlayerInfo>());
        playerInstance.GetComponent<PlayerInfo>().SetInfo(play, /*args.*/armyNo, /*args.*/username, managerGameobject, manager.varin);
    }
}
