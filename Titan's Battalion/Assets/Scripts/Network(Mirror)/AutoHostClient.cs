using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AutoHostClient : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;

    [SerializeField] GameObject startingScreen, connectionScreen;
    /*private void Start()
    {
        if (!Application.isBatchMode)
        {
            Debug.Log("Client Connected");
            networkManager.StartClient();
        }
        else
        {
            Debug.Log("Server Starting");
        }
    }*/

    public void Online()
    {
        if (networkManager == null)
            networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        StartCoroutine(GetOnline());
    }

    IEnumerator GetOnline()
    {
        startingScreen.SetActive(false);
        networkManager.StartClient();
        yield return new WaitForSeconds(3f);

        if (!networkManager.isNetworkActive)
        {
            Debug.Log("Server isn't active");
            connectionScreen.SetActive(true);
        }
        else
        {
            Debug.Log("Server is active");

            connectionScreen.SetActive(true);
            yield return null;
        }
        yield return null;
    }

    public void BackButton()
    {
        connectionScreen.SetActive(false);
        startingScreen.SetActive(true);
        networkManager.StopClient();
    }

    public void JoinLocal()
    {
        networkManager.networkAddress = /*"3.18.125.165"*/"localhost";
        networkManager.StartClient();
    }
}
