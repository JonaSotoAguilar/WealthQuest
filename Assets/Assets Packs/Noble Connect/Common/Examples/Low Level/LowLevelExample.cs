namespace NobleConnect.Examples
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;
    using NobleConnect;
    using NobleConnect.Stun;
    using NobleConnect.Turn;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Logger = Logger;

    public class LowLevelExample : MonoBehaviour
    {
        public ushort testServerPort = 42345;
        
        Peer peer;
        enum MenuState { START, CLIENT, HOST, CLIENT_HAS_ROUTE }
        MenuState menuState = MenuState.START;

        string hostIPOnClient;
        ushort hostPortOnClient;
        string hostPortStringOnClient;
        IPEndPoint clientConnectToIPv4;
        IPEndPoint clientConnecToIPv6;

        TcpListener testServer;
        TcpClient testClient;

        void Start()
        {
            Logger.logger = Debug.Log;
            Logger.logLevel = Logger.Level.Developer;
        }

        private void OnDestroy()
        {
            peer?.Dispose();
            peer = null;
            if (testServer != null) testServer.Stop();
            if (testClient != null) testClient.Close();
        }

        void Update()
        {
            peer?.Update();
        }

        void OnGUI()
        {
            switch (menuState)
            {
                case MenuState.START: StartGUI(); break;
                case MenuState.CLIENT: ClientGUI(); break;
                case MenuState.CLIENT_HAS_ROUTE: ClientWithRouteGUI(); break;
                case MenuState.HOST: HostGUI(); break;
            }
        }

        void StartGUI()
        {
            if (GUI.Button(new Rect(10, 10, 100, 60), "Host"))
            {
                FakeVOIPServer();
            }
            if (GUI.Button(new Rect(10, 70, 100, 60), "Client"))
            {
                menuState = MenuState.CLIENT;
            }
        }

        void CreatePeer()
        {
            var iceConfig = NobleConnectSettings.InitConfig();
            iceConfig.protocolType = ProtocolType.Tcp;
            iceConfig.iceServerAddress = "auto.connect.noblewhale.com";
            iceConfig.enableIPv6 = true;
            peer = new Peer(iceConfig);
        }

        void FakeVOIPServer()
        {
            testServer = new TcpListener(IPAddress.IPv6Any, testServerPort);
            testServer.Start();
            CreatePeer();
            peer.InitializeHosting((IPEndPoint)testServer.Server.LocalEndPoint, OnHostPrepared);
            ListenForIncomingConnections();
        }

        async void ListenForIncomingConnections()
        {
            try
            {
                while (testServer != null)
                {
                    TcpClient client = await testServer.AcceptTcpClientAsync();
                    ReceiveFromClient(client);
                }
            }
            catch
            {
                // Server has gone away
            }
        }

        async void ReceiveFromClient(TcpClient client)
        {
            byte[] someBytes = new byte[1024];
            try
            {
                while (client.Connected)
                {
                    int numRead = await client.GetStream().ReadAsync(someBytes, 0, someBytes.Length);
                    if (numRead == 0)
                    {
                        Debug.Log("Client disconnected");
                        break;
                    }
                    string s = Encoding.ASCII.GetString(someBytes, 0, numRead);
                    Debug.Log("Network message received: " + s);
                    await client.GetStream().WriteAsync(someBytes, 0, numRead);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to receive from client: " + ex.Message + " " + ex.StackTrace);
                 
            }
        }

        void ClientGUI()
        {
            GUI.Label(new Rect(10, 10, 100, 25), "Host IP:");
            hostIPOnClient = GUI.TextField(new Rect(110, 10, 500, 25), hostIPOnClient);
            GUI.Label(new Rect(10, 50, 100, 25), "Port:");
            hostPortStringOnClient = GUI.TextField(new Rect(110, 50, 500, 25), hostPortStringOnClient);
            bool success = ushort.TryParse(hostPortStringOnClient, out hostPortOnClient);
            if (GUI.Button(new Rect(10, 80, 100, 60), "Connect"))
            {
                CreatePeer();
                peer.InitializeClient(new IPEndPoint(IPAddress.Parse(hostIPOnClient), hostPortOnClient), OnClientPrepared);
            }
        }

        void ClientWithRouteGUI()
        {
            GUI.TextField(new Rect(110, 10, 500, 25), "Connect to IPv4: " + clientConnectToIPv4.ToString(), "label");
        }

        void HostGUI()
        {
            GUI.TextField(new Rect(10, 10, 500, 25), "Host IP: " + hostIPOnClient, "label");
            GUI.TextField(new Rect(10, 50, 500, 25), "Host Port: " + hostPortOnClient.ToString(), "label");
        }

        void OnHostPrepared(string ip, ushort port)
        {
            // Host can now receive connections
            hostIPOnClient = ip;
            hostPortOnClient = port;
            menuState = MenuState.HOST;
        }

        void OnClientPrepared(IPEndPoint routeEndPointIPv4, IPEndPoint routeEndPointIPv6)
        {
            clientConnectToIPv4 = routeEndPointIPv4;
            clientConnecToIPv6 = routeEndPointIPv6;
            menuState = MenuState.CLIENT_HAS_ROUTE;
            FakeVOIPClient(routeEndPointIPv6);
        }

        async void FakeVOIPClient(IPEndPoint voipHostAddress)
        {
            Logger.Log("Connecting client " + voipHostAddress);
            testClient = new TcpClient(voipHostAddress.AddressFamily);
            testClient.Connect(voipHostAddress);
            peer.SetLocalEndPoint((IPEndPoint)testClient.Client.LocalEndPoint);
            byte[] bytesToSend = Encoding.ASCII.GetBytes("Hello World");
            await Task.Delay(250);
            try
            {
                while (testClient.Connected)
                {
                    await testClient.GetStream().WriteAsync(bytesToSend, 0, bytesToSend.Length);
                    await testClient.GetStream().WriteAsync(bytesToSend, 0, bytesToSend.Length);
                    await testClient.GetStream().WriteAsync(bytesToSend, 0, bytesToSend.Length);
                    Debug.Log("Client sent");
                    byte[] receiveBuffer = new byte[1024];
                    int bytesReceived = await testClient.GetStream().ReadAsync(receiveBuffer, 0, receiveBuffer.Length);
                    string serverResponse = Encoding.ASCII.GetString(receiveBuffer, 0, bytesReceived);
                    Debug.Log("Client received: " + serverResponse);
                    await Task.Delay(100);
                }
            }
            catch { }
            menuState = MenuState.START;
            peer?.Dispose();
            peer = null;
        }
    }
}