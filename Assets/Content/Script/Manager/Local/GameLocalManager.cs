using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class GameLocalManager : MonoBehaviour
{
    private static GameLocalManager instance;
    enum GameStatus { None, Playing, Finish }

    [Header("Components")]
    [SerializeField] private CameraManager _camera;
    [SerializeField] private PlayableDirector cinematicDirector;

    [Header("Status")]
    [SerializeField] private GameData gameData;
    private GameStatus status = GameStatus.None;
    private DateTime currentTime;

    // Players
    private List<PlayerLocalManager> playersLocal = new List<PlayerLocalManager>();
    private PlayerLocalManager currPlayer;

    # region Getters

    public static GameData Data { get => instance.gameData; }
    public static List<PlayerLocalManager> Players { get => instance.playersLocal; }
    public static PlayerLocalManager CurrentPlayer { get => instance.currPlayer; }
    public static PlayerLocalManager GetPlayer(string clientID) => instance.playersLocal.Find(player => player.Data.UID == clientID);

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

    public static void PlayerJoined(PlayerLocalManager player)
    {
        instance.playersLocal.Add(player);
    }

    public static void InitializeGame()
    {
        //instance.StartIntroCinematic();
        StartGame();
    }

    private static void StartGame()
    {
        // 1. Active and Update UI
        instance.ActiveUI();
        instance.UpdateYear(Data.currentYear);

        // 2. Position players
        instance.InitializePosition();

        // 3. Status game
        instance.status = GameStatus.Playing;
        instance.currPlayer = instance.playersLocal[Data.turnPlayer];

        // 4. Camera
        instance._camera.CurrentCamera(instance.currPlayer.transform);

        // 5. Start game
        instance.currentTime = DateTime.Now;
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

    private void ActiveUI()
    {
        GameUIManager.ShowPanel(true);
    }

    #endregion

    #region Turn Management

    private void StartTurn()
    {
        GameUIManager.SetPlayerTurn(currPlayer.Data.UID);
        currPlayer.StartTurn();
    }

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

    private void NextPlayer()
    {
        GameUIManager.ResetPlayerTurn(currPlayer.Data.UID);
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
            status = GameStatus.Finish;
            instance.FinishGame();
            return;
        }
        foreach (var player in playersLocal)
            player.Data.ProccessFinances();

        UpdateYear(newYear);
    }

    private void FinishGame()
    {
        // 1. Calculate winner
        PlayerLocalManager winner = playersLocal[0];
        foreach (var player in playersLocal)
        {
            player.Data.SetFinalScore();
            if (player.Data.FinalScore > winner.Data.FinalScore)
                winner = player;
        }

        // 2. Announce winner (Cinematic)


        // 3. Save History
        SaveHistory();

        // 4. Close game (Return to main menu)
        LoadMenu();
    }

    private void SaveHistory()
    {
        int score = GetPlayer(ProfileUser.uid).Data.FinalScore;
        FinishGameData finishData = new FinishGameData(gameData.currentYear, gameData.timePlayed, gameData.content, score);
        int slotData = gameData.mode == 0 ? 1 : 2;
        ProfileUser.SaveGame(finishData, slotData);
    }

    private void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    #endregion

    #region Game Data

    private void SaveGame()
    {
        int slot = gameData.mode == 0 ? 1 : 2;
        StartCoroutine(SaveSystem.SaveGame(Data, slot));
    }

    private void UpdateTime()
    {
        // Calcular tiempo transcurrido
        DateTime timeNow = DateTime.Now;
        TimeSpan currentSpan = timeNow - currentTime;
        currentTime = timeNow;

        // Actualizar tiempo jugado
        TimeSpan totalSpan = TimeSpan.Parse(gameData.timePlayed);
        TimeSpan timeSpan = totalSpan + currentSpan;
        gameData.timePlayed = timeSpan.ToString(@"hh\:mm\:ss");
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

    #region Cinematic

    private void StartIntroCinematic()
    {
        if (cinematicDirector != null)
        {
            cinematicDirector.Play(); // Reproduce la cinemática

            // Registrar un evento para cuando termine
            cinematicDirector.stopped += OnIntroCinematicEnd;
        }
        else
        {
            StartGame(); // Si no hay cinemática, comienza el juego directamente
        }
    }

    private void OnIntroCinematicEnd(PlayableDirector director)
    {
        if (director == cinematicDirector)
        {
            // Desregistrar el evento
            cinematicDirector.stopped -= OnIntroCinematicEnd;

            // Continuar con el flujo del juego
            StartGame();
        }
    }

    #endregion

}
