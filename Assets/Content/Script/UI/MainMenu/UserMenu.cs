using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UserMenu : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private TextMeshProUGUI bGamesUsername;

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
    [SerializeField] private GameHistory gameHistory;
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
    [SerializeField] private Button exitBGames;

    [Header("Change Name")]
    [SerializeField] private GameObject changeNamePanel;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button change;
    [SerializeField] private Button exitChangeName;

    #region Initialize

    private void Awake()
    {
        Color color = new Color(1, 1, 1, 1);
        GameObject[] buttons = new GameObject[] { returnButton.gameObject, configButton.gameObject };
        MenuAnimation.Instance.SubscribeButtonsToEvents(buttons, color);

        color = new Color(0, 0, 0, 1);
        buttons = new GameObject[] { loginButton.gameObject, logoutButton.gameObject, changeName.gameObject };
        MenuAnimation.Instance.SubscribeButtonsToEvents(buttons, color);

        color = new Color(1f, 168 / 255f, 65 / 255f, 1f);
        buttons = new GameObject[] { change.gameObject, bGamesLogin.gameObject };
        MenuAnimation.Instance.SubscribeButtonsToEvents(buttons, color);

        buttons = new GameObject[] { exitBGames.gameObject, exitChangeName.gameObject };
        MenuAnimation.Instance.SubscribeButtonsToEvents(buttons);
    }

    private void OnEnable()
    {
        StartCoroutine(CreateGamePanel());
        LoadSettings();
        MenuAnimation.Instance.SelectObject(configButton.gameObject);
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
        ProfileUser.LoadProfile();

        username.text = ProfileUser.Username;

        if (ProfileUser.BGamesPlayer != null)
        {
            connected.SetActive(true);
            disconnected.SetActive(false);
            loginButton.gameObject.SetActive(false);
            logoutButton.gameObject.SetActive(true);
            bGamesUsername.text = ProfileUser.BGamesPlayer.name;
        }
        else
        {
            connected.SetActive(false);
            disconnected.SetActive(true);
            loginButton.gameObject.SetActive(true);
            logoutButton.gameObject.SetActive(false);
        }

        level.text = "Nivel: " + ProfileUser.Level.ToString();
        scoreAverage.text = ProfileUser.AverageScore.ToString();
        bestScore.text = ProfileUser.BestScore.ToString();
        playedGames.text = ProfileUser.PlayedGames.ToString();

        int currentXp = ProfileUser.XP;
        int xpNextLevel = ProfileUser.XPNextLevel();
        float fillValue = (float)currentXp / xpNextLevel;
        xpUser.value = fillValue;
        nextLevelXP.text = currentXp + "/" + xpNextLevel.ToString();
    }

    #endregion

    #region Config

    private void ActiveButtons(bool active)
    {
        returnButton.interactable = active;
        configButton.interactable = active;

        if (active) MenuAnimation.Instance.SelectObject(configButton.gameObject);
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

    private IEnumerator CreateGamePanel()
    {
        yield return gameHistory.GetGames();
        List<FinishGameData> finishGameData = gameHistory.finishGameData;
        foreach (FinishGameData game in finishGameData)
        {
            GameObject newPanel = Instantiate(gamePrefab, container);
            newPanel.transform.Find("Date").GetComponent<TextMeshProUGUI>().text = game.date;

            GameObject statsPanel = newPanel.transform.Find("Stats").gameObject;
            statsPanel.transform.Find("Years").GetComponent<TextMeshProUGUI>().text = game.years.ToString();
            statsPanel.transform.Find("TimePlayed").GetComponent<TextMeshProUGUI>().text = game.timePlayed.ToString();
            statsPanel.transform.Find("Topic").GetComponent<TextMeshProUGUI>().text = game.topicName;
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
            bGamesLogin.onClick.AddListener(AttemptLogin);
            MenuAnimation.Instance.SelectObject(bGamesNick.gameObject);
        }
        else
        {
            ActiveButtons(true);
            bGamesLogin.onClick.RemoveAllListeners();
        }
    }

    public void Logout()
    {
        ProfileUser.SaveBGamesPlayer(null);
        connected.SetActive(false);
        disconnected.SetActive(true);
        loginButton.gameObject.SetActive(true);
        logoutButton.gameObject.SetActive(false);

    }

    public void AttemptLogin()
    {
        bGamesLogin.interactable = false;
        string endpoint = $"/player/{bGamesNick.text}/{bGamesPass.text}";
        HttpService.Get(endpoint, HandleTryLoginResponse);
    }

    private void HandleTryLoginResponse(string response, bool success)
    {
        if (success)
        {
            if (!string.IsNullOrEmpty(response) && response != "No response received")
            {
                int id = int.Parse(response);
                FetchPlayerData(id);
                invalidMessage.SetActive(false);
                ShowBGamesMenu(false);
            }
            else
            {
                invalidMessage.SetActive(true);
            }
        }
        else
        {
            invalidMessage.SetActive(true);
        }
        bGamesLogin.interactable = true;
    }

    private void FetchPlayerData(int playerId)
    {
        string endpoint = $"/players/{playerId}";
        HttpService.Get(endpoint, HandlePlayerDataResponse);
    }

    private void HandlePlayerDataResponse(string response, bool success)
    {
        if (success)
        {
            BGamesPlayerList userDataList = JsonUtility.FromJson<BGamesPlayerList>("{\"players\":" + response + "}");
            if (userDataList.players != null && userDataList.players.Count > 0)
            {
                BGamesPlayer data = userDataList.players[0];
                ProfileUser.SaveBGamesPlayer(data);
                bGamesUsername.text = data.name;
            }
            else
            {
                invalidMessage.SetActive(true);
            }
        }
        else
        {
            invalidMessage.SetActive(true);
        }
    }

    #endregion

    #region Name Menu

    public void ShowChangeName(bool show)
    {
        changeNamePanel.SetActive(show);
        if (show)
        {
            nameInput.text = ProfileUser.Username;
            nameInput.Select();
            nameInput.ActivateInputField();
            ActiveButtons(false);
            change.onClick.AddListener(() => ChangeName());
            MenuAnimation.Instance.SelectObject(nameInput.gameObject);
        }
        else
        {
            ActiveButtons(true);
            change.onClick.RemoveAllListeners();
        }
    }

    private void ChangeName()
    {
        string name = nameInput.text;
        if (name == "" || name == ProfileUser.Username || name.Trim() == "")
        {
            ShowChangeName(false);
            return;
        }
        username.text = name;
        ProfileUser.SaveNameUser(username.text);
        ShowChangeName(false);
    }

    #endregion

}
