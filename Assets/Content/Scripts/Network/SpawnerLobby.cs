
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class SpawnerLobby : NetworkBehaviour
{
    [Header("Game Data")]
    [SerializeField] private GameData data;
    [SerializeField] private Topics topics;

    [Header("Player Banner")]
    [SerializeField] private GameObject playerLobbyPrefab;
    [SerializeField] private OnlineLobby lobby;

    // Variables for Players
    private const string SCENE_GAME = "OnlineGame";
    private Dictionary<NetworkConnection, PlayerBannerNetwork> clientPanels = new();
    private int numPlayers = 0;
    private int confirmationCount = 0;
    private Dictionary<int, int> positions = new Dictionary<int, int>(){
        {0, -510},
        {1, -170},
        {2, 170},
        {3, 510}
    };

    public override void OnStartServer()
    {
        base.OnStartServer();

        Debug.Log("OnStartServer");
        NetworkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;

        //FIXME: Si existe data cargada, crea paneles jugador desconectado, y reasigna enum de posiciones
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        NetworkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
    }

    #region Methods Lobby

    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        Debug.Log("OnRemoteConnectionState");
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            AddPlayerPanel(conn);
        }
        else if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
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

    [ServerRpc(RequireOwnership = false)]
    public void CmdSavePlayers(int bundle)
    {
        if (!data.DataExists())
        {
            data.LoadCardsAndQuestions(topics.LocalTopicList[bundle]);
            List<PlayerData> players = new List<PlayerData>();

            int index = 0;
            foreach (var bannerPlayer in clientPanels.Values)
            {
                PlayerData player = new PlayerData
                {
                    Index = index,
                    UID = bannerPlayer.UID,
                    NickName = bannerPlayer.Username,
                    CharacterID = bannerPlayer.Character
                };
                players.Add(player);
                PlayerStorage.SavePlayerStorageNetwork(player.UID, index, player.NickName, player.CharacterID);
                RpcSavePlayer(player, bundle);
                index++;
            }
            data.playersData = players;
            while (confirmationCount < clientPanels.Count) { }
            LoadGameScene();
        }
        else LoadGameScene();

    }

    [ObserversRpc] //FIXME: Revisar si todos lo guardan o solo server
    public void RpcSavePlayer(PlayerData player, int bundle)
    {
        data.bundleName = topics.LocalTopicList[bundle];
        data.playersData.Add(player);
        CmdConfirmPlayerSaved();
    }

    [ServerRpc(RequireOwnership = false)]
    private void CmdConfirmPlayerSaved() => confirmationCount++;

    [Server]
    private void LoadGameScene()
    {
        SceneLoadData sld = new SceneLoadData(SCENE_GAME);
        sld.ReplaceScenes = ReplaceOption.All;
        SceneManager.LoadGlobalScenes(sld);
    }

    #endregion
}

