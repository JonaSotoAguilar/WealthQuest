using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Components")]
    [SerializeField] private GameData gameData;
    [SerializeField] private CameraManager cameras;
    [SerializeField] private Transform[] squareList;

    [Header("Player")]
    [SerializeField] private List<PlayerController> players;
    [SerializeField] private PlayerController currentPlayer;

    public Transform[] SquareList { get => squareList; set => squareList = value; }
    public GameData GameData { get => gameData; set => gameData = value; }
    public List<PlayerController> Players { get => players; set => players = value; }
    public PlayerController CurrentPlayer { get => currentPlayer; set => currentPlayer = value; }

    // Singleton
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        InitializeSquares();
        Players = new List<PlayerController>();
    }

    private void InitializeSquares()
    {
        Transform containerSquares = GameObject.Find("Squares").transform;
        squareList = new Transform[containerSquares.childCount];
        for (int i = 0; i < squareList.Length; i++)
            squareList[i] = containerSquares.GetChild(i);
    }

    public void InitTurn()
    {
        currentPlayer = players[gameData.TurnPlayer];
        UpdatePositions();
        cameras.CurrentCamera(currentPlayer.transform);
        StartCoroutine(currentPlayer.InitQuestion());
    }

    public IEnumerator UpdateTurn()
    {
        NextPlayer();
        if (gameData.InitialPlayerIndex == gameData.TurnPlayer) yield return FinishYear();
        yield return SaveSystem.SaveGame(gameData);
        yield return cameras.UpdateCurrentCamera(currentPlayer.transform);
        NextTurn();
    }

    public void NextTurn()
    {
        StartCoroutine(currentPlayer.InitQuestion());
    }

    public void NextPlayer()
    {
        gameData.TurnPlayer = (gameData.TurnPlayer + 1) % players.Count;
        currentPlayer = players[gameData.TurnPlayer];
        UpdatePositions();
    }

    public IEnumerator FinishYear()
    {
        int newYear = gameData.CurrentYear + 1;
        if (newYear > gameData.YearsToPlay)
        {
            gameData.GameState = GameState.Finalizado;
            yield return null;
        }
        else
        {
            foreach (var player in players)
            {
                player.ProcessIncome();
                player.ProcessInvestments();
                player.ProcessRecurrentExpenses();
            }
            gameData.CurrentYear = newYear;
        }
    }

    public void UpdatePositions()
    {
        Square square = squareList[currentPlayer.PlayerData.CurrentPosition].GetComponent<Square>();
        square.UpdateSquare(currentPlayer);
    }
}

