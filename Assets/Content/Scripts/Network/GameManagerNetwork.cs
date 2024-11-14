using System;
using System.Collections.Generic;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

public class GameManagerNetwork : NetworkBehaviour, IGameManager
{
    public static GameManagerNetwork Instance { get; private set; }

    enum GameStatus { None, Playing, Finish }

    [Header("Game Components")]
    [SerializeField] private GameData gameData;
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private UIManager uiManager;

    // Variables Game Manager en Server y Client
    [SerializeField] private Square[] squares;

    // Variables Game Manager en Server
    [SerializeField] private GameStatus status = GameStatus.None;
    [SerializeField] private List<IPlayer> players = new List<IPlayer>();
    [SerializeField] private IPlayer currPlayer;
    [SerializeField] private DateTime currTime;

    // Variables Game Manager en Client
    [SerializeField] private IPlayer localPlayer;
    private const int slotData = 3;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        InitializeSquares();
        Instance = this;
    }

    #region Methods Getters & Setters

    public GameData GameData { get => gameData; }
    public Square[] Squares { get => squares; }
    public List<IPlayer> Players { get => players; }
    public IPlayer CurrPlayer { get => currPlayer; set => currPlayer = value; }
    public IPlayer LocalPlayer { get => localPlayer; set => localPlayer = value; }

    #endregion

    #region Methods Initialization

    [Server]
    public void StartGame()
    {
        status = GameStatus.Playing;
        currTime = DateTime.Now;
        InitTurn();
    }

    private void InitializeSquares()
    {
        Transform containerSquares = GameObject.Find("Squares").transform;
        squares = new Square[containerSquares.childCount];
        for (int i = 0; i < squares.Length; i++)
            squares[i] = containerSquares.GetChild(i).GetComponent<Square>();
    }

    [Server]
    public void InitTurn()
    {
        InitializePositions();
        currPlayer = players[gameData.turnPlayer];
        UpdatePositions();
        cameraManager.CurrentCamera(CurrPlayer.Transform);
        NextPlayer();
    }

    [Server]
    public void InitializePositions()
    {
        foreach (var player in players)
        {
            player.InitializePosition();
        }
    }

    #endregion

    #region Methods Game

    [Server]
    public void NextTurn()
    {
        UpdateTurn();
        UpdateTime();
        if (gameData.initialPlayerIndex == gameData.turnPlayer) UpdateYear();
        StartCoroutine(SaveSystem.SaveGame(gameData, 3));
        cameraManager.UpdateCurrentCamera(CurrPlayer.Transform);
        NextPlayer();
    }

    [Server]
    private void UpdateTurn()
    {
        gameData.turnPlayer = (gameData.turnPlayer + 1) % players.Count;
        RpcUpdateTurnPlayer(gameData.turnPlayer);

        CurrPlayer = players[gameData.turnPlayer];
        UpdatePositions();
    }

    [Server]
    private void UpdateTime()
    {
        DateTime timeNow = DateTime.Now;
        gameData.timePlayed = timeNow - currTime;
        currTime = timeNow;

        long timePlayedMilliseconds = (long)gameData.timePlayed.TotalMilliseconds;
        RpcUpdateTimePlayed(timePlayedMilliseconds);
    }

    [Server]
    private void UpdateYear()
    {
        int newYear = gameData.currentYear + 1;
        if (newYear > gameData.yearsToPlay)
        {
            //FIXME: Devolver inversiones y procesar deudas

            status = GameStatus.Finish;
            RpcSaveFinishGame();
            SceneLoadData menu = new SceneLoadData("MainMenu");
            SceneManager.LoadGlobalScenes(menu);
        }
        else
        {
            foreach (var player in players) player.ProccessFinances();
            gameData.currentYear = newYear;
            RpcUpdateYear(newYear);
        }
    }

    [Server]
    private void UpdatePositions() => squares[CurrPlayer.Position].UpdateSquarePositions(CurrPlayer);

    [Server]
    private void NextPlayer() => StartCoroutine(CurrPlayer.CmdCreateQuestion());

    #endregion

    #region Methods Game Data

    [ServerRpc]
    public void CmdDeleteQuestion(QuestionData question)
    {
        gameData.questions.Remove(question);
        RpcDeleteQuestion(question);
    }

    [ObserversRpc]
    private void RpcDeleteQuestion(QuestionData question) => gameData.questions.Remove(question);

    [ObserversRpc]
    private void RpcUpdateTurnPlayer(int turnPlayer) => gameData.turnPlayer = turnPlayer;

    [ObserversRpc]
    private void RpcUpdateTimePlayed(long timePlayedMilliseconds)
    {
        TimeSpan timePlayed = TimeSpan.FromMilliseconds(timePlayedMilliseconds);
        gameData.timePlayed = timePlayed;
    }

    [ObserversRpc]
    private void RpcUpdateYear(int mewYear)
    {
        gameData.currentYear = mewYear;
        uiManager.UpdateYear(mewYear);
    }

    [ObserversRpc]
    private void RpcSaveFinishGame() => StartCoroutine(SaveSystem.SaveHistory(gameData, 3));

    [ObserversRpc]
    public void RpcSavePlayerData(DataPlayerNetwork player, bool isLoad)
    {
        PlayerData playerData;
        if (isLoad)
        {
            playerData = new PlayerData();
            playerData.NewPlayer(player.index, player.name, player.character, player.uid);
            gameData.playersData.Add(playerData);
        }
    }

    [ObserversRpc]
    public void RpcInitializeHUD(int year)
    {
        UIManager uiManager = FindAnyObjectByType<UIManager>();
        uiManager.InitPlayersHUD();
        uiManager.UpdateYear(year);
        StartCoroutine(SaveSystem.SaveGame(gameData, slotData));
    }

    #endregion
}




