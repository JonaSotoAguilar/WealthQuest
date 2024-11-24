
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.UIElements;

public class SpawnerLobby : NetworkBehaviour
{
    [Header("Game Data")]
    [SerializeField] private GameData data;
    [SerializeField] private CharactersDatabase charactersDB;

    [Header("Lobby Game")]
    [SerializeField] private GameObject playerLobbyPrefab;
    [SerializeField] private OnlineLobby lobby;
    [SerializeField] private GameObject lobbyParent;
    [SerializeField] private GameObject backgroundParent;

    [Header("Board Game")]
    [SerializeField] NetworkObject playerPrefab;

    // Variables for Players
    private string bundle;
    private Dictionary<NetworkConnection, PlayerBannerNetwork> clientPanels = new();
    private int numPlayers = 0;
    private int dataLoaded = 0;
    private bool isDataLoaded = false;
    private Dictionary<int, int> positions = new Dictionary<int, int>(){
        {0, -510},
        {1, -170},
        {2, 170},
        {3, 510}
    };

    private const string SCENE_GAME = "OnlineScene";
    private const string SCENE_MENU = "Menu";

    public override void OnStartServer()
    {
        base.OnStartServer();

        NetworkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        NetworkManager.SceneManager.OnClientLoadedStartScenes += OnClientLoadScene;
        //FIXME: Si existe data cargada, crea paneles jugador desconectado, y reasigna enum de posiciones
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        NetworkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
    }

    #region Methods Lobby

    private void OnClientLoadScene(NetworkConnection conn, bool asServer)
    {
        if (asServer)
        {
            return;
        }

        // Host
        AddPlayerPanel(conn);
    }

    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        Debug.Log($"Connection State: {args.ConnectionState}");
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            AddPlayerPanel(conn);
        }
        else if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            Debug.Log("Connection Stopped");
            RemovePlayerPanel(conn);
        }
    }

    [Server]
    private void AddPlayerPanel(NetworkConnection conn)
    {
        if (numPlayers < positions.Count)
        {
            GameObject playerLobby = Instantiate(playerLobbyPrefab);
            Transform bannerTransform = playerLobby.transform.Find("UI/BannerNetwork");
            PlayerBannerNetwork banner = bannerTransform.GetComponent<PlayerBannerNetwork>();
            banner.position.Value = new Vector2(positions[numPlayers], 0);
            Spawn(playerLobby, conn);

            clientPanels[conn] = banner;
            numPlayers++;
        }

        if (data.DataExists() && numPlayers == data.playersData.Count) lobby.RpcActiveStartButton(true);
    }

    [Server]
    private void RemovePlayerPanel(NetworkConnection conn)
    {
        if (clientPanels.TryGetValue(conn, out PlayerBannerNetwork bannerPlayer))
        {
            clientPanels.Remove(conn);
            numPlayers--;
            ReorderPanels();
        }
    }

    [Server]
    private void ReorderPanels()
    {
        int index = 0;

        foreach (var bannerPlayer in clientPanels.Values)
        {
            bannerPlayer.position.Value = new Vector2(positions[index], 0);
            index++;
        }
    }

    //FIXME: Crea los players.
    [ServerRpc(RequireOwnership = false)]
    public void CmdSavePlayers(string bundle)
    {
        if (!data.DataExists())
        {
            this.bundle = bundle;

            int index = 0;
            // Turn
            foreach (var pair in clientPanels)
            {
                Debug.Log("Creating player");
                NetworkConnection conn = pair.Key;
                PlayerBannerNetwork bannerPlayer = pair.Value;

                // Crear los datos del jugador
                PlayerData player = new PlayerData();
                player.NewPlayer(index, bannerPlayer.Username, bannerPlayer.Character, bannerPlayer.UID);
                data.playersData.Add(player);

                // Spawnear al personaje del jugador
                SpawnCharacterForPlayer(conn, player);
                index++;
            }

            // Seleccionar un jugador aleatorio basado en su UID
            int randomIndex = Random.Range(0, data.playersData.Count);
            data.turnPlayer = data.playersData[randomIndex].UID;
            data.indexTurn = randomIndex;
            Debug.Log($"El turno es {data.playersData[randomIndex].NickName} con UID: {data.turnPlayer}");

            StartCoroutine(LoadBoard());
        }
        else
        {
            Debug.LogError("Data already exists");
        }
    }

    [Server]
    private void SpawnCharacterForPlayer(NetworkConnection conn, PlayerData player)
    {
        // Instancia el objeto del jugador
        NetworkObject playerInstance = Instantiate(playerPrefab);

        // Instancia el personaje 
        GameObject character = Instantiate(charactersDB.GetModel(player.CharacterID), playerInstance.transform);
        NetworkObject characterNet = character.GetComponent<NetworkObject>();
        characterNet.SetParent(playerInstance);

        // Inicializa los datos del jugador
        PlayerNetData playerData = playerInstance.GetComponent<PlayerNetData>();
        playerData.Initialize(player.Index, player.NickName, player.CharacterID, player.UID);

        // Realiza el Spawn del objeto principal
        NetworkManager.ServerManager.Spawn(playerInstance, conn);
        NetworkManager.ServerManager.Spawn(characterNet, conn);
    }

    [Server]
    public IEnumerator LoadBoard()
    {
        Debug.Log("Loading board");
        RpcLoadQuestions(bundle);
        yield return new WaitUntil(() => dataLoaded == numPlayers);

        foreach (var bannerPlayer in clientPanels.Values)
        {
            NetworkObject banner = bannerPlayer.GetComponent<NetworkObject>();
            banner.Despawn();

            GameObject bannerParent = banner.transform.parent.gameObject;
            GameObject playerLobby = bannerParent.transform.parent.gameObject;
            NetworkObject playerLobbyNet = playerLobby.GetComponent<NetworkObject>();
            playerLobbyNet.Despawn();
            Destroy(playerLobby);
        }

        DespawnLobby();
        DestroyBackground();
        GameNetManager.InitializeGame();
    }

    [ObserversRpc(RunLocally = true)]
    private void RpcLoadQuestions(string bundle)
    {
        StartCoroutine(LoadQuestions(bundle));
    }

    [Client]
    private IEnumerator LoadQuestions(string bundle)
    {
        yield return data.LoadCardsAndQuestions(bundle);
        RpcDataLoaded();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RpcDataLoaded() => dataLoaded++;

    [Server]
    private void DespawnLobby()
    {
        lobby.Despawn();
        RpcDespawnLobby();
    }

    [ObserversRpc]
    public void RpcDespawnLobby()
    {
        Destroy(lobbyParent);
    }

    [Server]
    private void DestroyBackground()
    {
        Destroy(backgroundParent);
        RpcDestroyBackground();
    }

    [ObserversRpc]
    public void RpcDestroyBackground()
    {
        Destroy(backgroundParent);
    }

    #endregion
}

