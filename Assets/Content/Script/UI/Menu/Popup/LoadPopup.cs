using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class LoadPopup : MonoBehaviour
{
    [Header("Game Data")]
    [SerializeField] private GameData gameData;

    [Header("Buttons Load Popup")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button exitLoadPopupButton;

    [Header("Buttons Confirm Popup")]
    [SerializeField] private Button confirmNewGameButton;
    [SerializeField] private Button cancelNewGameButton;
    [SerializeField] private Button exitConfirmPopupButton;

    [Header("Lobby Game")]
    [SerializeField] private LobbyLocal localMenu;

    [Header("Online Game")]
    [SerializeField] private TMP_InputField joinInput;

    // Variables de control de carga de datos
    private int mode;
    private int slotData;

    #region Load Lobby

    private void OnEnable()
    {
        ActiveLoadButtons(true);
        ActiveConfirmButtons(true);
    }

    public void OpenPopup(int index)
    {
        MenuManager.Instance.ActiveMenuButtons(false);
        mode = index;
        slotData = mode switch
        {
            0 => 1,
            1 or 2 => 2,
            3 or 4 => 3,
            _ => 0,
        };

        CheckSaveGame();
    }

    private void CheckSaveGame()
    {
        gameData.ClearGameData();
        if (mode == 4) TryJoinOnlineGame();
        else if (mode == 3) TryCreateOnlineGame(); //FIXME: Revisar
        else if (!SaveService.CheckSaveFile(slotData)) NewGame();
        else MenuManager.Instance.OpenLoadMenu();
    }

    public void NewGame()
    {
        ActiveConfirmButtons(false);
        if (mode == 3) TryCreateOnlineGame();
        else LoadLocalLobby();
    }

    public void LoadGame()
    {
        ActiveLoadButtons(false);
        StartCoroutine(LoadGameRoutine());
    }

    private IEnumerator LoadGameRoutine()
    {
        yield return SaveService.LoadGame(gameData, slotData);
        if (mode == 3) TryCreateOnlineGame();
        else LoadLocalLobby();
    }

    private void ActiveLoadButtons(bool active)
    {
        newGameButton.interactable = active;
        loadGameButton.interactable = active;
        exitLoadPopupButton.interactable = active;
    }

    private void ActiveConfirmButtons(bool active)
    {
        confirmNewGameButton.interactable = active;
        cancelNewGameButton.interactable = active;
        exitConfirmPopupButton.interactable = active;
    }

    #endregion

    #region Lobby Local

    private void LoadLocalLobby()
    {
        localMenu.SetMode(mode);
        MenuManager.Instance.CloseLoadPopup();
        MenuManager.Instance.OpenLobbyLocalMenu();
    }

    #endregion

    #region Lobby Online

    private async void TryCreateOnlineGame()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            MenuManager.Instance.CloseLoadPopup();
            MenuManager.Instance.OpenMessagePopup("Revisa tu conexión a internet.");
            return;
        }

        int maxPlayers = 4;
        if (gameData.DataExists()) maxPlayers = gameData.playersData.Count;

        RelayService.Instance.DefaultServer();
        bool connectionSuccessful = await RelayService.Instance.StartRelayHostAsync(maxPlayers);
        MenuManager.Instance.CloseLoadPopup();
        if (connectionSuccessful)
        {
            MenuManager.Instance.OpenLobbyOnlineMenu();
        }
        else
        {
            //FIXME: Agregar Popup de error
            Debug.LogError("Failed to create Relay, please try again.");
        }
    }

    private async void TryJoinOnlineGame()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            MenuManager.Instance.OpenMessagePopup("Revisa tu conexión a internet.");
            return;
        }

        string joinCode = joinInput.text;

        RelayService.Instance.DefaultServer();
        bool connectionSuccessful = await RelayService.Instance.JoinRelayServerAsync(joinCode);
        MenuManager.Instance.CloseLoadPopup();
        if (connectionSuccessful)
        {
            MenuManager.Instance.OpenLobbyOnlineMenu();
        }
        else
        {
            //FIXME: Agregar Popup de error
            Debug.Log("Failed to join Relay, please try again.");
        }
    }

    #endregion

}