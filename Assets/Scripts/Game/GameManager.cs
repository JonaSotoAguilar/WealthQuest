using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections;
using UnityEditor;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Components")]
    [SerializeField] private HUDManager hud;        // Controlador del HUD 
    [SerializeField] private SquareLoader squares;  // Cargador de casillas
    [SerializeField] private CameraManager cameras; // Controlador de cámaras

    [Header("Player")]
    [SerializeField] private PlayerData currentPlayer;

    public HUDManager HUD { get => hud; }
    //public DiceController Dice { get => dice; }
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
        // Inicializar componentes
        hud = FindFirstObjectByType<HUDManager>();
        squares = FindFirstObjectByType<SquareLoader>();
        cameras = FindFirstObjectByType<CameraManager>();
    }

    // Inicialización
    private void Start()
    {
        InitPositions();
        InitFirstTurn();
    }

    public void InitPositions()
    {
        // Busca todos los playersMovement en la escena
        var players = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        // Recorre los jugadores y desactiva aquellos cuyo índice sea mayor o igual al número de jugadores
        foreach (var player in players)
        {
            player.InitPosition();
        }
    }

    public void InitFirstTurn()
    {
        currentPlayer = GameData.Instance.Players.FirstOrDefault(p => p.Index == GameData.Instance.TurnPlayer);
        cameras.CurrentCamera(currentPlayer.transform);
        hud.UpdatePlayer(currentPlayer);
        ActiveDice();
        UpdateActionMap("Player");
    }

    // Actualizar el turno
    public IEnumerator UpdateTurn()
    {
        HUD.ShowPanel(true);
        var players = GameData.Instance.Players;

        if (players.All(p => p.State != GameState.EnCurso))     // Si todos los jugadores han terminado
            GameData.Instance.GameState = GameState.Finalizado; // Cambiar el estado del juego a Finalizado
        else
        {
            NextPlayer(players);
            if (GameData.Instance.TurnPlayer == 0) // Finalizo ronda
                yield return FinishRound();
            hud.UpdatePlayer(currentPlayer);
            yield return cameras.UpdateCurrentCamera(currentPlayer.transform);
            ActiveDice();
            UpdateActionMap("Player");
        }
    }

    // Cambiar al siguiente jugador
    public void NextPlayer(PlayerData[] players)
    {
        int turnPlayer = GameData.Instance.TurnPlayer;
        var nextPlayer = currentPlayer;

        do
        {
            turnPlayer = (turnPlayer + 1) % players.Length;                     // Cambiar al siguiente jugador en el array
            nextPlayer = players.FirstOrDefault(p => p.Index == turnPlayer);    // Obtener jugador con indice igual al turno actual
        } while (currentPlayer.State != GameState.EnCurso);                     // Solo pasar si está en curso

        GameData.Instance.TurnPlayer = turnPlayer;
        currentPlayer = nextPlayer;
    }

    public IEnumerator FinishRound()
    {
        var players = GameData.Instance.Players.Where(p => p.State == GameState.EnCurso).ToArray();
        if (players == null)
            yield break;
        foreach (var player in players)
        {
            player.ProcessRecurrentExpenses();
        }
    }

    public void UpdateActionMap(string actionMap)
    {
        PlayerInput playerInput = currentPlayer.GetComponent<PlayerInput>();
        playerInput.SwitchCurrentActionMap(actionMap);
    }

    public void ActiveDice()
    {
        PlayerDice playerDice = currentPlayer.GetComponent<PlayerManager>().PlayerDice;
        playerDice.ShowDice(true);
    }
}

