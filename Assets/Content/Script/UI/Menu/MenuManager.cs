using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
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
            activeMenu = testMenu;
        }
        else
        {
            if (activeMenu != null || activeMenu == startMenu) activeMenu.SetActive(false);
            startMenu.SetActive(true);
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
        OpenMenuFade(lobbyLocalMenu);
    }

    public void OpenLobbyOnlineMenu()
    {
        joinInput.text = "";
        activeMenu.SetActive(false);
        lobbyOnlineMenu.SetActive(true);
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

    #region Menu Animations

    private void OpenMenu(GameObject newMenu)
    {
        if (activeMenu != null)
        {
            CloseMenu(newMenu);
        }
        else
        {
            OpenNewMenu(newMenu);
        }
    }

    private void OpenMenuFade(GameObject newMenu)
    {
        if (activeMenu != null)
        {
            CloseMenu(newMenu, true);
        }
        else
        {
            OpenMenuFadeIn(newMenu);
        }
    }

    private void CloseMenu(GameObject newMenu, bool fadeIn = false)
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
                if (fadeIn)
                {
                    OpenMenuFadeIn(newMenu);
                }
                else
                {
                    OpenNewMenu(newMenu);
                }
            });
    }

    private void OpenNewMenu(GameObject newMenu)
    {
        // Activa el nuevo menú si no es el mismo que el actual y no está activo
        if (activeMenu == newMenu) return;

        if (newMenu != null)
        {
            CanvasGroup canvasGroup = newMenu.GetComponent<CanvasGroup>();

            newMenu.SetActive(true); // Activa el nuevo menú
            RectTransform rectTransform = newMenu.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 1080);

            // Mueve el nuevo menú hasta el centro con efecto de rebote
            LeanTween.moveY(rectTransform, 0, 0.6f)
                .setEase(LeanTweenType.easeOutBack) // Rebote suave
                .setOnComplete(() =>
                {
                    GroupActive(canvasGroup, true);
                    activeMenu = newMenu;
                });
        }
    }

    private void OpenMenuFadeIn(GameObject newMenu)
    {
        if (activeMenu == newMenu) return;

        if (newMenu != null)
        {
            CanvasGroup canvasGroup = newMenu.GetComponent<CanvasGroup>();

            RectTransform rectTransform = newMenu.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 0);

            // Desactiva el CanvasGroup al inicio
            canvasGroup.alpha = 0;
            GroupActive(canvasGroup, false);

            newMenu.SetActive(true);

            // Aumenta el alpha de 0 a 1 y activa la interactividad
            LeanTween.alphaCanvas(canvasGroup, 1f, 0.5f)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(() =>
                {
                    GroupActive(canvasGroup, true);
                    activeMenu = newMenu;
                });
        }
    }

    #endregion

}
