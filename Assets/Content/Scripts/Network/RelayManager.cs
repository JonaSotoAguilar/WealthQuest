using FishNet.Managing;
using FishNet.Transporting.UTP;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using System.Threading.Tasks;
using FishNet.Connection;
using FishNet.Transporting;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    [Header("Network Manager")]
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private int maxNumberOfConnections = 4;

    // Variables Relay
    private string code;
    private int connectedPlayers = 0;

    public string Code { get => code; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
    }

    #region Methods Relay

    public async Task<bool> CreateRelay()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            Allocation allocation = await Relay.Instance.CreateAllocationAsync(maxNumberOfConnections);

            RelayHostData relayHostData = new RelayHostData
            {
                Key = allocation.Key,
                Port = (ushort)allocation.RelayServer.Port,
                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                IPv4Address = allocation.RelayServer.IpV4,
                ConnectionData = allocation.ConnectionData
            };

            relayHostData.JoinCode = await Relay.Instance.GetJoinCodeAsync(relayHostData.AllocationID);

            var transport = networkManager.GetComponent<FishyUnityTransport>();
            transport.SetRelayServerData(relayHostData.IPv4Address, relayHostData.Port, relayHostData.AllocationIDBytes,
                    relayHostData.Key, relayHostData.ConnectionData);

            bool start = networkManager.ServerManager.StartConnection();
            if (!start) return false;

            code = relayHostData.JoinCode;

            start = networkManager.ClientManager.StartConnection();
            if (!start)
            {
                networkManager.ServerManager.StopConnection(false);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to create Relay: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> JoinRelay(string joinCode)
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(joinCode);

            RelayJoinData relayJoinData = new RelayJoinData
            {
                Key = allocation.Key,
                Port = (ushort)allocation.RelayServer.Port,
                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                ConnectionData = allocation.ConnectionData,
                HostConnectionData = allocation.HostConnectionData,
                IPv4Address = allocation.RelayServer.IpV4,
                JoinCode = joinCode
            };

            // Configurar FishyUnityTransport con los datos del servidor Relay
            var transport = networkManager.GetComponent<FishyUnityTransport>();
            transport.SetRelayServerData(relayJoinData.IPv4Address, relayJoinData.Port, relayJoinData.AllocationIDBytes,
                relayJoinData.Key, relayJoinData.ConnectionData, relayJoinData.HostConnectionData);

            code = relayJoinData.JoinCode;

            // Iniciar la conexión del cliente
            networkManager.ClientManager.StartConnection();

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to join Relay: " + ex.Message);
            return false;
        }
    }

    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        Debug.Log("Remote connection state: " + args.ConnectionState);
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            connectedPlayers++;
        }
        else if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            connectedPlayers--;
        }
    }

    #endregion

}

public struct RelayHostData
{
    public string JoinCode;
    public string IPv4Address;
    public ushort Port;
    public Guid AllocationID;
    public byte[] AllocationIDBytes;
    public byte[] ConnectionData;
    public byte[] Key;
}

public struct RelayJoinData
{
    public string JoinCode;
    public string IPv4Address;
    public ushort Port;
    public Guid AllocationID;
    public byte[] AllocationIDBytes;
    public byte[] ConnectionData;
    public byte[] HostConnectionData;
    public byte[] Key;
}