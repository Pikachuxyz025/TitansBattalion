using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.IO;

public class Server : MonoBehaviour
{
    public int port = 3028;
    private List<ServerClient> Sclients;
    private List<ServerClient> disconnectList;

    private TcpListener server;
    private bool serverStarted;

    public void Init()
    {
        DontDestroyOnLoad(gameObject);
        Sclients = new List<ServerClient>();
        disconnectList = new List<ServerClient>();

        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            StartListening();
            serverStarted = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e.Message);
        }
    }

    private void Update()
    {
        if (!serverStarted) return;
        foreach(ServerClient c in Sclients)
        {
            //is the client still connected
            if (!IsConnected(c.tcp))
            {
                c.tcp.Close();
                disconnectList.Add(c);
                continue;
            }
            else
            {
                NetworkStream s = c.tcp.GetStream();
                if (s.DataAvailable)
                {
                    StreamReader reader = new StreamReader(s, true);
                    string data = reader.ReadLine();

                    if (data != null)
                    {
                        OnIncomingData(c, data);
                    }
                }
            }
        }
        for (int i = 0; i < disconnectList.Count-1; i++)
        {
            //tell player somebody has disconnected
            Sclients.Remove(disconnectList[i]);
            disconnectList.RemoveAt(i);
        }
    }
    // Server Send
    private void Broadcast(string data, List<ServerClient> cl)
    {
        foreach (ServerClient sc in cl)
        {
            try
            {
                StreamWriter writer = new StreamWriter(sc.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Write error: " + e.Message);
            }
        }
    }
    private void Broadcast(string data, ServerClient c)
    {
        List<ServerClient> sc = new List<ServerClient> { c };
        Broadcast(data, sc);
    }
    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }
    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;
        string allUsers = "";
        foreach (ServerClient i in Sclients)
        {
            allUsers += i.clientName + '|';
        }
        ServerClient sc = new ServerClient(listener.EndAcceptTcpClient(ar));
        Sclients.Add(sc);

        StartListening();

        Broadcast("SWHO|" + allUsers, Sclients[Sclients.Count - 1]);
    }
    private bool IsConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);

                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
    //Server Read
    private void OnIncomingData(ServerClient c,string data)
    {
        Debug.Log("Server:" + data);
        string[] aData = data.Split('|');
        switch (aData[0])
        {
            case "CWHO":
                c.clientName = aData[1];
                c.isHost = (aData[2] == "0") ? false : true;
                Broadcast("SCNN|" + c.clientName, Sclients);
                break;
            case "CMOV":
                Broadcast("SMOV|" + aData[1] + "|" + aData[2], Sclients);
                break;
        }
    }
}
public class ServerClient
{
    public string clientName;
    public TcpClient tcp;
    public bool isHost;

    public ServerClient(TcpClient tcpclient)
    {
        this.tcp = tcpclient;
    }
}
