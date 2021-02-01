using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILobby : MonoBehaviour
{
    public static UILobby instance;

    [Header("Host Join")]
    [SerializeField] InputField joinMatchInput;
    [SerializeField] List<Selectable> lobbySelectables = new List<Selectable>();
    [SerializeField] Canvas lobbyCanvas;
    [SerializeField] Canvas searchCanvas;

    [Header("Lobby")]
    [SerializeField] Transform UIPlayerParent;
    [SerializeField] GameObject UIPlayerPrefab;
    [SerializeField] Text matchIDText;
    [SerializeField] GameObject beginGameButton;
    GameObject playerLobbyUI;
    bool searching = false;
    private void Start() => instance = this;


    public void HostPublic()
    {
        joinMatchInput.interactable = false;
        lobbySelectables.ForEach(x => x.interactable = false);

        Player_Mirror.localplayer.HostGame(true);
    }

    public void HostPrivate()
    {
        joinMatchInput.interactable = false;
        lobbySelectables.ForEach(x => x.interactable = false);

        Player_Mirror.localplayer.HostGame(false);
    }

    public void HostSuccess(bool success, string matchID)
    {
        if (success)
        {
            lobbyCanvas.enabled = true;
            if (playerLobbyUI != null)
                Destroy(playerLobbyUI);
            playerLobbyUI = SpawnPlayerPrefab(Player_Mirror.localplayer);
            matchIDText.text = matchID;
            beginGameButton.SetActive(true);
        }
        else
        {
            joinMatchInput.interactable = true;
            lobbySelectables.ForEach(x => x.interactable = true);
        }
    }

    public void Join()
    {
        joinMatchInput.interactable = false;
        lobbySelectables.ForEach(x => x.interactable = false);

        Player_Mirror.localplayer.JoinGame(joinMatchInput.text.ToUpper());
    }

    public void JoinSuccess(bool success, string matchID)
    {
        if (success)
        {
            lobbyCanvas.enabled = true;
            beginGameButton.SetActive(false);
            if (playerLobbyUI != null)
                Destroy(playerLobbyUI);

            playerLobbyUI = SpawnPlayerPrefab(Player_Mirror.localplayer);
            matchIDText.text = matchID;
        }
        else
        {
            joinMatchInput.interactable = true;
            lobbySelectables.ForEach(x => x.interactable = true);
        }
    }

    public GameObject SpawnPlayerPrefab(Player_Mirror player)
    {
        GameObject newUIPlayer = Instantiate(UIPlayerPrefab, UIPlayerParent);
        newUIPlayer.GetComponent<UIPlayer>().SetPlayer(player);
        newUIPlayer.transform.SetSiblingIndex(player.playerIndex - 1);
        return newUIPlayer;
    }

    public void BeginGame()
    {
        Player_Mirror.localplayer.BeginGame();
    }

    public void SearchGame()
    {
        Debug.Log("Searching for Game");
        searchCanvas.enabled = true;
        StartCoroutine(SearchingForGame());
    }

    IEnumerator SearchingForGame()
    {
        searching = true;
        float currentTime = 1;

        while (searching)
        {
            if (currentTime > 0)
                currentTime -= Time.deltaTime;
            else
            {
                Player_Mirror.localplayer.SearchGame();
                currentTime = 1;
            }

            yield return null;
        }
    }

    public void SearchSuccess(bool success, string matchID)
    {
        if (success)
        {
            searchCanvas.enabled = false;
            JoinSuccess(success, matchID);
            searching = false;
        }
    }

    public void SearchCancel()
    {
        searchCanvas.enabled = false;
        lobbySelectables.ForEach(x => x.interactable = true);
        searching = false;
    }


    public void DisconnectLobby()
    {
        if (playerLobbyUI != null)
            Destroy(playerLobbyUI);
        Player_Mirror.localplayer.DisconnectGame();

        lobbyCanvas.enabled = false;
        lobbySelectables.ForEach(x => x.interactable = true);
        beginGameButton.SetActive(false);
    }
}
