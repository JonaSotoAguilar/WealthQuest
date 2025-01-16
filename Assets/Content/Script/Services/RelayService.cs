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
using System.Collections;

public class RelayService : NetworkManager
{
    public static RelayService Instance { get; private set; }

    public enum GameState
    {
        Lobby,
        InProgress,
        Finished
    }

    // Transport
    private UtpTransport utpTransport;

    // Scenes
    private const string SCENE_MENU = "Menu";
    private const string SCENE_GAME = "OnlineBoard";

    [Header("Relay Settings")]
    public string relayJoinCode = "";

    [Header("Data")]
    [SerializeField] private GameData data;
    [SerializeField] private CharactersDatabase charactersDB;
    public GameState gameState = GameState.Lobby;

    [Header("Game Settings")]
    [SerializeField] private GameObject playerGamePrefab;
    public int connPlayers = 0;

    [Header("Lobby Settings")]
    [SerializeField] private GameObject bannerPrefab;
    public int connBanners = 0;
    public int connBannersDisconnected = 0;
    private List<BannerNetwork> bannersDisconnected = new();
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

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        RunSetupRelay();
    }

    private void RunSetupRelay()
    {
        _ = SetupRelay();
    }

    public void DefaultServer()
    {
        gameState = GameState.Lobby;
        connBanners = 0;
        connPlayers = 0;
        connBannersDisconnected = 0;
        bannersDisconnected.Clear();
        clientPanels.Clear();
    }

    public bool IsGameLobby()
    {
        return gameState == GameState.Lobby;
    }

    public void GameReady()
    {
        gameState = GameState.InProgress;
    }

    public void GameFinished()
    {
        gameState = GameState.Finished;
    }

    #endregion

    #region Server Callbacks

    // Server Start
    // Esto se invoca cuando se inicia un servidor, incluso cuando se inicia un host.
    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("Server started.");
        DefaultServer();
    }

    // Se llama en el servidor después de que se completa la carga de una escena con ServerChangeScene().
    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);
        Debug.Log("Server scene changed.");

        if (sceneName.Equals(SCENE_MENU))
        {
            Debug.Log("Server scene changed to Menu.");
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
        Debug.Log("Server connected.");
    }

    // Se llama al servidor cuando un cliente está listo (= carga la escena)
    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
        Debug.Log("Server Client ready.");

        if (SceneManager.GetActiveScene().name.Equals(SCENE_GAME))
        {
            SetupPlayer(conn);
            connPlayers++;
        }
        else if (SceneManager.GetActiveScene().name.Equals(SCENE_MENU))
        {
            if (gameState == GameState.Lobby)
            {
                SetupBanner(conn);
                connBanners++;
            }
        }
    }

    // Se llama en el servidor cuando un cliente solicita agregar el reproductor.
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        Debug.Log("Server added player.");
    }

    // Server: Client Disconnect
    // Se llama en el servidor cuando un cliente se desconecta.
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        Debug.Log($"Server disconnected. Active scene: {SceneManager.GetActiveScene().name}");

        if (SceneManager.GetActiveScene().name.Equals(SCENE_MENU))
        {
            RemoveBanner(conn);
            connBanners--;
        }
        else if (SceneManager.GetActiveScene().name.Equals(SCENE_GAME))
        {
            connPlayers--;
            Debug.Log($"Remaining players: {connPlayers}");

            // FIXME: Solucion hasta implementar sistema de reconexión
            if (NetworkServer.active)
            {
                ServerChangeScene(SCENE_MENU);
            }
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

        if (SceneManager.GetActiveScene().name.Equals(SCENE_GAME) && gameState != GameState.Finished)
        {
            SceneManager.sceneLoaded += ServerDown;
            SceneManager.LoadScene(SCENE_MENU);
        }
        else
        {
            Debug.Log("Client disconnected from Menu.");
        }
    }

    #endregion

    #region Lobby Scene

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

        if (clientPanels.Count == 0) return;

        foreach (var bannerPlayer in clientPanels.Values)
        {
            if (bannerPlayer == null) continue;
            bannerPlayer.transform.position = new Vector2(positions[index], 0);
            index++;
        }
    }

    // FIXME: disconnected banner 

    // private void LoadBanner(NetworkConnectionToClient conn)
    // {
    //     // FIXME: Revisar si el uid coincide con el del jugador


    //     // Bloquear botones de cambio de personaje

    //     // Asignar el banner al jugador
    //     clientPanels[conn] = bannersDisconnected[connBanners];
    //     NetworkServer.AddPlayerForConnection(conn, bannersDisconnected[connBanners].gameObject);
    // }

    // public void SetupBannerDisconnected()
    // {
    //     GameObject banner = Instantiate(bannerPrefab);
    //     BannerNetwork bannerNetwork = banner.GetComponent<BannerNetwork>();
    //     bannerNetwork.position = new Vector2(positions[connBannersDisconnected], 0);
    //     connBannersDisconnected++;
    // }

    #endregion

    #region Game Scene

    private void SetupPlayer(NetworkConnectionToClient conn)
    {
        string uid = "0";
        if (clientPanels.TryGetValue(conn, out BannerNetwork banner)) uid = banner.UID;
        PlayerData playerData = data.GetPlayerData(uid);

        GameObject player = Instantiate(playerGamePrefab);
        PlayerNetManager playerManager = player.GetComponent<PlayerNetManager>();
        playerManager.Data.SetPlayerData(playerData);
        playerManager.Data.Initialize();

        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public void UpdateCharacter(PlayerNetData player, int characterID)
    {
        PlayerNetManager playerManager = player.gameObject.GetComponent<PlayerNetManager>();
        GameObject character = Instantiate(charactersDB.GetModel(characterID), player.transform);
        Animator animator = character.GetComponent<Animator>();

        NetworkAnimator networkAnimator = playerManager.GetComponent<NetworkAnimator>();
        networkAnimator.animator = animator;
        playerManager.Animator = animator;
        playerManager.Movement.Animator = animator;

        GameObject characterObject = playerManager.transform.Find("Character").gameObject;
        Destroy(characterObject);
    }

    private IEnumerator InitializeGame()
    {
        yield return new WaitUntil(() => connPlayers == data.playersData.Count);
        GameNetManager.InitializeGame();
    }

    private void ServerDown(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SCENE_MENU)
        {
            SceneManager.sceneLoaded -= ServerDown;
            StartCoroutine(MessagePopup("Se ha perdido la conexión con el servidor."));
        }
    }

    public void FinishGame()
    {
        gameState = GameState.Finished;
        if (SceneManager.GetActiveScene().name.Equals(SCENE_GAME))
        {
            Debug.Log("Terminando el juego y regresando al menú...");
            if (NetworkServer.active)
            {
                ServerChangeScene(SCENE_MENU);
            }
        }
    }

    private IEnumerator MessagePopup(string message)
    {
        yield return new WaitForSeconds(1.5f);
        MenuManager.Instance.OpenMessagePopup(message);
    }

    #endregion

    #region Relay Methods

    private async Task SetupRelay()
    {
        utpTransport = GetComponent<UtpTransport>();

        // Leer argumentos de línea de comando para el puerto
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

        // Verificar conexión a Internet antes de continuar
        while (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogWarning("No internet connection. Waiting for network...");
            await Task.Delay(1000); // Espera 1 segundo antes de volver a comprobar
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
            Debug.Log("Failed to join Relay server.");
            tcs.SetResult(false);
        });

        return await tcs.Task;
    }

    #endregion

}