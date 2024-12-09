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
    [SerializeField] private LobbySingle singleMenu;             // index = 0
    [SerializeField] private LobbyLocalMulti localMultiRoom;     // index = 1
    [SerializeField] private LobbyLocalPass localPassRoom;       // index = 2
    //[SerializeField] private LobbyOnline onlineLobby;            // index = 3

    [Header("Select Game Mode")]
    [SerializeField] private GameObject playMenu;
    [SerializeField] private GameObject localMenu;
    [SerializeField] private GameObject onlineMenu;

    [Header("Online Game")]
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

        int maxPlayers = 4;
        if (gameData.DataExists()) maxPlayers = gameData.playersData.Count;

        bool connectionSuccessful = await WQRelayManager.Instance.StartRelayHostAsync(maxPlayers);
        if (connectionSuccessful)
        {
            ShowPanel(false);
            onlineMenu.SetActive(false);
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
            Debug.Log("Join code not valid.");
            ShowPanel(false);
            return;
        }

        bool connectionSuccessful = await WQRelayManager.Instance.JoinRelayServerAsync(joinCode);
        if (connectionSuccessful)
        {
            ShowPanel(false);
            onlineMenu.SetActive(false);
            Debug.Log("Relay joined successfully.");
        }
        else
        {
            //FIXME: Agregar Popup de error
            ShowPanel(false);
            Debug.Log("Failed to join Relay, please try again.");
        }
    }

    #endregion

}