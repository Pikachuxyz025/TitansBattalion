using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DataSend : NetworkBehaviour
{
    public static GameMode boardData=GameMode.None;
    public static DataSend instance;
    public override void OnNetworkSpawn()
    {
        instance = this;
    }
}
