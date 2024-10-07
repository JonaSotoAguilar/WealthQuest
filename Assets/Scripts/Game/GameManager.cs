using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Singleton instance
    private bool initTurn = true;  // Controla si es posible iniciar el turno

    [Header("Game Components")]
    [SerializeField] private HUDController hud; // Controlador del HUD (asegúrate de asignarlo en el Inspector o por código)
    [SerializeField] private DiceController dice; // Controlador del dado
    [SerializeField] private SquareLoader squares; // Cargador de casillas

    [Header("Game Cameras")]
    [SerializeField] private PlayerCamera playerCamera; // Cámara para el jugador
    [SerializeField] private Camera diceCamera; // Cámara para el dado

    public bool InitTurn { get => initTurn; set => initTurn = value; }
    public HUDController HUD { get => hud; }
    public DiceController Dice { get => dice; }
    public SquareLoader Squares { get => squares; }

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
        hud = FindFirstObjectByType<HUDController>();
        playerCamera = FindFirstObjectByType<PlayerCamera>();
        dice = FindFirstObjectByType<DiceController>();
        squares = FindFirstObjectByType<SquareLoader>();
        diceCamera = GameObject.Find("DiceCamera").GetComponent<Camera>();
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
        // Inicializar el primer turno
        var currentPlayer = GameData.Instance.Players.FirstOrDefault(p => p.Index == GameData.Instance.TurnPlayer);
        playerCamera.Player = currentPlayer.transform;
        hud.UpdatePlayer(currentPlayer);
        ChangePlayerView();
    }

    // Actualizar el turno
    public void UpdateTurn()
    {
        var players = GameData.Instance.Players;

        if (players.All(p => p.State != GameState.EnCurso))
            GameData.Instance.GameState = GameState.Finalizado; // Cambiar el estado del juego a Finalizado
        else
        {
            // Buscar el siguiente jugador en curso
            int turnPlayer = GameData.Instance.TurnPlayer;
            PlayerData currentPlayer = players.FirstOrDefault(p => p.Index == turnPlayer);
            // Buscar el siguiente jugador en curso
            do
            {
                turnPlayer = (turnPlayer + 1) % players.Length; // Cambiar al siguiente jugador en el array
                currentPlayer = players.FirstOrDefault(p => p.Index == turnPlayer); // Obtener jugador con indice igual al turno actual
            } while (currentPlayer.State != GameState.EnCurso); // Solo pasar si está en curso

            // Cambiar al siguiente jugador
            GameData.Instance.TurnPlayer = turnPlayer; // Actualizar el turno
            playerCamera.Player = currentPlayer.transform; // Cambiar la cámara al jugador actual
            hud.UpdatePlayer(currentPlayer); // Actualizar el HUD 
            initTurn = true; // Permite iniciar el siguiente turno
        }
    }

    // Comprobar si es posible jugar el turno
    public bool CanPlayTurn(int playerIndex) => initTurn && playerIndex == GameData.Instance.TurnPlayer;

    // Cambiar a la vista del dado
    public void ChangeDiceView()
    {
        playerCamera.enabled = false;
        diceCamera.enabled = true;
    }

    // Cambiar a la vista del jugador
    public void ChangePlayerView()
    {
        playerCamera.enabled = true;
        diceCamera.enabled = false;
    }
}

