using System.Collections.Generic;
using System.IO;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnlineLobby : NetworkBehaviour
{
    [Header("Game Data")]
    [SerializeField] private GameData data;
    [SerializeField] private Topics topics;
    [SerializeField] private SpawnerLobby spawner;

    [Header("Network Manager")]
    [SerializeField] private TextMeshProUGUI codeText;
    //[SerializeField] private GameObject playMenu;
    //[SerializeField] private GameObject onlineMenu;

    [Header("Game Actions")]
    [SerializeField] private TMP_Dropdown topicDropdown;
    [SerializeField] private Button startGame;

    // Variables for AssetBundle
    private readonly SyncVar<int> selectedTopic = new SyncVar<int>();
    private string assetBundleDirectory;

    #region Methods Getters & Setters

    public TextMeshProUGUI CodeText { get => codeText; set => codeText = value; }

    #endregion

    private void Start()
    {
        assetBundleDirectory = Path.Combine(Application.persistentDataPath, "AssetBundles");
        codeText.text = RelayManager.Instance.Code;
    }

    #region Methods Server

    public override void OnStartClient()
    {
        base.OnStartClient();
        PopulateBundleDropdown();
        InitStartButton();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        // if (this != null && gameObject.activeInHierarchy)
        // {
        //     ShowPanel(false);
        //     if (onlineMenu != null) onlineMenu.SetActive(true);
        // }
    }

    #endregion

    #region Methods Panel

    public void Return()
    {
        if (IsHostInitialized)
        {
            NetworkManager.ServerManager.StopConnection(true);
        }
        else if (IsClientInitialized)
        {
            ShowPanel(false);
            //onlineMenu.SetActive(true);
            NetworkManager.ClientManager.StopConnection();
        }
    }

    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }

    #endregion

    #region Methods Topic

    private void PopulateBundleDropdown()
    {
        selectedTopic.OnChange += OnChangeTopic;
        topicDropdown.ClearOptions();
        List<string> options = new List<string> { "Default" };
        options.AddRange(topics.LocalTopicList);
        topicDropdown.AddOptions(options);

        // Juego Cargado o Nuevo
        if (IsHostInitialized && data.DataExists())
        {
            int topic = topics.LocalTopicList.IndexOf(data.bundleName);
            CmdChangeTopic(topic);
            topicDropdown.interactable = false;
        }
        else
        {
            if (IsHostInitialized)
            {
                CmdChangeTopic(0);
                topicDropdown.interactable = true;
                topicDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            }
            else topicDropdown.interactable = false;
        }
    }

    private void OnDropdownValueChanged(int value)
    {
        CmdChangeTopic(value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CmdChangeTopic(int topic)
    {
        selectedTopic.Value = topic;
    }

    private void OnChangeTopic(int oldTopic, int newTopic, bool asServer)
    {
        topicDropdown.value = newTopic;
    }

    #endregion

    #region Methods Start Game

    private void InitStartButton()
    {
        if (data.DataExists()) startGame.interactable = false;
        else if (IsHostInitialized) startGame.interactable = true;
        else startGame.interactable = false;
    }

    [ObserversRpc]
    public void RpcActiveStartButton(bool active)
    {
        if (IsHostInitialized) startGame.interactable = active;
    }

    public void StartGameScene()
    {
        string bundle = topicDropdown.options[selectedTopic.Value].text;
        if (IsHostInitialized) spawner.CmdSavePlayers(bundle);
    }

    #endregion

}