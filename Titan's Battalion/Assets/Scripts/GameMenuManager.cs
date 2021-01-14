using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class GameMenuManager : MonoBehaviour
{
    public static GameMenuManager Instance { set; get; }

    public GameObject mainMenu, serverMenu, connectMenu, serverPrefab, clientPrefab;
    public InputField nameInput;
    // Start is called before the first frame update

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }
    void Start()
    {
        serverMenu.SetActive(false);
        connectMenu.SetActive(false);
        mainMenu.SetActive(true);
        DontDestroyOnLoad(gameObject);
    }

    public void ConnectButton()
    {
        mainMenu.SetActive(false);
        connectMenu.SetActive(true);
    }
    public void HostButton()
    {
        try
        {
            Server s = Instantiate(serverPrefab).GetComponent<Server>();
            s.Init();

            Client c = Instantiate(clientPrefab).GetComponent<Client>();
            c.clientName = nameInput.text;
            c.isHost = true;
            if (c.clientName == "")
                c.clientName = "Host";
            c.ConnectToServer("127.0.0.1", 3028);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        mainMenu.SetActive(false);
        serverMenu.SetActive(true);
    }
    public void ConnectToServerButton()
    {
        string hostAddress = GameObject.Find("HostInput").GetComponent<InputField>().text;
        if (hostAddress == "")
            hostAddress = "127.0.0.1";

        try
        {
            Client c = Instantiate(clientPrefab).GetComponent<Client>();
            c.clientName = nameInput.text;
            if (c.clientName == "")
                c.clientName = "Client";
            c.ConnectToServer(hostAddress, 3028);
            connectMenu.SetActive(false);
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }
    }
    public void BackButton()
    {
        serverMenu.SetActive(false);
        connectMenu.SetActive(false);
        mainMenu.SetActive(true);

        Server s = FindObjectOfType<Server>();
        if (s != null)
            Destroy(s.gameObject);

        Client c = FindObjectOfType<Client>();
        if (c != null)
            Destroy(c.gameObject);
    }
    public void StartGame()
    {
        SceneManager.LoadScene("TestScene");
    }
}
