using System;
using System.Collections.Generic;

using Mirror;

using UnityEngine;
using Unity.Services.Relay.Models;
using Utp;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;
using Telepathy;
using System.Collections;

public class WQRelayManager : NetworkManager
{
    public static WQRelayManager Instance { get; private set; }

    // Transport
    private UtpTransport utpTransport;

    // Scenes
    private const string SCENE_MENU = "MenuScene";
    private const string SCENE_GAME = "Test";

    [Header("Relay Settings")]
    public string relayJoinCode = "";

    [Header("Data")]
    [SerializeField] private GameData data;
    [SerializeField] private CharactersDatabase charactersDB;

    [Header("Lobby Settings")]
    [SerializeField] private GameObject bannerPrefab;
    public int connBanners = 0;

    [Header("Game Settings")]
    [SerializeField] private GameObject playerGamePrefab;
    public int connPlayers = 0;

    // Variables for Players
    public Dictionary<NetworkConnectionToClient, BannerNetwork> clientPanels = new();
    private Dictionary<int, int> positions = new Dictionary<int, int>(){
        {0, -510},
        {1, -170},
        {2, 170},
        {3, 510}
    };

    #region Initialization

    public override void Awake()
    {
        base.Awake();

        Instance = this;

        RunSetupRelay();
    }

    private void RunSetupRelay()
    {
        _ = SetupRelay();
    }

    #endregion

    #region Server Callbacks

    // Server Start
    // Esto se invoca cuando se inicia un servidor, incluso cuando se inicia un host.
    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("Server started.");
    }

    // Se llama en el servidor después de que se completa la carga de una escena con ServerChangeScene().
    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);
        Debug.Log("Server scene changed.");

        if (sceneName.Equals(SCENE_MENU))
        {
            Debug.Log("Server scene changed to Menu.");
            playerPrefab = bannerPrefab;
            StopHost();
        }
        else if (sceneName.Equals(SCENE_GAME))
        {
            Debug.Log("Server scene changed to Game.");
            playerPrefab = playerGamePrefab;
            StartCoroutine(InitializeGame());
        }
    }

    // Server Client Connect
    // Se llama en el servidor cuando se conecta un nuevo cliente.
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
    }

    // Se llama al servidor cuando un cliente está listo (= carga la escena)
    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
        Debug.Log("Server Client ready.");

        if (SceneManager.GetActiveScene().name.Equals(SCENE_GAME))
        {
            Debug.Log("OnServerReady: Server Client ready in Game.");
            SetupPlayer(conn);
            connPlayers++;
        }
        else if (SceneManager.GetActiveScene().name.Equals(SCENE_MENU))
        {
            Debug.Log("OnServerReady: Server Client ready in Menu.");
            SetupBanner(conn);
            connBanners++;
        }
    }

    // Se llama en el servidor cuando un cliente solicita agregar el reproductor.
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        Debug.Log("Server added player.");

        // if (SceneManager.GetActiveScene().name.Equals(SCENE_GAME))
        // {
        //     Debug.Log("OnServerAddPlayer: Server added player in Game.");
        //     SetupPlayer(conn);
        // }
        // else if (SceneManager.GetActiveScene().name.Equals(SCENE_MENU))
        // {
        //     Debug.Log("OnServerAddPlayer: Server added player in Menu.");
        //     SetupBanner(conn);
        //     connBanners++;
        //     if (data.DataExists() && connBanners == data.playersData.Count)
        //     {
        //         LobbyOnline lobby = FindAnyObjectByType<LobbyOnline>();
        //         lobby.EnableStartButton();
        //     }
        // }
    }

    // Server: Client Disconnect
    // Se llama en el servidor cuando un cliente se desconecta.
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);

        if (SceneManager.GetActiveScene().name.Equals(SCENE_MENU))
        {
            RemoveBanner(conn);
            connBanners--;
        }
    }

    #endregion

    #region Client Callbacks

    // Client Start
    // Esto se invoca cuando se inicia la cliente.
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("Client started.");
    }

    // Se llama al cliente cuando se conecta a un servidor. De forma predeterminada, configura al cliente como listo y agrega un jugador.
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Debug.Log("Client connected.");
    }

    // Se llama desde ClientChangeScene inmediatamente antes de que se ejecute SceneManager.LoadSceneAsync
    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
        Debug.Log("Client changed scene.");
    }

    // Se llama a los clientes cuando una escena ha terminado de cargarse y cuando el servidor inició la carga de la escena.
    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();
        Debug.Log("Client scene changed.");
    }

    // Client Stop
    // Se llama a los clientes cuando se desconecta de un servidor.
    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        Debug.Log("Client disconnected.");
    }

    #endregion

    #region Lobby Spawner

    private void SetupBanner(NetworkConnectionToClient conn)
    {
        GameObject banner = Instantiate(bannerPrefab);
        BannerNetwork bannerNetwork = banner.GetComponent<BannerNetwork>();
        bannerNetwork.position = new Vector2(positions[connBanners], 0);
        clientPanels[conn] = bannerNetwork;

        NetworkServer.AddPlayerForConnection(conn, banner);
    }

    private void RemoveBanner(NetworkConnectionToClient conn)
    {
        if (clientPanels.TryGetValue(conn, out BannerNetwork banner))
        {
            clientPanels.Remove(conn);
            ReorderPanels();
        }
    }

    private void ReorderPanels()
    {
        int index = 0;

        foreach (var bannerPlayer in clientPanels.Values)
        {
            bannerPlayer.transform.position = new Vector2(positions[index], 0);
            index++;
        }
    }

    #endregion

    #region Game Spawner

    private void SetupPlayer(NetworkConnectionToClient conn)
    {
        string uid = "0";
        if (clientPanels.TryGetValue(conn, out BannerNetwork banner)) uid = banner.UID;
        PlayerData playerData = data.GetPlayerData(uid);

        GameObject player = Instantiate(playerGamePrefab);
        PlayerNetManager playerManager = player.GetComponent<PlayerNetManager>();
        playerManager.Data.SetPlayerData(playerData);
        playerManager.Data.Initialize();

        int characterID = playerData.CharacterID;
        if (characterID > 0)
        {
            GameObject character = Instantiate(charactersDB.GetModel(characterID), playerManager.transform);
            Animator animator = character.GetComponent<Animator>();

            NetworkAnimator networkAnimator = playerManager.GetComponent<NetworkAnimator>();
            networkAnimator.animator = animator;
            playerManager.Animator = animator;
            playerManager.Movement.Animator = animator;

            GameObject characterObject = playerManager.transform.Find("Character").gameObject;
            Destroy(characterObject);
        }

        NetworkServer.AddPlayerForConnection(conn, player);
    }

    private IEnumerator InitializeGame()
    {
        yield return new WaitUntil(() => connPlayers == data.playersData.Count);
        GameNetManager.InitializeGame();
    }

    #endregion

    # region Relay Methods

    private async Task SetupRelay()
    {
        utpTransport = GetComponent<UtpTransport>();

        // Lee argumentos de línea de comando para el puerto
        string[] args = Environment.GetCommandLineArgs();
        for (int key = 0; key < args.Length; key++)
        {
            if (args[key] == "-port" && key + 1 < args.Length)
            {
                string value = args[key + 1];
                try
                {
                    utpTransport.Port = ushort.Parse(value);
                }
                catch
                {
                    Debug.LogWarning($"Unable to parse {value} into transport Port");
                }
            }
        }

        try
        {
            // Inicializar Unity Services
            await UnityServices.InitializeAsync();

            // Iniciar sesión anónima
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Relay setup complete.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error setting up relay: {e.Message}");
        }
    }
    public ushort GetPort()
    {
        return utpTransport.Port;
    }

    public bool IsRelayEnabled()
    {
        return utpTransport.useRelay;
    }

    public void StartStandardServer()
    {
        utpTransport.useRelay = false;
        StartServer();
    }

    public void StartStandardHost()
    {
        utpTransport.useRelay = false;
        StartHost();
    }

    public void GetRelayRegions(Action<List<Region>> onSuccess, Action onFailure)
    {
        utpTransport.GetRelayRegions(onSuccess, onFailure);
    }

public async Task<bool> StartRelayHostAsync(int maxPlayers, string regionId = null)
{
    TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

    utpTransport.useRelay = true;
    utpTransport.AllocateRelayServer(maxPlayers, regionId,
    (string joinCode) =>
    {
        relayJoinCode = joinCode;
        Debug.Log($"Relay join code: {joinCode}");
        StartHost();
        tcs.SetResult(true);
    },
    () =>
    {
        UtpLog.Error("Failed to start a Relay host.");
        tcs.SetResult(false);
    });

    return await tcs.Task;
}

    public void JoinStandardServer()
    {
        utpTransport.useRelay = false;
        StartClient();
    }

    public async Task<bool> JoinRelayServerAsync(string joinCode)
    {
        relayJoinCode = joinCode;
        if (string.IsNullOrEmpty(relayJoinCode))
        {
            UtpLog.Error("Relay join code is not set.");
            return false;
        }

        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

        utpTransport.useRelay = true;
        utpTransport.ConfigureClientWithJoinCode(relayJoinCode,
        () =>
        {
            StartClient();
            tcs.SetResult(true);
        },
        () =>
        {
            UtpLog.Error("Failed to join Relay server.");
            tcs.SetResult(false);
        });

        return await tcs.Task;
    }

    #endregion
}
