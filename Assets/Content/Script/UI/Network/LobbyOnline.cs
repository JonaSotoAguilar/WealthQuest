using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyOnline : NetworkBehaviour
{
    private const string SCENE_GAME = "Test";

    [Header("Game Data")]
    [SerializeField] private GameData data;
    [SerializeField] private Topics topics;

    [Header("UI Elements")]
    [SerializeField] private GameObject gameMenu;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TextMeshProUGUI codeText;

    [Header("Game Actions")]
    [SerializeField] private TMP_Dropdown topicDropdown;
    [SerializeField] private Button startGame;

    // Variables for Topics
    private List<string> localTopics = new List<string>();
    private readonly SyncList<string> onlineTopics = new SyncList<string>();
    [SyncVar(hook = nameof(OnChangeTopic))] private int selectedTopic = 0;
    private string assetBundleDirectory;

    // Flags
    private int playersReady = 0;


    #region Initialization

    private void Start()
    {
        assetBundleDirectory = Path.Combine(Application.persistentDataPath, "AssetBundles");

        if (isClient && isServer)
        {
            Debug.Log("LobbyOnline Start");
            startGame.onClick.AddListener(StartGameScene);
        }
        Debug.Log("LobbyOnline Start End");
    }

    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("OnStartClient LobbyOnline");

        lobbyPanel.SetActive(true);
        codeText.text = WQRelayManager.Instance.relayJoinCode;

        onlineTopics.OnAdd += OnAddOnlineTopic;
        onlineTopics.OnRemove += OnRemoveTopic;
        PopulateBundleDropdown();
        SetupStartButton();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        onlineTopics.OnAdd -= OnAddOnlineTopic;
        onlineTopics.OnRemove -= OnRemoveTopic;

        //FIXME: Volver a cargar menu game
        if (this != null && gameObject.activeInHierarchy)
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

    #endregion

    #region Topics

    private void PopulateBundleDropdown()
    {
        // 1. Topics Local
        topicDropdown.ClearOptions();
        List<string> options = new List<string> { "Default" };
        options.AddRange(topics.LocalTopicList);
        topicDropdown.AddOptions(options);
        localTopics = options;

        // 2. Topics Online
        topicDropdown.interactable = false;
        if (!(isClient && isServer)) return;

        CmdUpdateTopics();
        if (data.DataExists())
        {
            int topic = topics.LocalTopicList.IndexOf(data.topicName);
            CmdChangeTopic(topic);
        }
        else
        {
            CmdChangeTopic(0);
            topicDropdown.interactable = true;
            topicDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdUpdateTopics()
    {
        onlineTopics.Clear();
        foreach (string topic in localTopics)
            onlineTopics.Add(topic);
    }

    private void OnAddOnlineTopic(int index)
    {
        string topic = onlineTopics[index];

        if (!localTopics.Contains(topic))
        {
            //FIXME: Si no tiene el topic local, lo descarga
            CmdDeleteTopic(index);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdDeleteTopic(int index)
    {
        onlineTopics.RemoveAt(index);
    }

    private void OnRemoveTopic(int index, string oldTopic)
    {
        if (selectedTopic == index)
        {
            topicDropdown.value = 0;
            if (isClient && isServer) CmdChangeTopic(0);
        }

        if (localTopics.Contains(oldTopic))
        {
            topicDropdown.options.RemoveAt(index);
            localTopics.Remove(oldTopic);
        }
    }

    private void OnDropdownValueChanged(int topic)
    {
        CmdChangeTopic(topic);
    }

    [Command(requiresAuthority = false)]
    private void CmdChangeTopic(int topic)
    {
        selectedTopic = topic;
    }

    private void OnChangeTopic(int oldTopic, int newTopic)
    {
        topicDropdown.value = newTopic;
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
        Debug.Log("StartGameScene");
        string bundle = topicDropdown.options[selectedTopic].text;
        Debug.Log("Bundle: " + bundle);
        CmdSavePlayersData(bundle);
    }

    [Command(requiresAuthority = false)]
    private void CmdSavePlayersData(string bundle)
    {
        if (data.DataExists())
        {
            RpcLoadQuestions(bundle);
            return;
        };

        Dictionary<NetworkConnectionToClient, BannerNetwork> clientPanels = WQRelayManager.Instance.clientPanels;
        foreach (var pair in clientPanels)
        {
            BannerNetwork bannerPlayer = pair.Value;

            PlayerData player = new PlayerData(bannerPlayer.UID, bannerPlayer.Username, bannerPlayer.Character);
            data.playersData.Add(player);
        }

        RpcLoadQuestions(bundle);
    }

    [ClientRpc]
    private void RpcLoadQuestions(string bundle)
    {
        StartCoroutine(RoutineLoadQuestions(bundle));
    }

    private IEnumerator RoutineLoadQuestions(string bundle)
    {
        yield return data.LoadCardsAndQuestions(bundle);
        CmdFinishLoad();
    }

    [Command(requiresAuthority = false)]
    private void CmdFinishLoad()
    {
        playersReady++;

        if (playersReady == data.playersData.Count)
        {
            WQRelayManager.Instance.ServerChangeScene(SCENE_GAME);
        }
    }

    [Server]
    public void LoadBoard()
    {
        WQRelayManager.Instance.ServerChangeScene(SCENE_GAME);
    }

    #endregion
}
