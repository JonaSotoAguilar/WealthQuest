using System;
using System.Collections.Generic;
using UnityEngine;

public class GameSingleManageer : MonoBehaviour
{
    private static GameSingleManageer instance;
    enum GameStatus { None, Playing, Finish }

    [Header("Components")]
    [SerializeField] private CameraManager _camera;

    [Header("Status")]
    [SerializeField] private GameData gameData;
    private GameStatus status = GameStatus.None;
    private DateTime currTime;

    // Players
    private List<PlayerSingleManager> playersLocal = new List<PlayerSingleManager>();
    private PlayerSingleManager currPlayer;

    # region Getters

    public static GameData Data { get => instance.gameData; }
    public static List<PlayerSingleManager> Players { get => instance.playersLocal; }
    public static PlayerSingleManager CurrentPlayer { get => instance.currPlayer; }
    public static PlayerSingleManager GetPlayer(string clientID) => instance.playersLocal.Find(player => player.Data.UID == clientID);

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

    public static void PlayerJoined(PlayerSingleManager player)
    {
        instance.playersLocal.Add(player);
    }

    public static void InitializeGame()
    {
        // FIXME: Cinematic select first player

        // 1. Update UI
        instance.UpdateYear(Data.currentYear);

        // 2. Position players
        instance.InitializePosition();

        // 3. Status game
        instance.status = GameStatus.Playing;
        instance.currPlayer = instance.playersLocal[Data.turnPlayer];

        // 4. Camera
        instance._camera.CurrentCamera(instance.currPlayer.transform);

        // 5. Start game
        instance.currTime = DateTime.Now;
        instance.StartTurn();
    }

    private void InitializePosition()
    {
        List<Square> squares = new List<Square>();
        List<int> positions = new List<int>();
        foreach (var player in playersLocal)
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

    private void StartTurn()
    {
        currPlayer.StartTurn();
    }

    public static void FinishTurn()
    {
        instance.UpdateTime();
        instance.NextYear();

        if (instance.status == GameStatus.Finish) return;
        instance.NextPlayer();
        instance.SaveGame(2);
        instance._camera.CurrentCamera(instance.currPlayer.transform);
        instance.StartTurn();
    }

    private void NextPlayer()
    {
        int nexTurn = (gameData.turnPlayer + 1) % gameData.playersData.Count;
        UpdateTurnPlayer(nexTurn);
        currPlayer = playersLocal[nexTurn];
    }

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
        foreach (var player in playersLocal)
            player.Data.ProccessFinances();

        UpdateYear(newYear);
    }

    private void FinishGame()
    {
        status = GameStatus.Finish;

        // 1. Calculate winner
        PlayerSingleManager winner = playersLocal[0];
        foreach (var player in playersLocal)
        {
            player.Data.SetFinalScore();
            if (player.Data.FinalScore > winner.Data.FinalScore)
                winner = player;
        }

        // 2. Announce winner (Cinematic)


        // 3. Close game (Return to main menu)
    }

    #endregion

    #region Game Data

    private void SaveGame(int slot)
    {
        StartCoroutine(SaveSystem.SaveGame(Data, slot));
    }

    private void UpdateTime()
    {
        DateTime timeNow = DateTime.Now;
        gameData.timePlayed = timeNow - currTime;
        currTime = timeNow;
    }

    private void UpdateYear(int newYear)
    {
        gameData.currentYear = newYear;
        GameUIManager.ChangeYear(gameData.currentYear);
    }

    private void UpdateTurnPlayer(int newTurn)
    {
        gameData.turnPlayer = newTurn;
    }

    #endregion
}
