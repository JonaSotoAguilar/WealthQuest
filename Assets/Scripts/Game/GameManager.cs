using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Singleton instance

    [SerializeField]
    private GameData gameData; // Datos del juego
    public GameData GameData { get => gameData; }

    [SerializeField]
    private DiceController diceController; // Controlador del dado (asegúrate de asignarlo en el Inspector o por código)
    public DiceController Dice { get => diceController; }

    [SerializeField]
    private SquareController squareController; // Controlador de las casillas (asegúrate de asignarlo en el Inspector o por código)
    public SquareController Squares { get => squareController; }

    [SerializeField]
    private HUDController hudController; // Controlador del HUD (asegúrate de asignarlo en el Inspector o por código)
    public HUDController HUD { get => hudController; }

    [SerializeField]
    private PlayerCamera playerCamera; // Cámara para el jugador

    [SerializeField]
    private Camera diceCamera; // Cámara para el dado

    private bool initTurn = true;  // Controla si es posible iniciar el turno
    public bool InitTurn { get => initTurn; set => initTurn = value; }

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
    }

    // Inicialización
    private void Start()
    {
        StartCoroutine(InitializeGame());
    }

    // Inicializa los datos del juego
    private IEnumerator InitializeGame()
    {
        var players = FindObjectsByType<PlayerData>(FindObjectsSortMode.None); // Buscar jugadores
        // Si el índice de jugador es mayor o igual al número de jugadores, eliminarlo
        foreach (var player in players)
            if (player.PlayerIndex >= gameData.NumPlayers)
                Destroy(player.gameObject);
        yield return null;// Esperar un frame para que Unity destruya los objetos
        GameData.Players = FindObjectsByType<PlayerData>(FindObjectsSortMode.None); // Buscar nuevamente los jugadores de la partida y guardarlos

        // Inicializar el primer turno
        var currentPlayer = GameData.Players.FirstOrDefault(p => p.PlayerIndex == gameData.TurnPlayer);
        playerCamera.Player = currentPlayer.transform;
        hudController.UpdateHUD(currentPlayer);
        ChangePlayerView();
    }

    // Actualizar el turno
    public void UpdateTurn()
    {
        var players = GameData.Players;

        if (players.All(p => p.PlayerState != GameState.EnCurso))
            gameData.GameState = GameState.Finalizado; // Cambiar el estado del juego a Finalizado
        else
        {
            // Buscar el siguiente jugador en curso
            int turnPlayer = gameData.TurnPlayer;
            PlayerData currentPlayer = players.FirstOrDefault(p => p.PlayerIndex == turnPlayer);
            // Buscar el siguiente jugador en curso
            do
            {
                turnPlayer = (turnPlayer + 1) % players.Length; // Cambiar al siguiente jugador en el array
                currentPlayer = players.FirstOrDefault(p => p.PlayerIndex == turnPlayer); // Obtener jugador con indice igual al turno actual
            } while (currentPlayer.PlayerState != GameState.EnCurso); // Solo pasar si está en curso

            // Cambiar al siguiente jugador
            gameData.TurnPlayer = turnPlayer; // Actualizar el turno
            playerCamera.Player = currentPlayer.transform; // Cambiar la cámara al jugador actual
            hudController.UpdateHUD(currentPlayer); // Actualizar el HUD 
            initTurn = true; // Permite iniciar el siguiente turno
        }
    }

    // Comprobar si es posible jugar el turno
    public bool CanPlayTurn(int playerIndex) => initTurn && playerIndex == gameData.TurnPlayer;

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

