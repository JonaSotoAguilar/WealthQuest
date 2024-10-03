using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Singleton instance
    private bool initTurn = true;  // Controla si es posible iniciar el turno

    [Header("Game Components")]
    [SerializeField] private GameData gameData; // Datos del juego (asegúrate de asignarlo en el Inspector o por código)
    [SerializeField] private HUDController hud; // Controlador del HUD (asegúrate de asignarlo en el Inspector o por código)
    [SerializeField] private PlayerCamera playerCamera; // Cámara para el jugador
    [SerializeField] private Camera diceCamera; // Cámara para el dado

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
            if (player.Index >= gameData.NumPlayers)
                player.gameObject.SetActive(false);
        yield return null;// Esperar un frame para que Unity destruya los objetos
        gameData.Players = FindObjectsByType<PlayerData>(FindObjectsSortMode.None); // Buscar nuevamente los jugadores de la partida y guardarlos

        // Inicializar el primer turno
        var currentPlayer = gameData.Players.FirstOrDefault(p => p.Index == gameData.TurnPlayer);
        playerCamera.Player = currentPlayer.transform;
        hud.UpdatePlayer(currentPlayer);
        ChangePlayerView();
    }

    // Actualizar el turno
    public void UpdateTurn()
    {
        var players = gameData.Players;

        if (players.All(p => p.State != GameState.EnCurso))
            gameData.GameState = GameState.Finalizado; // Cambiar el estado del juego a Finalizado
        else
        {
            // Buscar el siguiente jugador en curso
            int turnPlayer = gameData.TurnPlayer;
            PlayerData currentPlayer = players.FirstOrDefault(p => p.Index == turnPlayer);
            // Buscar el siguiente jugador en curso
            do
            {
                turnPlayer = (turnPlayer + 1) % players.Length; // Cambiar al siguiente jugador en el array
                currentPlayer = players.FirstOrDefault(p => p.Index == turnPlayer); // Obtener jugador con indice igual al turno actual
            } while (currentPlayer.State != GameState.EnCurso); // Solo pasar si está en curso

            // Cambiar al siguiente jugador
            gameData.TurnPlayer = turnPlayer; // Actualizar el turno
            playerCamera.Player = currentPlayer.transform; // Cambiar la cámara al jugador actual
            hud.UpdatePlayer(currentPlayer); // Actualizar el HUD 
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

