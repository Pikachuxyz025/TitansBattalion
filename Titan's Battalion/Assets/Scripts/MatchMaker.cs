using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Text;
using System.Security.Cryptography;

public class MatchMaker : NetworkBehaviour
{
    public SyncListMatch matches = new SyncListMatch();
    public SyncListString matchIDs = new SyncListString();

    [SerializeField] GameObject turnManagerPrefab;
    [SerializeField] int maxMatchPlayers = 12;
    public static MatchMaker instance;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    public void BeginGame(string _matchID)
    {
        GameObject newTurnManager = Instantiate(turnManagerPrefab);
        NetworkServer.Spawn(newTurnManager);
        newTurnManager.GetComponent<NetworkMatchChecker>().matchId = _matchID.ToGuid();
        TurnManager turnManager = newTurnManager.GetComponent<TurnManager>();
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == _matchID)
            {
                foreach (var player in matches[i].players)
                {
                    Player_Mirror _player = player.GetComponent<Player_Mirror>();
                    //turnManager.AddPlayer(_player);
                    _player.StartGame();
                }
                break;
            }
        }
    }

    public bool HostGame(string _matchID, bool publicMatch, GameObject _player, out int playerIndex)
    {
        playerIndex = -1;
        if (!matchIDs.Contains(_matchID))
        {
            matchIDs.Add(_matchID);
            Matche match = new Matche(_matchID, _player);
            match.publicMatch = publicMatch;
            matches.Add(match);
            Debug.Log($"Match generated");
            _player.GetComponent<Player_Mirror>().currentMatch = match;
            playerIndex = 1;
            return true;
        }
        else
        {
            Debug.Log($"Match ID already exists");
            return false;
        }
    }

    public bool JoinGame(string _matchID, GameObject _player, out int playerIndex)
    {
        playerIndex = -1;
        if (matchIDs.Contains(_matchID))
        {
            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i].matchID == _matchID)
                {
                    if (!matches[i].inMatch && !matches[i].fullMatch)
                    {
                        matches[i].players.Add(_player);
                        _player.GetComponent<Player_Mirror>().currentMatch = matches[i];
                        playerIndex = matches[i].players.Count;

                        if (matches[i].players.Count == maxMatchPlayers)
                            matches[i].fullMatch = true;
                        break;
                    }
                    else
                        return false;
                }
            }
            Debug.Log($"Match joined");
            return true;
        }
        else
        {
            Debug.Log($"Match ID doesn't exists");
            return false;
        }
    }

    public bool SearchGame(GameObject _player, out int playerIndex, out string _matchID)
    {
        playerIndex = -1;
        _matchID = string.Empty;
        for (int i = 0; i < matches.Count; i++)
        {
            Debug.Log($"Checking match {matches[i].matchID} | inMatch {matches[i].inMatch} | matchFull {matches[i].fullMatch} | publicMatch {matches[i].publicMatch}");
            if (matches[i].publicMatch && !matches[i].fullMatch && !matches[i].inMatch)
            {
                _matchID = matches[i].matchID;
                if (JoinGame(_matchID, _player, out playerIndex))
                {
                    _matchID = matches[i].matchID;
                    return true;
                }
            }
        }
        return false;
    }
    public static string GetRandomMatchID()
    {
        string _id = string.Empty;
        for (int i = 0; i < 5; i++)
        {
            int random = UnityEngine.Random.Range(0, 36);
            if (random < 26)
                _id += (char)(random * 65);
            else
                _id += (random - 26).ToString();
        }
        Debug.Log($"Random Match ID: {_id}");
        return _id;
    }

    public void PlayerDisconnected(Player_Mirror player,string _matchID)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == _matchID)
            {
                int playerIndex = matches[i].players.IndexOf(player.gameObject);
                matches[i].players.RemoveAt(playerIndex);
                Debug.Log($"Player disconnected from match {_matchID}|{matches[i].players.Count} players remaining");

                if (matches[i].players.Count == 0)
                {
                    Debug.Log($"No more players in Match. Terminating {_matchID}");
                    matches.RemoveAt(i);
                    matchIDs.Remove(_matchID);
                }
                break;
            }
        }
    }
}

[System.Serializable]
public class Matche
{
    public string matchID;
    public bool inMatch;
    public bool publicMatch;
    public bool fullMatch;
    public SyncListGameObject players = new SyncListGameObject();

    public Matche(string matchID, GameObject player)
    {
        this.matchID = matchID;
        players.Add(player);
    }

    public Matche() { }
}

    [System.Serializable]
    public class SyncListGameObject : SyncList<GameObject> { }

    [System.Serializable]
    public class SyncListMatch : SyncList<Matche> { }


public static class MatchExtensions
{
    public static Guid ToGuid(this string id)
    {
        MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
        byte[] inputBytes = Encoding.Default.GetBytes(id);
        byte[] hashBytes = provider.ComputeHash(inputBytes);

        return new Guid(hashBytes);
    }
}
