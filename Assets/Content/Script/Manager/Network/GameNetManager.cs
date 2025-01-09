using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class GameNetManager : NetworkBehaviour
{
    private static GameNetManager instance;
    enum GameStatus { None, Playing, Finish }

    [Header("Components")]
    [SerializeField] private CameraManager _camera;
    [SerializeField] private PlayableDirector cinematicDirector;

    [Header("Status")]
    [SerializeField] private GameData gameData;
    private GameStatus status = GameStatus.None;
    private DateTime currentTime;

    // Players
    private List<PlayerNetManager> playersNet = new List<PlayerNetManager>();
    private PlayerNetManager currPlayer;
    private int turnPlayer = 0;

    // SyncVars
    [SyncVar] private string content = "Default";
    [SyncVar] private string timePlayed = "00:00:00";
    [SyncVar(hook = nameof(OnChangeYear))] private int currentYear = 0;

    # region Getters

    public static GameData Data { get => instance.gameData; }
    public static List<PlayerNetManager> Players { get => instance.playersNet; }
    public static PlayerNetManager CurrentPlayer { get => instance.currPlayer; }
    public static PlayerNetManager GetPlayer(string clientID) => instance.playersNet.Find(player => player.Data.UID == clientID);
    public static int CurrentYear { get => instance.currentYear; }
    public static int TurnPlayer { get => instance.turnPlayer; }

    #endregion

    #region Initialization

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public static void PlayerJoined(PlayerNetManager player)
    {
        instance.playersNet.Add(player);
    }

    [Server]
    public static void InitializeGame()
    {
        instance.StartIntroCinematic();
    }

    [Server]
    public static void StartGame()
    {
        // 1. Active and Update UI
        instance.RpcActiveUI();
        instance.UpdateYear(Data.currentYear);

        // 2. Position players
        instance.InitializePosition();

        // 3. Status game
        instance.status = GameStatus.Playing;
        instance.currPlayer = instance.playersNet[Data.turnPlayer];

        // 4. Camera
        instance._camera.CurrentCamera(instance.currPlayer.transform);

        // 5. Start game
        instance.content = instance.gameData.content;
        instance.currentTime = DateTime.Now;
        instance.StartTurn();
    }

    [Server]
    private void InitializePosition()
    {
        List<Square> squares = new List<Square>();
        List<int> positions = new List<int>();
        foreach (var player in playersNet)
        {
            int pos = player.Data.Position;
            Square currSquare = SquareManager.Squares[pos];
            currSquare.players.Add(player.Movement);
            positions.Add(pos);
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

    #region Turn Management

    [Server]
    private void StartTurn()
    {
        RpcNextPlayer(currPlayer.Data.UID);
        currPlayer.StartTurn();
    }

    [Server]
    public static void FinishTurn()
    {
        instance.UpdateTime();
        instance.NextYear();

        if (instance.status == GameStatus.Finish) return;
        instance.NextPlayer();
        instance.SaveGame();
        instance._camera.CurrentCamera(instance.currPlayer.transform);
        instance.StartTurn();
    }

    [Server]
    private void NextPlayer()
    {
        RpcResetPlayerHUD(currPlayer.Data.UID);
        int nexTurn = (gameData.turnPlayer + 1) % gameData.playersData.Count;
        UpdateTurnPlayer(nexTurn);
        currPlayer = playersNet[nexTurn];
    }

    [Server]
    private void NextYear()
    {
        int nextTurn = (gameData.turnPlayer + 1) % gameData.playersData.Count;
        if (nextTurn != gameData.initialPlayerIndex) return;

        int newYear = gameData.currentYear + 1;

        if (newYear > gameData.yearsToPlay)
        {
            status = GameStatus.Finish;
            instance.FinishGame();
            return;
        }
        foreach (var player in playersNet)
            player.Data.ProccessFinances();

        UpdateYear(newYear);
    }

    [Server]
    private void FinishGame()
    {
        // 1. Calculate winner
        PlayerNetManager winner = playersNet[0];
        foreach (var player in playersNet)
        {
            player.Data.SetFinalScore();
            if (player.Data.FinalScore > winner.Data.FinalScore)
                winner = player;
        }

        // 2. Announce winner (Cinematic)


        // 3. Save History
        RpcSaveHistory();

        // 4. Close game (Return to main menu)
        LoadMenu();
    }

    [ClientRpc]
    private void RpcSaveHistory()
    {
        int score = GetPlayer(ProfileUser.uid).Data.FinalScore;
        FinishGameData finishData = new FinishGameData(currentYear, timePlayed, content, score);
        ProfileUser.SaveGame(finishData, 3);
    }

    [Server]
    private void LoadMenu()
    {
        WQRelayManager.Instance.ServerChangeScene("Menu");
    }

    #endregion

    #region UI

    [ClientRpc]
    private void RpcResetPlayerHUD(string clientID)
    {
        GameUIManager.ResetPlayerTurn(clientID);
    }

    [ClientRpc]
    private void RpcNextPlayer(string clientID)
    {
        GameUIManager.SetPlayerTurn(clientID);
    }

    [ClientRpc]
    private void RpcActiveUI()
    {
        GameUIManager.ShowPanel(true);
    }

    #endregion

    #region Game Data

    [Server]
    private void SaveGame()
    {
        StartCoroutine(SaveSystem.SaveGame(Data, 3));
    }

    [Server]
    private void UpdateTime()
    {
        // Calcular tiempo transcurrido
        DateTime timeNow = DateTime.Now;
        TimeSpan currentSpan = timeNow - currentTime;
        currentTime = timeNow;

        // Actualizar tiempo jugado
        TimeSpan totalSpan = TimeSpan.Parse(gameData.timePlayed);
        TimeSpan timeSpan = totalSpan + currentSpan;
        timePlayed = timeSpan.ToString(@"hh\:mm\:ss");
        gameData.timePlayed = timePlayed;
    }

    [Server]
    private void UpdateYear(int newYear)
    {
        currentYear = newYear;
        gameData.currentYear = newYear;
    }

    private void OnChangeYear(int oldYear, int newYear)
    {
        GameUIManager.ChangeYear(newYear);
    }

    [Server]
    private void UpdateTurnPlayer(int newTurn)
    {
        turnPlayer = newTurn;
        gameData.turnPlayer = newTurn;
    }

    #endregion

    #region Cinematic

    [Server]
    private void StartIntroCinematic()
    {
        if (cinematicDirector != null)
        {
            cinematicDirector.Play();
            cinematicDirector.stopped += OnIntroCinematicEnd;
        }
        else
        {
            if (isClient && isServer) StartGame();
        }
    }

    private void OnIntroCinematicEnd(PlayableDirector director)
    {
        if (director == cinematicDirector)
        {
            cinematicDirector.stopped -= OnIntroCinematicEnd;
            StartGame();
        }
    }

    #endregion

}