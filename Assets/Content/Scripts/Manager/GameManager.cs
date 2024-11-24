using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using FishNet.Object.Synchronizing;

public class GameManager : MonoBehaviour, IGameManager
{
    public static GameManager Instance { get; private set; }

    [Header("Game Components")]
    [SerializeField] private GameData gameData; // Actualizar datos del juego
    [SerializeField] private CameraManager cameras;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private Square[] squares;

    [Header("Player")]
    [SerializeField] private List<IPlayer> players;
    [SerializeField] private IPlayer currPlayer;
    private DateTime starTime;

    public Square[] Squares { get => squares; set => squares = value; }
    public GameData GameData { get => gameData; set => gameData = value; }
    public List<IPlayer> Players { get => players; set => players = value; }
    public IPlayer CurrPlayer { get => currPlayer; set => currPlayer = value; }
    public IPlayer LocalPlayer { get => currPlayer; set => currPlayer = value; }

    // Singleton
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeSquares();
        Players = new List<IPlayer>();
        DateTime starTime = DateTime.Now;
    }

    private void InitializeSquares()
    {
        Transform containerSquares = GameObject.Find("Squares").transform;
        squares = new Square[containerSquares.childCount];
        for (int i = 0; i < squares.Length; i++)
            squares[i] = containerSquares.GetChild(i).GetComponent<Square>();
    }

    public void InitTurn()
    {
        //currentPlayer = players[gameData.TurnPlayer];
        UpdatePositions();
        //cameras.CurrentCamera(currentPlayer.transform);
        // StartCoroutine(currentPlayer.InitQuestion());
    }

    public IEnumerator UpdateTurn()
    {
        NextPlayer();
        UpdateTime();
        //if (gameData.initialPlayerIndex == gameData.turnPlayer) yield return FinishYear();
        yield return SaveSystem.SaveGame(gameData, 2);
        //yield return cameras.UpdateCurrentCamera(currentPlayer.transform);
        NextTurn();
    }

    private void NextTurn()
    {
        //StartCoroutine(currentPlayer.InitQuestion());
    }

    private void NextPlayer()
    {
        //gameData.turnPlayer = (gameData.turnPlayer + 1) % players.Count;
        //currentPlayer = players[gameData.TurnPlayer];
        UpdatePositions();
    }

    private void UpdateTime()
    {
        DateTime timeNow = DateTime.Now;
        gameData.timePlayed = timeNow - starTime;
        starTime = timeNow;
    }

    private IEnumerator FinishYear()
    {
        int newYear = gameData.currentYear + 1;
        if (newYear > gameData.yearsToPlay)
        {
            yield return SaveSystem.SaveHistory(gameData, 2);
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            foreach (var player in players)
            {
                player.ProccessFinances();
            }
            gameData.currentYear = newYear;
            uiManager.UpdateYear(newYear);
        }
    }

    private void UpdatePositions()
    {
        //Square square = squareList[currentPlayer.PlayerData.CurrentPosition].GetComponent<Square>();
        //square.UpdateSquare(currentPlayer);
    }
}

