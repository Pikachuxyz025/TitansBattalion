using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DataSend:NetworkBehaviour
{
    public static int mainBoardId = 0;
    public static DataSend instance;
    public override void OnNetworkSpawn()
    {
        Debug.Log(mainBoardId);
        instance = this;

    }
}
