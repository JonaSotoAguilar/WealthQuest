using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

public class GameOnline : NetworkBehaviour, IGameManager
{
    public static GameOnline Instance { get; private set; }

    [Header("Game Components")]
    [SerializeField] private GameData data;
    [SerializeField] private CameraManager _camera;
    [SerializeField] private UIOnline ui;

    // Game Status
    enum GameStatus { None, Playing, Finish }
    [SerializeField] private GameStatus status = GameStatus.None;
    [SerializeField] private List<IPlayer> players = new List<IPlayer>();
    [SerializeField] private IPlayer currPlayer;
    [SerializeField] private DateTime currTime;

    // Variables statics
    [SerializeField] private Square[] squares;
    private const int slotData = 3;
    private const string SCENE_MENU = "Menu";

    #region Getters & Setters

    public List<IPlayer> Players { get => players; }
    public Square[] Squares { get => squares; }
    public UIOnline UI { get => ui; }

    #endregion

    #region Initialization

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

    private void InitializeSquares()
    {
        Transform containerSquares = GameObject.Find("Squares").transform;
        squares = new Square[containerSquares.childCount];
        for (int i = 0; i < squares.Length; i++)
            squares[i] = containerSquares.GetChild(i).GetComponent<Square>();
    }

    [Server]
    public void StartGame()
    {
        UpdateYearUI();

        status = GameStatus.Playing;
        currTime = DateTime.Now;
        //currPlayer = players[data.turnPlayer];
        _camera.CurrentCamera(currPlayer.Transform);

        SaveGame();
        InitializeNewTurn();
    }

    [Server]
    private void UpdateYearUI()
    {
        ui.UpdateYear(data.currentYear);
        RpcUpdateYear(data.currentYear);
    }

    [ObserversRpc]
    private void RpcUpdateYear(int year)
    {
        ui.UpdateYear(year);
    }

    [Server]
    public void AddPlayer(IPlayer player)
    {
        if (players.Contains(player)) return;
        players.Add(player);
    }

    #endregion

    #region Game Actions

    [Server]
    public void NextTurn()
    {
        UpdateTurn();
        UpdateTime();

        //if (data.initialPlayerIndex == data.turnPlayer) UpdateYear();
        if (status == GameStatus.Finish) return;

        StartCoroutine(SaveSystem.SaveGame(data, 3));
        _camera.UpdateCurrentCamera(currPlayer.Transform);
        InitializeNewTurn();
    }

    [Server]
    private void UpdateTurn()
    {
        //data.turnPlayer = (data.turnPlayer + 1) % players.Count;
        //currPlayer = players[data.turnPlayer];
    }

    [Server]
    private void UpdateTime()
    {
        DateTime timeNow = DateTime.Now;
        data.timePlayed = timeNow - currTime;
        currTime = timeNow;
    }

    [Server]
    private void UpdateYear()
    {
        int newYear = data.currentYear + 1;
        if (newYear > data.yearsToPlay)
        {
            // FIXME: Devolver inversiones y procesar deudas

            // Cambiar estado del juego
            status = GameStatus.Finish;

            // Notificar a los clientes que deben guardar y cambiar a menú
            RpcSaveFinishGame();

            // FIXME: Cerrar el servidor
        }
        else
        {
            foreach (var player in players)
                player.ProccessFinances();

            data.currentYear = newYear;
            UpdateYearUI();
        }
    }

    [Server]
    private void InitializeNewTurn()
    {
        currPlayer.CreateQuestion();
    }

    #endregion

    #region Methods Data

    [ServerRpc(RequireOwnership = false)]
    public void CmdDeleteQuestion(QuestionData question) => data.questions.Remove(question);

    [Server]
    public void SaveGame() => StartCoroutine(SaveSystem.SaveGame(data, slotData));

    [ObserversRpc(BufferLast = true)]
    private void RpcSaveFinishGame() => StartCoroutine(SaveSystem.SaveHistory(data, 3));

    #endregion

}
