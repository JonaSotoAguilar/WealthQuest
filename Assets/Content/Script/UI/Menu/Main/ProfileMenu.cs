using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProfileMenu : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private TextMeshProUGUI bGamesUsername;
    [SerializeField] private TextMeshProUGUI bGamesPoints;

    [Header("Statistics")]
    [SerializeField] private TextMeshProUGUI level;
    [SerializeField] private TextMeshProUGUI nextLevelXP;
    [SerializeField] private Slider xpUser;
    [SerializeField] private TextMeshProUGUI scoreAverage;
    [SerializeField] private TextMeshProUGUI bestScore;
    [SerializeField] private TextMeshProUGUI playedGames;
    [SerializeField] private TextMeshProUGUI financeLevel;

    [Header("Config")]
    [SerializeField] private Button configButton;
    [SerializeField] private GameObject configPanel;
    [SerializeField] private Button configChangeNameButton;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button logoutButton;

    [Header("History")]
    [SerializeField] private GameObject gamePrefab;
    [SerializeField] private Transform container;

    [Header("bGames Status")]
    [SerializeField] private GameObject connected;
    [SerializeField] private GameObject disconnected;

    [Header("bGames Menu")]
    [SerializeField] private TMP_InputField bGamesNick;
    [SerializeField] private TMP_InputField bGamesPass;
    [SerializeField] private GameObject invalidMessage;
    [SerializeField] private Button bGamesLogin;

    [Header("Change Name")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button changeNameButton;

    #region Initialization

    private void OnEnable()
    {
        CreateGamePanel();
        LoadSettings();
    }

    private void OnDisable()
    {
        ClearScrollView();
        if (configPanel.activeSelf) CloseConfigPanel();
    }

    public void ShowPanel(bool show)
    {
        gameObject.SetActive(show);
    }

    #endregion

    #region Profile

    public void LoadSettings()
    {
        LoadData();
        LoadLevel();

        if (ProfileUser.bGamesProfile != null) LoadBGames();
        else WithoutBGames();
    }

    private void LoadData()
    {
        username.text = ProfileUser.username;
        level.text = "Nivel: " + ProfileUser.level.ToString();
        scoreAverage.text = ProfileUser.averageScore.ToString();
        bestScore.text = ProfileUser.bestScore.ToString();
        playedGames.text = ProfileUser.playedGames.ToString();
        financeLevel.text = "Nivel Financiero: " + ProfileUser.GetGrade(ProfileUser.financeLevel);
    }

    private void LoadLevel()
    {
        int currentXp = ProfileUser.xp;
        int xpNextLevel = ProfileUser.XPNextLevel();
        float fillValue = (float)currentXp / xpNextLevel;
        xpUser.value = fillValue;
        nextLevelXP.text = currentXp + "/" + xpNextLevel.ToString();
    }

    private void LoadBGames()
    {
        connected.SetActive(true);
        disconnected.SetActive(false);
        loginButton.gameObject.SetActive(false);
        logoutButton.gameObject.SetActive(true);
        bGamesUsername.text = ProfileUser.bGamesProfile.name;
        bGamesPoints.text = "Puntos: " + ProfileUser.bGamesProfile.points;
    }

    private void WithoutBGames()
    {
        connected.SetActive(false);
        disconnected.SetActive(true);
        bGamesPoints.text = "";

        if (ProfileUser.GetBGamesID() != -1)
        {
            bGamesUsername.text = "Sin conexión";
            loginButton.gameObject.SetActive(false);
            logoutButton.gameObject.SetActive(true);
        }
        else
        {
            bGamesUsername.text = "Desconectado";
            loginButton.gameObject.SetActive(true);
            logoutButton.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Config

    public void ShowConfigPanel()
    {
        ActiveConfigButton(false);

        bool show = !configPanel.activeSelf;
        CanvasGroup canvasGroup = configPanel.GetComponent<CanvasGroup>();

        if (show)
        {
            configPanel.SetActive(true);
            canvasGroup.alpha = 0;

            LeanTween.moveY(configPanel.GetComponent<RectTransform>(), -160, 0.5f)
                .setEase(LeanTweenType.easeOutBack);

            LeanTween.value(configPanel, 0, 1, 0.5f)
                .setOnUpdate((float val) =>
                {
                    canvasGroup.alpha = val;
                })
                .setEase(LeanTweenType.easeOutBack)
                .setOnComplete(() =>
                {
                    ActiveConfigButton(true);
                    configButton.interactable = true;
                });
        }
        else
        {
            LeanTween.moveY(configPanel.GetComponent<RectTransform>(), 120, 0.5f)
                .setEase(LeanTweenType.easeInBack);

            LeanTween.value(configPanel, 1, 0, 0.5f)
                .setOnUpdate((float val) =>
                {
                    canvasGroup.alpha = val;
                })
                .setEase(LeanTweenType.easeInBack)
                .setOnComplete(() =>
                {
                    configPanel.SetActive(false);
                    configButton.interactable = true;
                });
        }
    }

    private void CloseConfigPanel()
    {
        configPanel.SetActive(false);
        configButton.interactable = true;
        configPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 120);
    }

    private void ActiveConfigButton(bool active)
    {
        configButton.interactable = active;
        loginButton.interactable = active;
        logoutButton.interactable = active;
        configChangeNameButton.interactable = active;
    }

    #endregion

    #region History

    private void CreateGamePanel()
    {
        List<FinishGameData> finishGameData = new List<FinishGameData>(ProfileUser.history);
        finishGameData.Reverse();

        foreach (FinishGameData game in finishGameData)
        {
            GameObject newPanel = Instantiate(gamePrefab, container);
            newPanel.transform.Find("Date").GetComponent<TextMeshProUGUI>().text = game.date;

            GameObject statsPanel = newPanel.transform.Find("Stats").gameObject;
            statsPanel.transform.Find("Years").GetComponent<TextMeshProUGUI>().text = game.years.ToString();
            statsPanel.transform.Find("TimePlayed").GetComponent<TextMeshProUGUI>().text = game.timePlayed;
            statsPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = game.content;
            statsPanel.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = game.score.ToString();
            statsPanel.transform.Find("Grade").GetComponent<TextMeshProUGUI>().text = game.grade;

            newPanel.SetActive(true);
        }
    }

    public void ClearScrollView()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    #endregion

    #region bGames Menu

    public void ShowBGamesMenu(bool show)
    {
        bGamesNick.text = "";
        bGamesPass.text = "";
        invalidMessage.SetActive(false);
        MenuManager.Instance.OpenBGamesLoginPopup(show);
    }

    public void LogoutBGames()
    {
        ProfileUser.LogoutBGames();
        WithoutBGames();
        MenuManager.Instance.OpenBGamesLogoutPopup(false);
    }

    public void LoginBGames()
    {
        AttemptLogin();
    }

    private async void AttemptLogin()
    {
        bGamesLogin.interactable = false;
        bool success = await HttpService.Login(bGamesNick.text, bGamesPass.text);
        HandleTryLoginResponse(success);
    }

    private void HandleTryLoginResponse(bool success)
    {
        if (success)
        {
            LoadBGames();
            invalidMessage.SetActive(false);
            ShowBGamesMenu(false);
        }
        else
        {
            invalidMessage.SetActive(true);
        }
        bGamesLogin.interactable = true;
    }

    #endregion

    #region Name Menu

    public void ShowChangeName(bool show)
    {
        nameInput.text = ProfileUser.username;
        nameInput.Select();
        nameInput.ActivateInputField();
        MenuManager.Instance.OpenChangeNamePopup(show);
    }

    public void ChangeName()
    {
        changeNameButton.interactable = false;
        string name = nameInput.text;
        if (name == "" || name == ProfileUser.username || name.Trim() == "")
        {
            ShowChangeName(false);
            return;
        }
        username.text = name;
        ProfileUser.SaveNameUser(username.text);
        ShowChangeName(false);
        changeNameButton.interactable = true;
    }

    #endregion

}
