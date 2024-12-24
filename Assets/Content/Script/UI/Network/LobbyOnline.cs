using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyOnline : NetworkBehaviour
{
    private const string SCENE_GAME = "Test";

    [Header("Game Data")]
    [SerializeField] private GameData data;
    [SerializeField] private Content content;

    [Header("UI Elements")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TextMeshProUGUI codeText;

    [Header("Game Actions")]
    public GameObject gameMenu;
    [SerializeField] private TMP_Dropdown contentDropdown;
    [SerializeField] private Button startGame;
    [SerializeField] private GameObject returnButton;

    // Variables for Content
    private readonly SyncList<string> contents = new SyncList<string>();
    [SyncVar(hook = nameof(OnChangeContent))] private int selectedContent = 0;
    [SyncVar(hook = nameof(OnChangeCode))] private string code;

    #region Initialization

    private void Start()
    {
        GameObject[] buttons = { startGame.gameObject, returnButton };
        MenuAnimation.Instance.SubscribeButtonsToEvents(buttons);

        if (isClient && isServer)
        {
            startGame.onClick.AddListener(StartGameScene);
        }
    }

    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        lobbyPanel.SetActive(true);
        code = WQRelayManager.Instance.relayJoinCode;
        //contents.OnAdd += OnAddContent;
        PopulateBundleDropdown();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        lobbyPanel.SetActive(true);
        if (isClient && isServer) contentDropdown.interactable = true;
        else contentDropdown.interactable = false;
        SetupStartButton();

        // Sincronizar contenido del Dropdown
        contentDropdown.ClearOptions();
        foreach (var content in contents)
        {
            contentDropdown.options.Add(new TMP_Dropdown.OptionData(content));
        }

        // Asegurar que el valor inicial sea correcto
        if (contents.Count > 0)
        {
            contentDropdown.value = selectedContent; // Ajustar al valor sincronizado
            contentDropdown.captionText.text = contentDropdown.options[selectedContent].text; // Actualizar el texto del Label
        }
        else
        {
            Debug.LogWarning("No hay opciones disponibles en el Dropdown.");
        }

        Debug.Log($"OnStartClient: Selected Content: {selectedContent}");
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
            gameMenu.SetActive(true);
        }
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

    #region Topics

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

        // Seleccionar un tema predeterminado
        if (data.DataExists() && content.LocalTopicList.Contains(data.content))
        {
            selectedContent = content.LocalTopicList.IndexOf(data.content);
        }
        else
        {
            selectedContent = 0;
        }
    }

    private void OnDropdownValueChanged(int topic)
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

    #endregion

    #region Start Game

    private void SetupStartButton()
    {
        if (isClient && isServer && !data.DataExists()) startGame.interactable = true;
        else startGame.interactable = false;
    }

    public void EnableStartButton()
    {
        if (isClient && isServer) startGame.interactable = true;
    }

    public void StartGameScene()
    {
        string content = contentDropdown.options[selectedContent].text;
        CmdSavePlayersData(content);
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
        if (!data.DataExists())
        {
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

    #endregion
}