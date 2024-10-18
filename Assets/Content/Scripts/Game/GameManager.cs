using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections;
using UnityEditor;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Components")]
    [SerializeField] private SquareLoader squares;
    [SerializeField] private CameraManager cameras;

    [Header("Player")]
    private PlayerController currentPlayer;

    public SquareLoader Squares { get => squares; }
    public CameraManager Cameras { get => cameras; }

    // Singleton
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitComponents();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitComponents()
    {
        squares = FindFirstObjectByType<SquareLoader>();
        cameras = FindFirstObjectByType<CameraManager>();
    }

    private void Start()
    {
        InitPositions();
        InitTurn();
    }

    public void InitPositions()
    {
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            player.InitPosition();
        }
    }

    public void InitTurn()
    {
        currentPlayer = GameData.Instance.Players.FirstOrDefault(p => p.Index == GameData.Instance.TurnPlayer).GetComponent<PlayerController>();
        cameras.CurrentCamera(currentPlayer.transform);
        currentPlayer.EnableDice();
    }

    public IEnumerator UpdateTurn()
    {
        NextPlayer();
        if (GameData.Instance.InitialPlayerIndex == GameData.Instance.TurnPlayer)
            yield return FinishYear();
        yield return cameras.UpdateCurrentCamera(currentPlayer.transform);
        currentPlayer.EnableDice();
    }

    public void NextPlayer()
    {
        var players = GameData.Instance.Players;
        int turnPlayer = (GameData.Instance.TurnPlayer + 1) % players.Length;
        currentPlayer = players.FirstOrDefault(p => p.Index == turnPlayer).GetComponent<PlayerController>();
        GameData.Instance.TurnPlayer = turnPlayer;
    }

    public IEnumerator FinishYear()
    {
        int newYear = GameData.Instance.CurrentYear + 1;
        if (newYear > GameData.Instance.YearsToPlay)
        {
            GameData.Instance.GameState = GameState.Finalizado;
            yield break;
        }
        else
        {
            var players = GameData.Instance.Players;
            foreach (var player in players)
            {
                player.ProcessIncome();
                player.ProcessInvestments();
                player.ProcessRecurrentExpenses();
            }
            GameData.Instance.CurrentYear = newYear;
        }
    }
}

