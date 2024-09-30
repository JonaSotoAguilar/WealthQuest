using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class MultiplayerLocal : MonoBehaviour
{

    [SerializeField]
    private PlayerInputManager playerInputManager;

    // Detectar cuando un jugador se une
    private void OnPlayerJoined(PlayerInput playerInput)
    {
        // Verificar si ya se alcanzó el límite de jugadores
        if (playerInputManager.playerCount > GameManager.Instance.GameData.NumPlayers)
        {
            Destroy(playerInput.gameObject); // Destruir el PlayerInput adicional
        }
        else
        {
            InitializePlayer(playerInput); // Asignar los PlayerControllers a los PlayerInputs de manera adecuada
        }
    }


    // Inicializar el jugador
    private void InitializePlayer(PlayerInput playerInput)
    {
        var players = FindObjectsByType<PlayerData>(FindObjectsSortMode.None); // Encontrar todos los PlayerControllers
        var index = playerInput.playerIndex; // Obtener el índice del PlayerInput
        var player = players.FirstOrDefault(p => p.PlayerIndex == index); // Obtener el PlayerController con el índice correcto
        // Imprimir el index del jugador
        Debug.Log($"Player {index} joined the game.");

        // Buscar un PlayerInput por su índice

        if (player == null)
        {
            Debug.LogWarning($"No se encontró un PlayerController para el índice {index}.");
        }
        else
        {
            // Inicializar el PlayerInputHandler
            var playerInputHandler = playerInput.GetComponent<PlayerController>(); // Obtener el PlayerInputHandler
            var canvasPlayer = playerInput.GetComponentInChildren<CanvasPlayer>(); // Obtener el CanvasPlayer

            if (playerInputHandler != null) // Inicializar el PlayerInputHandler
                playerInputHandler.Initialize(player, playerInput, canvasPlayer);
        }
    }
    // Suscribirse al evento correcto
    private void OnEnable()
    {
        playerInputManager.onPlayerJoined += OnPlayerJoined; // Subscribirse al evento correcto: onPlayerJoined
    }

    // Desuscribirse del evento correcto
    private void OnDisable()
    {
        playerInputManager.onPlayerJoined -= OnPlayerJoined; // Desubscribirse del evento correcto
    }
}
