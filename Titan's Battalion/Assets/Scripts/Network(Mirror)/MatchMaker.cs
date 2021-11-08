using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Text;
using System.Security.Cryptography;
using Random = UnityEngine.Random;

public class MatchMaker : NetworkBehaviour
{
    public SyncListMatch matches = new SyncListMatch();
    public SyncList<string> matchIDs = new SyncList<string>();
    public SyncList<bool> matchFull = new SyncList<bool>();
    public SyncList<bool> matchReady = new SyncList<bool>();
    public SyncList<bool> matchInProgress = new SyncList<bool>();

    [SerializeField] int maxMatchPlayers = 2;
    public static MatchMaker instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        //DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if (isServer)
            setnReady();
    }

    void setnReady()
    {
        for (int i = 0; i < matches.Count; i++)
        {
            matches[i].matchReady = MatchReady(matches[i].matchID);
            matchReady[i] = matches[i].matchReady;
        }
    }

    public Matche AccessMatch(string _matchID)
    {
        Matche currentMatch = new Matche();
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == _matchID)
            {
                currentMatch = matches[i];
            }
        }
        return currentMatch;
    }

    public void BeginGame(string _matchID)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == _matchID)
            {
                matches[i].inMatch = true;
                foreach (var player in matches[i].players)
                {
                    Player_Mirror _player = player.GetComponent<Player_Mirror>();
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
            Matche match = new Matche(_matchID, _player, publicMatch);
            matches.Add(match);
            Debug.Log($"Match generated");
            _player.GetComponent<Player_Mirror>().currentMatch = match;
            playerIndex = 1;
            matchReady.Add(match.matchReady);
            matchFull.Add(match.fullMatch);
            matchInProgress.Add(match.inMatch);
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
                        {
                            matches[i].fullMatch = true;
                            matchFull[i] = matches[i].fullMatch;
                            Debug.Log(matchFull[i]);
                        }
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

    public bool MatchReady(string _matchID)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == _matchID)
            {
                foreach (GameObject _player in matches[i].players)
                {
                    Debug.Log("should be visible");
                    if (!_player.GetComponent<Player_Mirror>().isReady)
                        return false;
                }
            }
        }
        Debug.Log("only shown if thing is true");
        return true;
    }

    public bool GetReadyMatch(string _matchID)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == _matchID)
            {
                return matchReady[i];
            }
        }
        return false;
    }

    public void SetReadyMatch(string _matchID, bool OnOff)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == _matchID)
            {
                matchReady[i] = OnOff;
            }
        }
    }

    public void SetMatchFull(string _matchID, bool OnOff)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == _matchID)
            {
                matchFull[i] = OnOff;
            }
        }
    }

    public void SetMatchInProgress(string _matchID, bool OnOff)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == _matchID)
            {
                matchInProgress[i] = OnOff;
            }
        }
    }

    public bool GetMatchFull(string _matchID)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == _matchID)
            {
                return matchFull[i];
            }
        }
        return false;
    }

    public bool SearchGame(GameObject _player, out int playerIndex, out string _matchID)
    {
        playerIndex = -1;
        _matchID = string.Empty;
        RandomizeMatches();
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
            int random = Random.Range(0, 36);
            if (random < 26)
                _id += (char)(random + 65);
            else
                _id += (random - 26).ToString();
        }
        Debug.Log($"Random Match ID: {_id}");
        return _id;
    }

    public void RandomizeMatches()
    {
        for (int i = 0; i < matches.Count; i++)
        {
            Matche match = matches[i];
            int randomIndex = Random.Range(i, matches.Count);
            matches[i] = matches[randomIndex];
            matches[randomIndex] = match;
        }
    }

    public void PlayerDisconnected(Player_Mirror player, string _matchID)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == _matchID)
            {
                int playerIndex = matches[i].players.IndexOf(player.gameObject);
                matches[i].players.RemoveAt(playerIndex);
                if (matches[i].players.Count < maxMatchPlayers)
                    matches[i].fullMatch = false;
                matchFull[i] = matches[i].fullMatch;
                Debug.Log($"Player disconnected from match {_matchID}|{matches[i].players.Count} players remaining");

                if (matches[i].players.Count == 0)
                {
                    Debug.Log($"No more players in Match. Terminating {_matchID}");
                    matchFull.RemoveAt(i);
                    matchReady.RemoveAt(i);
                    matchInProgress.RemoveAt(i);
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
    public bool matchReady;
    public bool fullMatch;
    public SyncListGameObject players = new SyncListGameObject();

    public Matche(string matchID, GameObject player, bool publicMatch)
    {
        fullMatch = false;
        inMatch = false;
        this.matchID = matchID;
        this.publicMatch = publicMatch;
        players.Add(player);
    }

    public Matche() { }
}

[System.Serializable]
public struct LobbyList
{
    [SyncVar] public Matche match;       
    [SyncVar] public string matchIDs;
    [SyncVar] public bool matchFull;
    [SyncVar] public bool matchReady;
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
