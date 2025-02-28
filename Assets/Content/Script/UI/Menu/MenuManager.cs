using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Login Menus")]
    [SerializeField] private GameObject loginMenu;
    [SerializeField] private GameObject registerMenu;
    [SerializeField] private GameObject forgotMenu;
    [SerializeField] private GameObject testMenu;

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

    [Header("Event System")]
    [SerializeField] private EventSystem eventSystem;

    // Active Menu
    private GameObject activeMenu;

    #region Initialization

    private void Awake()
    {
        CreateInstance();
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

    public void OpenGameMenu()
    {
        if (RelayService.Instance.exitNetwork)
        {
            RelayService.Instance.exitNetwork = false;
            OpenPlayOnlineMenu();
        }
        else
        {
            _ = ApplyTest();
        }
    }

    private async Task ApplyTest()
    {
        bool applyTest = await ProfileUser.ApplyTest();
        if (applyTest)
        {
            if (activeMenu != null || activeMenu == testMenu) activeMenu.SetActive(false);
            testMenu.SetActive(true);
            SetFirstSelectable(testMenu);
            activeMenu = testMenu;
        }
        else
        {
            if (activeMenu != null || activeMenu == startMenu) activeMenu.SetActive(false);
            startMenu.SetActive(true);
            SetFirstSelectable(startMenu);
            activeMenu = startMenu;
        }
    }

    #endregion

    #region Login Menus

    public void OpenLoginMenu()
    {
        LoginManager.Instance.ResetLoginFields();
        OpenMenu(loginMenu);
    }

    public void OpenRegistrationMenu()
    {
        LoginManager.Instance.ResetRegistrationFields();
        OpenMenu(registerMenu);
    }

    public void OpenForgotMenu()
    {
        LoginManager.Instance.ResetForgotFields();
        OpenMenu(forgotMenu);
    }

    #endregion

    #region Main Menus

    private void OpenTestMenu()
    {
        OpenMenu(testMenu);
    }

    public void OpenStartMenu()
    {
        OpenMenu(startMenu);
    }

    public void OpenOptionMenu()
    {
        OpenMenu(optionMenu);
    }

    public void OpenProfileMenu()
    {
        OpenMenu(profileMenu);
    }

    public void OpenContentMenu()
    {
        OpenMenu(contentMenu);
    }

    public void OpenCreateContentMenu()
    {
        OpenMenu(createContentMenu);

    }

    #endregion

    #region Play Menus

    public void OpenPlayMenu()
    {
        OpenMenu(playMenu);
    }

    public void OpenPlayLocalMenu()
    {
        OpenMenu(playLocalMenu);
    }

    public void OpenPlayOnlineMenu()
    {
        joinInput.text = "";
        OpenMenu(playOnlineMenu);
    }

    #endregion

    #region Lobbys Menus

    public void OpenLobbyLocalMenu()
    {
        OpenMenu(lobbyLocalMenu);
    }

    public void OpenLobbyOnlineMenu()
    {
        joinInput.text = "";
        OpenMenu(lobbyOnlineMenu);
    }

    #endregion

    #region Profile Popup

    public void OpenBGamesLoginPopup(bool active)
    {
        CanvasGroup canvasGroup = profileMenu.GetComponent<CanvasGroup>();
        GroupActive(canvasGroup, !active);
        bGamesLoginPopup.SetActive(active);
        SetFirstSelectable(bGamesLoginPopup);
    }

    public void OpenBGamesLogoutPopup(bool active)
    {
        CanvasGroup canvasGroup = profileMenu.GetComponent<CanvasGroup>();
        GroupActive(canvasGroup, !active);
        bGamesLogoutPopup.SetActive(active);
        SetFirstSelectable(bGamesLogoutPopup);
    }

    public void OpenChangeNamePopup(bool active)
    {
        CanvasGroup canvasGroup = profileMenu.GetComponent<CanvasGroup>();
        GroupActive(canvasGroup, !active);
        changeNamePopup.SetActive(active);
        SetFirstSelectable(changeNamePopup);
    }

    #endregion

    #region Content Popup

    public void OpenConfirmCreatePopup(bool active)
    {
        CanvasGroup canvasGroup = createContentMenu.GetComponent<CanvasGroup>();
        GroupActive(canvasGroup, !active);
        confirmCreatePopup.SetActive(active);
        SetFirstSelectable(confirmCreatePopup);
    }

    #endregion

    #region Load Popup

    public void OpenLoadMenu()
    {
        loadPopup.gameObject.SetActive(true);
        SetFirstSelectable(loadPopup.gameObject);
    }

    public void CloseLoadPopup()
    {
        loadPopup.gameObject.SetActive(false);
        confirmNewGamePopup.SetActive(false);
        ActiveMenuButtons(true);
        SetFirstSelectable(activeMenu);
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
        SetFirstSelectable(activeMenu);
    }

    public void OpenConfirmNewGamePopup()
    {
        loadPopup.gameObject.SetActive(false);
        confirmNewGamePopup.SetActive(true);
        SetFirstSelectable(confirmNewGamePopup);
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
        LeanTween.alphaCanvas(canvasGroup, 0, 2.5f)
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
        SetFirstSelectable(exitGamePopup);
    }

    public void OpenPopupExitOnlineLobby(bool active)
    {
        CanvasGroup canvasGroup = lobbyOnlineMenu.GetComponent<CanvasGroup>();
        GroupActive(canvasGroup, !active);
        exitOnlineLobbyPopup.SetActive(active);
        SetFirstSelectable(exitOnlineLobbyPopup);
    }

    #endregion

    #region Canvas Group

    private void GroupActive(CanvasGroup canvasGroup, bool active)
    {
        canvasGroup.interactable = active;
        canvasGroup.blocksRaycasts = active;
    }

    #endregion

    #region Menu Animations

    private void OpenMenu(GameObject newMenu)
    {
        if (activeMenu != null && activeMenu != newMenu)
        {
            CloseMenu(newMenu);
        }
        else
        {
            OpenMenuTransition(newMenu);
        }
    }

    private void CloseMenu(GameObject newMenu)
    {
        if (activeMenu == lobbyOnlineMenu || activeMenu == lobbyLocalMenu
            || newMenu == lobbyOnlineMenu || newMenu == lobbyLocalMenu)
        {
            activeMenu.SetActive(false);
            newMenu.SetActive(true);
            SetFirstSelectable(newMenu);
            activeMenu = newMenu;
        }
        else
        {
            CloseMenuTransition(newMenu);
        }
    }

    private void CloseMenuTransition(GameObject newMenu)
    {
        if (activeMenu == newMenu) return;

        CanvasGroup canvasGroup = activeMenu.GetComponent<CanvasGroup>();
        GroupActive(canvasGroup, false);

        // Mueve el menú hacia arriba (1080 en Y)
        RectTransform rectTransform = activeMenu.GetComponent<RectTransform>();
        LeanTween.moveY(rectTransform, 1080, 0.5f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() =>
            {
                activeMenu.SetActive(false);
                rectTransform.anchoredPosition = new Vector2(0, 0);
                GroupActive(canvasGroup, true);
                OpenMenuTransition(newMenu);
            });
    }

    private void OpenMenuTransition(GameObject newMenu)
    {
        // Activa el nuevo menú si no es el mismo que el actual y no está activo
        if (activeMenu == newMenu) return;

        if (newMenu != null)
        {
            CanvasGroup canvasGroup = newMenu.GetComponent<CanvasGroup>();
            GroupActive(canvasGroup, false);

            RectTransform rectTransform = newMenu.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 1080);
            newMenu.SetActive(true);

            // Mueve el nuevo menú hasta el centro con efecto de rebote
            LeanTween.moveY(rectTransform, 0, 0.6f)
                .setEase(LeanTweenType.easeOutBack) // Rebote suave
                .setOnComplete(() =>
                {
                    GroupActive(canvasGroup, true);
                    activeMenu = newMenu;
                    SetFirstSelectable(newMenu);
                });
        }
    }

    #endregion

    #region Selectable

    private void SetFirstSelectable(GameObject menu)
    {
        if (menu == null || eventSystem == null) return;

        Selectable firstSelectable = menu.GetComponentInChildren<Selectable>();
        if (firstSelectable != null)
        {
            eventSystem.SetSelectedGameObject(firstSelectable.gameObject);
        }
    }

    #endregion

}
