using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Demo.AdditiveScenes;
using FishNet.Object;
using UnityEngine;

public class GameNetManager : NetworkBehaviour
{
    private static GameNetManager instance;
    [SerializeField] private GameData gameData;
    [SerializeField] private Square[] squares;

    // Game Components
    [SerializeField] private CameraManager _camera;

    // Game status
    enum GameStatus { None, Playing, Finish }
    [SerializeField] private GameStatus status = GameStatus.None;
    [SerializeField] private DateTime currTime;

    private Dictionary<string, PlayerNetManager> playersNet = new Dictionary<string, PlayerNetManager>();
    private PlayerNetManager currPlayer;

    public static GameData Data { get => instance.gameData; }
    public static Square[] Squares { get => instance.squares; }


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        InitializeSquares();
    }

    private void InitializeSquares()
    {
        Transform containerSquares = GameObject.Find("Squares").transform;
        squares = new Square[containerSquares.childCount];
        for (int i = 0; i < squares.Length; i++)
            squares[i] = containerSquares.GetChild(i).GetComponent<Square>();
    }


    public static void PlayerJoined(string clientID, PlayerNetManager player)
    {
        instance.playersNet.Add(clientID, player);
    }

    public static PlayerNetManager GetPlayer(string clientID)
    {
        return instance.playersNet[clientID];
    }

    [Server]
    public static void InitializeGame()
    {
        //1. Update year
        GameUINetManager.UpdateYear(Data.currentYear);
        //2. Position players
        instance.InitializePosition();
        //3. Status game
        instance.status = GameStatus.Playing;
        instance.currTime = DateTime.Now;

        instance.currPlayer = instance.playersNet[Data.turnPlayer];
        instance._camera.CurrentCamera(instance.currPlayer.transform);
        //4. Start game
        instance.StartTurn();
    }

    [Server]
    private void StartTurn()
    {
        currPlayer.StartTurn();
    }

    [Server]
    private void NextTurn()
    {
        int nextIndex = (Data.indexTurn + 1) % Data.playersData.Count;
        Data.turnPlayer = Data.playersData[nextIndex].UID;
        Data.indexTurn = nextIndex;
    }

    #region Positions


    [Server]
    private void InitializePosition()
    {
        List<Square> squares = new List<Square>();
        List<int> positions = new List<int>();
        foreach (var player in playersNet.Values)
        {
            int pos = player.Data.Position;
            Debug.Log("Player position: " + pos);
            Square currSquare = Squares[pos];
            currSquare.players.Add(player.Movement);
            positions.Add(pos);
            // Guarda la casilla si no se encuentra en la lista
            if (!squares.Contains(currSquare)) squares.Add(currSquare);
        }

        int index = 0;
        foreach (var square in squares)
        {
            square.UpdateCornerPositions(positions[index]);
            index++;
        }
    }

    #endregion


}
