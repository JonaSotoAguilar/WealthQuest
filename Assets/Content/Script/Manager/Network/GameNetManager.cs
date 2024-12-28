using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class GameNetManager : NetworkBehaviour
{
    private static GameNetManager instance;
    enum GameStatus { None, Playing, Finish }

    [Header("Components")]
    [SerializeField] private CameraManager _camera;

    [Header("Status")]
    [SerializeField] private GameData gameData;
    private GameStatus status = GameStatus.None;
    private DateTime currTime;

    // Players
    private List<PlayerNetManager> playersNet = new List<PlayerNetManager>();
    private PlayerNetManager currPlayer;

    // SyncVars
    [SyncVar(hook = nameof(OnChangeYear))] private int currentYear = 0;
    [SyncVar(hook = nameof(OnChangeTurnPlayer))] private int turnPlayer = 0;

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
        // FIXME: Cinematic select first player

        // 1. Update UI
        instance.UpdateYear(Data.currentYear);

        // 2. Position players
        instance.InitializePosition();

        // 3. Status game
        instance.status = GameStatus.Playing;
        instance.currPlayer = instance.playersNet[Data.turnPlayer];

        // 4. Camera
        instance._camera.CurrentCamera(instance.currPlayer.transform);

        // 5. Start game
        instance.currTime = DateTime.Now;
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
        currPlayer.StartTurn();
    }

    [Server]
    public static void FinishTurn()
    {
        Debug.Log("Finish Turn");
        instance.UpdateTime();
        instance.NextYear();

        if (instance.status == GameStatus.Finish) return;
        instance.NextPlayer();
        instance.SaveGame(3);
        instance._camera.CurrentCamera(instance.currPlayer.transform);
        Debug.Log("Next Player: " + instance.currPlayer.Data.UID);
        instance.StartTurn();
    }

    [Server]
    private void NextPlayer()
    {
        Debug.Log("Next Player");
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
        status = GameStatus.Finish;

        // 1. Calculate winner
        PlayerNetManager winner = playersNet[0];
        foreach (var player in playersNet)
        {
            player.Data.SetFinalScore();
            if (player.Data.FinalScore > winner.Data.FinalScore)
                winner = player;
        }

        // 2. Announce winner (Cinematic)


        // 3. Close game (Return to main menu)
    }

    #endregion

    #region Game Data (Revisar)

    [Server]
    private void SaveGame(int slot)
    {
        StartCoroutine(SaveSystem.SaveGame(Data, slot));
    }

    [Server]
    private void UpdateTime()
    {
        DateTime timeNow = DateTime.Now;
        gameData.timePlayed = timeNow - currTime;
        currTime = timeNow;
    }

    [Server]
    private void UpdateYear(int newYear)
    {
        currentYear = newYear;
        gameData.currentYear = newYear;
    }

    private void OnChangeYear(int oldYear, int newYear)
    {
        GameUIManager.ChangeYear(gameData.currentYear);
    }

    [Server]
    private void UpdateTurnPlayer(int newTurn)
    {
        turnPlayer = newTurn;
        gameData.turnPlayer = newTurn;
    }

    private void OnChangeTurnPlayer(int oldTurn, int newTurn)
    {
        gameData.turnPlayer = newTurn;
    }

    #endregion
}