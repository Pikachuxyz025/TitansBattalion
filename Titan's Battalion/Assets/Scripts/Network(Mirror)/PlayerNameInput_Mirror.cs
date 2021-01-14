using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class PlayerNameInput_Mirror : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField nameInputField = null;
    [SerializeField] private Button continuebutton = null;

    public static string DisplayName { get; private set; }

    private const string PlayerPrefsNameKey = "PlayerName";

    public static int ArmyID { get; private set; }

    private void Start()
    {
        SetUpInputField();
    }
    private void Update()
    {
        SetPlayerName(nameInputField.text);
        if (ArmyID == 0)
            continuebutton.interactable = false;
        else
            continuebutton.interactable = true;
    }
    
    private void SetUpInputField()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsNameKey)) { return; }
        string defaultName = PlayerPrefs.GetString(PlayerPrefsNameKey);
        nameInputField.text = defaultName;
        SetPlayerName(defaultName);
    }

    public void SaveArmyID(int armyid)
    {
        ArmyID = armyid;
    }

    public void SetPlayerName(string name)
    {
        continuebutton.interactable = !string.IsNullOrEmpty(name);
    }

    public void SavePlayerName()
    {
        DisplayName = nameInputField.text;

        PlayerPrefs.SetString(PlayerPrefsNameKey, DisplayName);
    }
}
