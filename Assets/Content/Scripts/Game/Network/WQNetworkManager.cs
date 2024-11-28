using System;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WQNetworkManager : NetworkManager
{
    [Header("Prefabs")]
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private GameData gameData;

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Debug.Log("Client connected.");
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        Debug.Log("Client disconnected.");
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        Debug.Log("Server received a client connection.");

        // Create player
        GameObject player = Instantiate(playerPrefab);
        //GameObject character = Instantiate(characterPrefab, player.transform);
        //Animator animator = character.GetComponent<Animator>();
        //NetworkAnimator networkAnimator = player.GetComponent<NetworkAnimator>();
        //networkAnimator.animator = animator;
        //networkAnimator.enabled = true;

        // Set Components
        PlayerNetManager playerManager = player.GetComponent<PlayerNetManager>();
        PlayerNetData data = playerManager.Data;
        //playerManager.Animator = animator;
        //playerManager.Movement.Animator = animator;

        // FIXME: Set player data
        int clientID = conn.connectionId;
        data.Initialize(clientID.ToString(), "Player " + clientID, 0);

        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        Debug.Log("Server added player.");
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        Debug.Log("Server lost a client.");
    }

}
