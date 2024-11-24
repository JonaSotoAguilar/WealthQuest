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
    [SerializeField] private SingleMenu singleMenu;             // index = 0
    [SerializeField] private LocalMultiRoom localMultiRoom;     // index = 1
    [SerializeField] private LocalPassMenu localPassRoom;       // index = 2
    [SerializeField] private OnlineLobby onlineLobby;           // index = 3

    [Header("Select Game Mode")] // FIXME: Cambiar por Objeto Canvas Menu
    [SerializeField] private GameObject playMenu;
    [SerializeField] private GameObject localMenu;
    [SerializeField] private GameObject onlineMenu;

    [Header("Online Game")]
    [SerializeField] private TMP_InputField joinInput;

    // Variables de control de carga de datos
    private int indexRoom;
    private int slotData;

    void Start()
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
                playMenu.SetActive(false);
                singleMenu.ShowPanel(true);
                break;
            case 1:
                localMenu.SetActive(false);
                localMultiRoom.ShowPanel(true);
                break;
            case 2:
                localMenu.SetActive(false);
                localPassRoom.ShowPanel(true);
                break;
            case 3:
            case 4:
                onlineMenu.SetActive(false);
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
        gameData.ClearGameData();
        if (indexRoom == 3) TryCreateOnlineGame();
        else LoadLobby(indexRoom);
    }

    private IEnumerator LoadGame()
    {
        gameData.ClearGameData();
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

        bool connectionSuccessful = await RelayManager.Instance.CreateRelay();
        if (connectionSuccessful)
        {
            LoadLobby(3);
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
        if (joinCode.Length == 0)
        {
            // FIXME: Mostrar mensaje de código vacío

            Debug.LogError("Join code is empty.");
            ShowPanel(false);
            return;
        }

        bool connectionSuccessful = await RelayManager.Instance.JoinRelay(joinCode);
        if (connectionSuccessful)
        {
            LoadLobby(4);
        }
        else
        {
            //FIXME: Agregar Popup de error
            ShowPanel(false);
            Debug.LogError("Failed to join Relay, please try again.");
        }
    }

    #endregion

}