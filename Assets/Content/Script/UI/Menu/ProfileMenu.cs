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

    [Header("Config")]
    [SerializeField] private Button returnButton;
    [SerializeField] private Button configButton;
    [SerializeField] private GameObject configPanel;
    [SerializeField] private Button changeName;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button logoutButton;

    [Header("History")]
    [SerializeField] private GameObject gamePrefab;
    [SerializeField] private Transform container;

    [Header("bGames Status")]
    [SerializeField] private GameObject connected;
    [SerializeField] private GameObject disconnected;

    [Header("bGames Menu")]
    [SerializeField] private GameObject bGamesMenu;
    [SerializeField] private TMP_InputField bGamesNick;
    [SerializeField] private TMP_InputField bGamesPass;
    [SerializeField] private GameObject invalidMessage;
    [SerializeField] private Button bGamesLogin;

    [Header("Change Name")]
    [SerializeField] private GameObject changeNamePanel;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button changeNameButton;

    #region Initialize


    private void OnEnable()
    {
        CreateGamePanel();
        LoadSettings();
    }

    private void OnDisable()
    {
        ClearScrollView();
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
        bGamesPoints.text = "Puntos bGames: " + ProfileUser.bGamesProfile.points;
    }

    private void WithoutBGames()
    {
        connected.SetActive(false);
        disconnected.SetActive(true);
        loginButton.gameObject.SetActive(true);
        logoutButton.gameObject.SetActive(false);
        bGamesUsername.text = "Desconectado";
        bGamesPoints.text = "";
    }

    #endregion

    #region Config

    private void ActiveButtons(bool active)
    {
        returnButton.interactable = active;
        configButton.interactable = active;
        changeName.interactable = active;
        loginButton.interactable = active;
        logoutButton.interactable = active;
    }

    public void ShowConfigPanel()
    {
        configButton.interactable = false;

        bool show = !configPanel.activeSelf;
        CanvasGroup canvasGroup = configPanel.GetComponent<CanvasGroup>();

        if (show)
        {
            configPanel.SetActive(true);
            canvasGroup.alpha = 0;

            LeanTween.moveY(configPanel.GetComponent<RectTransform>(), -160, 1f)
                .setEase(LeanTweenType.easeOutBack);

            LeanTween.value(configPanel, 0, 1, 1f)
                .setOnUpdate((float val) =>
                {
                    canvasGroup.alpha = val;
                })
                .setEase(LeanTweenType.easeOutBack)
                .setOnComplete(() =>
                {
                    configButton.interactable = true;
                });
        }
        else
        {
            LeanTween.moveY(configPanel.GetComponent<RectTransform>(), 120, 1f)
                .setEase(LeanTweenType.easeInBack);
            LeanTween.value(configPanel, 1, 0, 1f)
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

    #endregion

    #region History

    private void CreateGamePanel()
    {
        List<FinishGameData> finishGameData = ProfileUser.history;
        foreach (FinishGameData game in finishGameData)
        {
            GameObject newPanel = Instantiate(gamePrefab, container);
            newPanel.transform.Find("Date").GetComponent<TextMeshProUGUI>().text = game.date;

            GameObject statsPanel = newPanel.transform.Find("Stats").gameObject;
            statsPanel.transform.Find("Years").GetComponent<TextMeshProUGUI>().text = game.years.ToString();
            statsPanel.transform.Find("TimePlayed").GetComponent<TextMeshProUGUI>().text = game.timePlayed;
            statsPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = game.content;
            statsPanel.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = game.score.ToString();

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
        bGamesMenu.SetActive(show);
        if (show)
        {
            bGamesNick.text = "";
            bGamesPass.text = "";
            invalidMessage.SetActive(false);
            ActiveButtons(false);
        }
        else
        {
            ActiveButtons(true);
        }
    }

    public void LogoutBGames()
    {
        ProfileUser.LogoutBGames();
        WithoutBGames();
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
        changeNamePanel.SetActive(show);
        if (show)
        {
            nameInput.text = ProfileUser.username;
            nameInput.Select();
            nameInput.ActivateInputField();
            ActiveButtons(false);
        }
        else
        {
            ActiveButtons(true);
        }
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
