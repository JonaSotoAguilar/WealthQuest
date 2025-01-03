using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class LoadMenu : MonoBehaviour
{
    [Header("Game Data")]
    [SerializeField] private GameData gameData;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;

    [Header("Lobby Game")]
    [SerializeField] private LobbyLocal localMenu;

    [Header("Select Game Mode")]
    [SerializeField] private GameObject playMenu;
    [SerializeField] private GameObject modeMenu;
    [SerializeField] private GameObject onlineMenu;

    [Header("Online Game")]
    [SerializeField] private Button createOnlineButton;
    [SerializeField] private Button joinOnlineButton;
    [SerializeField] private TMP_InputField joinInput;

    // Variables de control de carga de datos
    private int indexRoom;
    private int slotData;

    private void Awake()
    {
        newGameButton.onClick.AddListener(NewGame);
        loadGameButton.onClick.AddListener(() => StartCoroutine(LoadGame()));
    }

    #region Methods panel load

    public void ShowPanel(bool active)
    {
        gameObject.SetActive(active);
    }

    public void LoadGameMenu(int index)
    {
        gameData.ClearGameData();
        indexRoom = index;
        switch (index)
        {
            case 0:
                slotData = 1;
                break;
            case 1:
            case 2:
                slotData = 2;
                break;
            case 3:
            case 4:
                createOnlineButton.interactable = false;
                joinOnlineButton.interactable = false;
                slotData = 3;
                break;
            default:
                slotData = 0;
                break;
        }

        if (index == 4) TryJoinOnlineGame();
        else CheckGameData();
    }

    private void LoadLobby(int index)
    {
        switch (index)
        {
            case 0:
                localMenu.SetMode(0);
                playMenu.SetActive(false);
                localMenu.ShowPanel(true);
                break;
            case 1:
                localMenu.SetMode(1);
                modeMenu.SetActive(false);
                localMenu.ShowPanel(true);
                break;
            case 2:
                localMenu.SetMode(2);
                modeMenu.SetActive(false);
                localMenu.ShowPanel(true);
                break;
            default:
                break;
        }
        ShowPanel(false);
    }

    #endregion

    #region Methods Game data

    private void CheckGameData()
    {
        bool exists = SaveSystem.CheckSaveFile(slotData);
        if (!exists) NewGame();
        else gameObject.SetActive(true);
    }

    public void NewGame()
    {
        if (indexRoom == 3) TryCreateOnlineGame();
        else LoadLobby(indexRoom);
    }

    private IEnumerator LoadGame()
    {
        yield return SaveSystem.LoadGame(gameData, slotData);
        if (indexRoom == 3) TryCreateOnlineGame();
        else LoadLobby(indexRoom);
    }

    #endregion

    #region Methods online game

    private async void TryCreateOnlineGame()
    {
        bool exists = SaveSystem.CheckSaveFile(slotData);
        gameObject.SetActive(exists);

        int maxPlayers = 4;
        if (gameData.DataExists()) maxPlayers = gameData.playersData.Count;

        bool connectionSuccessful = await WQRelayManager.Instance.StartRelayHostAsync(maxPlayers);
        if (connectionSuccessful)
        {
            ShowPanel(false);
            onlineMenu.SetActive(false);
            createOnlineButton.interactable = true;
            CleanJoinInput();
        }
        else
        {
            //FIXME: Agregar Popup de error
            ShowPanel(false);
            Debug.LogError("Failed to create Relay, please try again.");
        }
    }

    private async void TryJoinOnlineGame()
    {
        string joinCode = joinInput.text;
        if (joinCode.Length == 0 || joinCode.Length > 6)
        {
            ShowPanel(false);
            return;
        }

        bool connectionSuccessful = await WQRelayManager.Instance.JoinRelayServerAsync(joinCode);
        if (connectionSuccessful)
        {
            ShowPanel(false);
            onlineMenu.SetActive(false);
            createOnlineButton.interactable = true;
            CleanJoinInput();
        }
        else
        {
            //FIXME: Agregar Popup de error
            ShowPanel(false);
            Debug.Log("Failed to join Relay, please try again.");
        }
    }

    private void CleanJoinInput()
    {
        joinInput.text = "";
        joinOnlineButton.interactable = false;
    }

    #endregion

}