using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyOnline : NetworkBehaviour
{
    public static LobbyOnline Instance { get; private set; }

    private const string SCENE_GAME = "OnlineBoard";

    [Header("Game Data")]
    [SerializeField] private GameData data;

    [Header("UI Elements")]
    [SerializeField] private TMP_Dropdown yearDropdown;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TextMeshProUGUI codeText;

    [Header("Game Actions")]
    [SerializeField] private TMP_Dropdown contentDropdown;
    [SerializeField] private Button startButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button returnButton;

    // Code
    [SyncVar(hook = nameof(OnChangeCode))] private string code;

    // Content
    private readonly SyncList<string> contents = new SyncList<string>();
    [SyncVar(hook = nameof(OnChangeContent))] private int selectedContent = 0;

    // Variables for Year
    [SyncVar(hook = nameof(OnChangeYear))] private int selectedYear = 0;

    #region Initialization

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        yearDropdown.onValueChanged.AddListener(OnDropdownYearValueChanged);
        contentDropdown.onValueChanged.AddListener(OnDropdownContentChanged);

        lobbyPanel.SetActive(true);
        code = RelayService.Instance.relayJoinCode;
        YearDropdown();
        PopulateBundleDropdown();

        NewGameData();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        lobbyPanel.SetActive(true);
        ConfigStartButton();
        SetupContentClient();
        SetupButtons();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        Debug.Log("OnStopClient Lobby");

        if (isServer && isClient)
        {
            Debug.Log("Stop Host");
            return;
        }
        else if (isClient && gameObject.activeInHierarchy)
        {
            Debug.Log("Stop Client");
            lobbyPanel.SetActive(false);
            contentDropdown.onValueChanged.RemoveAllListeners();
            contentDropdown.ClearOptions();
            MenuManager.Instance.OpenPopupExitOnlineLobby(false);
            MenuManager.Instance.OpenPlayMenu();
        }
    }

    public void ConfirmReturn()
    {
        MenuManager.Instance.OpenPopupExitOnlineLobby(true);
    }

    public void CancelReturn()
    {
        MenuManager.Instance.OpenPopupExitOnlineLobby(false);
    }

    public void Return()
    {
        if (isClient && isServer)
        {
            RelayService.Instance.StopHost();
        }
        else if (isClient)
        {
            RelayService.Instance.StopClient();
        }
    }

    #endregion

    #region Game data

    [Server]
    private void NewGameData()
    {
        selectedContent = 0;
        selectedYear = 0;
    }

    private void SetupButtons()
    {
        if (isClient && isServer)
        {
            contentDropdown.interactable = true;
            yearDropdown.interactable = true;
        }
        else if (isClient)
        {
            contentDropdown.interactable = false;
            yearDropdown.interactable = false;
        }
        readyButton.gameObject.SetActive(true);
        readyButton.interactable = true;
    }

    #endregion

    #region Code

    public void CopyToClipboard()
    {
        if (codeText != null && !string.IsNullOrEmpty(codeText.text))
        {
            GUIUtility.systemCopyBuffer = codeText.text;
        }
    }

    private void OnChangeCode(string oldCode, string newCode)
    {
        if (codeText != null)
        {
            codeText.text = newCode;
        }
    }

    #endregion

    #region Content

    [Server]
    private void PopulateBundleDropdown()
    {
        List<string> options = new List<string>();

        foreach (var content in ContentDatabase.localContentList)
        {
            string baseName = SaveService.ExtractNameContent(content);
            if (!string.IsNullOrEmpty(baseName))
            {
                options.Add(baseName);
            }
        }

        contents.Clear();
        contents.AddRange(options);
    }

    public void OnDropdownContentChanged(int topic)
    {
        if (topic >= 0 && topic < contents.Count)
        {
            CmdChangeContent(topic);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdChangeContent(int content)
    {
        if (content >= 0 && content < contents.Count)
        {
            selectedContent = content;
        }
    }

    private void OnChangeContent(int oldTopic, int newTopic)
    {
        if (contentDropdown != null && newTopic >= 0 && newTopic < contentDropdown.options.Count)
        {
            contentDropdown.value = newTopic;
        }
    }

    private void SetupContentClient()
    {
        contentDropdown.ClearOptions();
        foreach (var content in contents)
            contentDropdown.options.Add(new TMP_Dropdown.OptionData(content));

        if (contents.Count > 0)
        {
            contentDropdown.value = selectedContent;
            contentDropdown.captionText.text = contentDropdown.options[selectedContent].text;
        }
    }

    #endregion

    #region Year

    [Server]
    private void YearDropdown()
    {
        yearDropdown.value = 0;
        yearDropdown.interactable = true;
    }

    public void OnDropdownYearValueChanged(int year)
    {
        CmdChangeYear(year);
    }

    [Command(requiresAuthority = false)]
    private void CmdChangeYear(int year)
    {
        selectedYear = year;
    }

    private void OnChangeYear(int oldYear, int newYear)
    {
        yearDropdown.value = newYear;
    }

    #endregion

    #region Start Button

    private void ConfigStartButton()
    {
        if (isClient && isServer)
        {
            startButton.interactable = true;
        }
        else if (isClient)
        {
            startButton.interactable = false;
        }
        startButton.gameObject.SetActive(false);
        readyButton.gameObject.SetActive(true);
    }

    public void ReadyPlayer()
    {
        readyButton.interactable = false;
        CmdReadyPlayer();
    }

    [Command(requiresAuthority = false)]
    private void CmdReadyPlayer(NetworkConnectionToClient conn = null)
    {
        RelayService.Instance.ReadyPlayerLobby(conn);
        int readyPlayers = RelayService.Instance.ReadyPlayers;
        if (readyPlayers > 1 && readyPlayers == RelayService.Instance.connBanners)
            RpcEnableStartButton(true);
    }

    [ClientRpc]
    public void RpcEnableStartButton(bool enable)
    {
        readyButton.gameObject.SetActive(!enable);
        readyButton.interactable = !enable;
        startButton.gameObject.SetActive(enable);
    }

    [Server]
    public void GameReady()
    {
        int readyPlayers = RelayService.Instance.ReadyPlayers;
        if (readyPlayers > 1 && readyPlayers == RelayService.Instance.connBanners)
            RpcEnableStartButton(true);
        else
            RpcDisableStartButton();
    }

    [ClientRpc]
    public void RpcDisableStartButton()
    {
        startButton.gameObject.SetActive(false);
    }

    #endregion

    #region Start Game

    public void StartGame()
    {
        RpcReadyGame();
        contentDropdown.interactable = false;
        yearDropdown.interactable = false;
        string content = contentDropdown.options[selectedContent].text;
        CmdSavePlayersData(content);
    }

    [ClientRpc]
    private void RpcReadyGame()
    {
        returnButton.interactable = false;
    }

    [Command(requiresAuthority = false)]
    private void CmdSavePlayersData(string content)
    {
        LoadBoard(content);
    }

    [Server]
    public void LoadBoard(string content)
    {
        data.LoadContent(content);

        if (!data.DataExists())
        {
            CreateNewGameData();
            Dictionary<NetworkConnectionToClient, BannerNetwork> clientPanels = RelayService.Instance.clientPanels;
            foreach (var pair in clientPanels)
            {
                BannerNetwork bannerPlayer = pair.Value;

                PlayerData player = new PlayerData(bannerPlayer.UID, bannerPlayer.Username, bannerPlayer.Character, 1);
                data.playersData.Add(player);
            }
        }
        StartCoroutine(LoadBoardRoutine());
    }

    [Server]
    private IEnumerator LoadBoardRoutine()
    {
        RpcLoadSceneNet();
        yield return new WaitForSeconds(1.2f);
        RelayService.Instance.ServerChangeScene(SCENE_GAME);
    }

    [ClientRpc]
    private void RpcLoadSceneNet()
    {
        SceneTransition.Instance.LoadSceneNet();
    }

    private void CreateNewGameData()
    {
        data.mode = 3;
        int years = 10 + (selectedYear * 5);
        data.yearsToPlay = years;
    }

    #endregion

}