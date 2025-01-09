using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Login Menus")]
    [SerializeField] private GameObject loginMenu;
    [SerializeField] private GameObject registerMenu;

    [Header("Main Menus")]
    [SerializeField] private GameObject startMenu;
    [SerializeField] private GameObject optionMenu;
    [SerializeField] private GameObject profileMenu;
    [SerializeField] private GameObject contentMenu;
    [SerializeField] private GameObject createContentMenu;

    [Header("Play Menus")]
    [SerializeField] private GameObject playMenu;
    [SerializeField] private GameObject playLocalMenu;
    [SerializeField] private GameObject playOnlineMenu;
    [SerializeField] private TMP_InputField joinInput;

    [Header("Lobbys Menus")]
    [SerializeField] private GameObject lobbyLocalMenu;
    [SerializeField] private GameObject lobbyOnlineMenu;

    [Header("Load Popup")]
    [SerializeField] private LoadPopup loadPopup;
    [SerializeField] private GameObject confirmNewGamePopup;

    [Header("Content Popup")]
    [SerializeField] private GameObject confirmCreatePopup;

    [Header("Profile Popup")]
    [SerializeField] private GameObject bGamesLoginPopup;
    [SerializeField] private GameObject bGamesLogoutPopup;
    [SerializeField] private GameObject changeNamePopup;

    [Header("Message Popup")]
    [SerializeField] private GameObject messagePopup;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Exit Popup")]
    [SerializeField] private GameObject exitGamePopup;
    [SerializeField] private GameObject exitOnlineLobbyPopup;


    // Active Menu
    private GameObject activeMenu;

    #region Initialization

    private void Awake()
    {
        Debug.Log("Menu Manager Awake");
        CreateInstance();
        activeMenu = loginMenu;
        if (FirebaseManager.Instance.logged) OpenStartMenu();
    }

    private void CreateInstance()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    #endregion

    #region Login Menus

    public void OpenLoginMenu()
    {
        LoginManager.Instance.ResetLoginFields();
        activeMenu.SetActive(false);
        loginMenu.SetActive(true);
        activeMenu = loginMenu;
    }

    public void OpenRegistrationMenu()
    {
        LoginManager.Instance.ResetRegistrationFields();
        activeMenu.SetActive(false);
        registerMenu.SetActive(true);
        activeMenu = registerMenu;
    }

    #endregion

    #region Main Menus

    public void OpenStartMenu()
    {
        activeMenu.SetActive(false);
        startMenu.SetActive(true);
        activeMenu = startMenu;
    }

    public void OpenOptionMenu()
    {
        activeMenu.SetActive(false);
        optionMenu.SetActive(true);
        activeMenu = optionMenu;
    }

    public void OpenProfileMenu()
    {
        activeMenu.SetActive(false);
        profileMenu.SetActive(true);
        activeMenu = profileMenu;
    }

    public void OpenContentMenu()
    {
        activeMenu.SetActive(false);
        contentMenu.SetActive(true);
        activeMenu = contentMenu;
    }

    public void OpenCreateContentMenu()
    {
        activeMenu.SetActive(false);
        createContentMenu.SetActive(true);
        activeMenu = createContentMenu;
    }

    #endregion

    #region Play Menus

    public void OpenPlayMenu()
    {
        activeMenu.SetActive(false);
        playMenu.SetActive(true);
        activeMenu = playMenu;
    }

    public void OpenPlayLocalMenu()
    {
        activeMenu.SetActive(false);
        playLocalMenu.SetActive(true);
        activeMenu = playLocalMenu;
    }

    public void OpenPlayOnlineMenu()
    {
        joinInput.text = "";
        activeMenu.SetActive(false);
        playOnlineMenu.SetActive(true);
        activeMenu = playOnlineMenu;
    }

    #endregion

    #region Lobbys Menus

    public void OpenLobbyLocalMenu()
    {
        activeMenu.SetActive(false);
        lobbyLocalMenu.SetActive(true);
        activeMenu = lobbyLocalMenu;
    }

    public void OpenLobbyOnlineMenu()
    {
        joinInput.text = "";
        activeMenu.SetActive(false);
        activeMenu = lobbyOnlineMenu;
    }

    #endregion

    #region Profile Popup

    public void OpenBGamesLoginPopup(bool active)
    {
        CanvasGroup canvasGroup = profileMenu.GetComponent<CanvasGroup>();
        GroupActive(canvasGroup, !active);
        bGamesLoginPopup.SetActive(active);
    }

    public void OpenBGamesLogoutPopup(bool active)
    {
        CanvasGroup canvasGroup = profileMenu.GetComponent<CanvasGroup>();
        GroupActive(canvasGroup, !active);
        bGamesLogoutPopup.SetActive(active);
    }

    public void OpenChangeNamePopup(bool active)
    {
        CanvasGroup canvasGroup = profileMenu.GetComponent<CanvasGroup>();
        GroupActive(canvasGroup, !active);
        changeNamePopup.SetActive(active);
    }

    #endregion

    #region Content Popup

    public void OpenConfirmCreatePopup(bool active)
    {
        CanvasGroup canvasGroup = createContentMenu.GetComponent<CanvasGroup>();
        GroupActive(canvasGroup, !active);
        confirmCreatePopup.SetActive(active);
    }

    #endregion

    #region Load Popup

    public void OpenLoadMenu()
    {
        loadPopup.gameObject.SetActive(true);
    }

    public void CloseLoadPopup()
    {
        loadPopup.gameObject.SetActive(false);
        confirmNewGamePopup.SetActive(false);
        ActiveMenuButtons(true);
    }

    public void ActiveMenuButtons(bool active)
    {
        if (activeMenu == playMenu)
        {
            CanvasGroup canvasGroup = playMenu.GetComponent<CanvasGroup>();
            GroupActive(canvasGroup, active);
        }
        else if (activeMenu == playLocalMenu)
        {
            CanvasGroup canvasGroup = playLocalMenu.GetComponent<CanvasGroup>();
            GroupActive(canvasGroup, active);
        }
        else if (activeMenu == playOnlineMenu)
        {
            CanvasGroup canvasGroup = playOnlineMenu.GetComponent<CanvasGroup>();
            GroupActive(canvasGroup, active);
        }
    }

    public void OpenConfirmNewGamePopup()
    {
        loadPopup.gameObject.SetActive(false);
        confirmNewGamePopup.SetActive(true);
    }

    #endregion

    #region Mesagge Popup

    public void OpenMessagePopup(string message)
    {
        messageText.text = message;
        ShowMessagePopup();
        ReturnMenu();
        CloseMessagePopup();
    }

    private void ReturnMenu()
    {
        if (activeMenu == createContentMenu)
            OpenContentMenu();
    }

    private void ShowMessagePopup()
    {
        CanvasGroup canvasGroup = messagePopup.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1;
        messagePopup.SetActive(true);
    }

    private void CloseMessagePopup()
    {
        CanvasGroup canvasGroup = messagePopup.GetComponent<CanvasGroup>();
        LeanTween.alphaCanvas(canvasGroup, 0, 1.5f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() => messagePopup.SetActive(false));
    }

    #endregion

    #region Exit Popup

    public void OpenExitGamePopup(bool active)
    {
        CanvasGroup canvasGroup = startMenu.GetComponent<CanvasGroup>();
        GroupActive(canvasGroup, !active);
        exitGamePopup.SetActive(active);
    }

    public void OpenPopupExitOnlineLobby(bool active)
    {
        CanvasGroup canvasGroup = lobbyOnlineMenu.GetComponent<CanvasGroup>();
        GroupActive(canvasGroup, !active);
        exitOnlineLobbyPopup.SetActive(active);
    }

    #endregion

    #region Canvas Group

    private void GroupActive(CanvasGroup canvasGroup, bool active)
    {
        canvasGroup.interactable = active;
        canvasGroup.blocksRaycasts = active;
    }

    #endregion

}
