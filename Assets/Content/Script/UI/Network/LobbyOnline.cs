using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyOnline : NetworkBehaviour
{
    private const string SCENE_GAME = "OnlineBoard";

    [Header("Game Data")]
    [SerializeField] private GameData data;
    [SerializeField] private Content content;

    [Header("UI Elements")]
    [SerializeField] private TMP_Dropdown yearDropdown;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TextMeshProUGUI codeText;

    [Header("Game Actions")]
    [SerializeField] private TMP_Dropdown contentDropdown;
    [SerializeField] private Button readyButton;

    // Code
    [SyncVar(hook = nameof(OnChangeCode))] private string code;

    // Content
    private readonly SyncList<string> contents = new SyncList<string>();
    [SyncVar(hook = nameof(OnChangeContent))] private int selectedContent = 0;

    // Variables for Year
    [SyncVar(hook = nameof(OnChangeYear))] private int selectedYear = 0;

    // Variables control
    [SyncVar] private bool newGame = true;
    [SyncVar] private int readyPlayers = 0;
    private bool readyLocal = false;

    #region Initialization

    //FIXME: Reviusar Load Game y reconexion

    public override void OnStartServer()
    {
        base.OnStartServer();

        yearDropdown.onValueChanged.AddListener(OnDropdownYearValueChanged);
        contentDropdown.onValueChanged.AddListener(OnDropdownContentChanged);

        lobbyPanel.SetActive(true);
        code = WQRelayManager.Instance.relayJoinCode;
        YearDropdown();
        PopulateBundleDropdown();

        // New game o load game
        if (data.DataExists()) LoadGameData();
        else NewGameData();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        lobbyPanel.SetActive(true);
        SetupContentClient();
        SetupButtons();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (contentDropdown != null)
        {
            contentDropdown.onValueChanged.RemoveAllListeners();
            contentDropdown.ClearOptions();
        }

        if (gameObject.activeInHierarchy)
        {
            MenuManager.Instance.OpenPopupExitOnlineLobby(false);
            MenuManager.Instance.OpenPlayMenu();
        }
    }

    public void ConfirmReturn()
    {
        if (readyLocal) CmdDisconnectPlayer();
        MenuManager.Instance.OpenPopupExitOnlineLobby(true);
    }

    public void CancelReturn()
    {
        if (readyLocal) ReadyPlayer();
        MenuManager.Instance.OpenPopupExitOnlineLobby(false);
    }

    public void Return()
    {
        if (isClient && isServer)
        {
            WQRelayManager.Instance.StopHost();
        }
        else if (isClient)
        {
            WQRelayManager.Instance.StopClient();
        }
    }

    #endregion

    #region Game data

    [Server]
    private void NewGameData()
    {
        newGame = true;
        selectedContent = 0;
        selectedYear = 0;
    }

    [Server]
    private void LoadGameData()
    {
        newGame = false;
        selectedContent = content.LocalTopicList.IndexOf(data.content);
        selectedYear = (data.yearsToPlay - 10) / 5;
        // FIXME: Cargar banners jugadores como desconectados
    }

    private void SetupButtons()
    {
        if (isClient && isServer)
        {
            contentDropdown.interactable = newGame;
            yearDropdown.interactable = newGame;
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

        foreach (var topic in content.LocalTopicList)
        {
            string baseName = SaveSystem.ExtractName(topic);
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

    public void ReadyPlayer()
    {
        readyLocal = true;
        readyButton.interactable = false;
        CmdReadyPlayer();
    }

    [Command(requiresAuthority = false)]
    private void CmdReadyPlayer()
    {
        readyPlayers++;
        Debug.Log("Player ready:" + readyPlayers);
        if (readyPlayers <= 1) return;
        if (newGame && readyPlayers == WQRelayManager.Instance.connBanners)
            StartGameScene();
        else if (readyPlayers == data.playersData.Count)
            StartGameScene();
    }

    [Command(requiresAuthority = false)]
    private void CmdDisconnectPlayer()
    {
        readyPlayers--;
        Debug.Log("Player disconnected:" + readyPlayers);
    }

    #endregion

    #region Start Game

    public void StartGameScene()
    {
        RpcDesactiveReturn();
        string content = contentDropdown.options[selectedContent].text;
        CmdSavePlayersData(content);
    }

    [ClientRpc]
    private void RpcDesactiveReturn()
    {
        readyButton.interactable = false;
    }

    [Command(requiresAuthority = false)]
    private void CmdSavePlayersData(string content)
    {
        StartCoroutine(LoadBoard(content));
    }

    [Server]
    public IEnumerator LoadBoard(string content)
    {
        yield return data.LoadContent(content);

        //FIXME: Revisar NewGame y LoadGame
        if (!data.DataExists())
        {
            CreateNewGameData();
            Dictionary<NetworkConnectionToClient, BannerNetwork> clientPanels = WQRelayManager.Instance.clientPanels;
            foreach (var pair in clientPanels)
            {
                BannerNetwork bannerPlayer = pair.Value;

                PlayerData player = new PlayerData(bannerPlayer.UID, bannerPlayer.Username, bannerPlayer.Character);
                data.playersData.Add(player);
            }
        }
        WQRelayManager.Instance.ServerChangeScene(SCENE_GAME);
    }

    private void CreateNewGameData()
    {
        data.mode = 3;
        int years = 10 + (selectedYear * 5);
        data.yearsToPlay = years;
    }

    #endregion

}