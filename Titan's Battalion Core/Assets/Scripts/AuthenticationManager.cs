using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class AuthenticationManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameEntry;
    [SerializeField] private GameObject LoginButton;
    private void Update()
    {
        if (usernameEntry.text == string.Empty)
            LoginButton.SetActive(false);
        else
            LoginButton.SetActive(true);
    }

    public async void LoginAnonymously()
    {
        using (new Load("Logging you in..."))
        {
            await Authentication.Login(usernameEntry.text);
            SceneManager.LoadSceneAsync("Login Menu");
        };
    }
}