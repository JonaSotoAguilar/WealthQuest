using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mirror;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utp;

public class RelayService : NetworkManager
{
    public static RelayService Instance { get; private set; }

    // Transport
    private UtpTransport utpTransport;

    // Scenes
    private const string SCENE_MENU = "Menu";

    [Header("Relay Settings")]
    public string relayJoinCode = "";
    public bool exitNetwork = false;
    private bool finishGame = false;

    [Header("Data")]
    [SerializeField] private GameData data;
    [SerializeField] private CharactersDatabase charactersDB;

    [Header("Game Settings")]
    [SerializeField] private GameObject playerGamePrefab;
    public int connPlayers = 0;

    [Header("Lobby Settings")]
    [SerializeField] private GameObject bannerPrefab;
    public int connBanners = 0;
    private List<NetworkConnectionToClient> readyPlayers = new();
    public Dictionary<NetworkConnectionToClient, BannerNetwork> clientPanels = new();
    private Dictionary<int, int> positions = new Dictionary<int, int>(){
        {0, -510},
        {1, -170},
        {2, 170},
        {3, 510}
    };

    public int ReadyPlayers { get => readyPlayers.Count; }

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
        finishGame = false;
        exitNetwork = false;
        connBanners = 0;
        connPlayers = 0;
        readyPlayers.Clear();
        clientPanels.Clear();
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

        if (!sceneName.Equals(SCENE_MENU))
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

        if (SceneManager.GetActiveScene().name.Equals(SCENE_MENU))
        {
            SetupBanner(conn);
            connBanners++;
        }
        else
        {
            SetupPlayer(conn);
            connPlayers++;
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
        Debug.Log($"Cliente desconectado del servidor: {conn.connectionId}");

        if (SceneManager.GetActiveScene().name.Equals(SCENE_MENU))
        {
            RemoveBanner(conn);
            connBanners--;
            if (readyPlayers.Contains(conn))
            {
                readyPlayers.Remove(conn);
                LobbyOnline.Instance.RpcEnableStartButton(false);
                foreach (var banner in clientPanels.Values)
                {
                    banner.ReadyPlayer(false);
                }
                readyPlayers.Clear();
            }
        }
        else
        {
            connPlayers--;
            FinishGame();
        }
    }

    public override void OnStopHost()
    {
        base.OnStopHost();
        Debug.Log("Host detenido.");
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        Debug.Log("Server detenido.");

        if (SceneManager.GetActiveScene().path == offlineScene)
        {
            exitNetwork = true;
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
        Debug.Log("Cliente desconectado.");

        if (NetworkServer.active)
        {
            Debug.Log("El host sigue activo, no se cambiará de escena.");
            return;
        }

        // Si el cliente está en el juego, cambiarlo a la escena del menú
        if (SceneManager.GetActiveScene().path != offlineScene)
        {
            Debug.Log("Cliente desconectado de la partida. Volviendo al menú...");
            SceneTransition.Instance.LoadSceneNet();
        }
    }

    #endregion

    #region Lobby Scene

    public void ReadyPlayerLobby(NetworkConnectionToClient conn)
    {
        readyPlayers.Add(conn);

        // Busca el banner del jugador y actualiza el texto
        if (clientPanels.TryGetValue(conn, out BannerNetwork banner))
        {
            banner.ReadyPlayer(true);
        }
    }

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

    public void FinishGame()
    {
        if (NetworkServer.active && !finishGame)
        {
            Debug.Log("Cerrando la partida...");
            finishGame = true;
            StartCoroutine(FinishGameCoroutine());
        }
    }

    private IEnumerator FinishGameCoroutine()
    {
        SceneTransition.Instance.LoadSceneNet();
        yield return new WaitForSeconds(1f);
        StopHost();
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

            // Verificar si ya está autenticado
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("Signing in anonymously...");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            else
            {
                Debug.Log("Player is already signed in.");
            }

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