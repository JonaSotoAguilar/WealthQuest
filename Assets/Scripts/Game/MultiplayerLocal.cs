using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class MultiplayerLocal : MonoBehaviour
{

    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private GameData gameData;
    [SerializeField] private GameObject[] playerPieces;

    // Referencias a los componentes que ya están en la escena
    [SerializeField] private DiceController diceController;
    [SerializeField] private HUDController hudController;
    [SerializeField] private SquareLoader squareLoader;

    // Detectar cuando un jugador se une
    private void OnPlayerJoined(PlayerInput playerInput)
    {
        if (playerInputManager.playerCount > gameData.NumPlayers)
        {
            Destroy(playerInput.gameObject);
        }
        else
        {
            InitializePlayer(playerInput);
        }
    }

    // Inicializar el jugador y asignar su pieza correspondiente
    private void InitializePlayer(PlayerInput playerInput)
    {
        var index = playerInput.playerIndex; // Obtener el índice del PlayerInput
        var playerPiece = playerPieces[index]; // Obtener la pieza correspondiente de la lista


        // Obtener los componentes de PlayerData y PlayerMovement de la pieza
        var playerData = playerPiece.GetComponent<PlayerData>();
        var playerMovement = playerPiece.GetComponent<PlayerMovement>();

        // Inicializar el PlayerInputHandler
        var playerController = playerInput.GetComponent<PlayerController>(); // Obtener el PlayerController
        var canvasPlayer = playerInput.GetComponentInChildren<CanvasPlayer>(); // Obtener el CanvasPlayer

        if (playerController != null)
        {
            // Asignar el PlayerData y PlayerMovement al PlayerController
            playerController.InitializePlayer(playerData, playerInput, playerMovement, canvasPlayer); // Inicializar el PlayerController
            playerController.InitializeComponents(diceController, hudController, squareLoader); // Inicializar los componentes del PlayerController
        }
    }

    // Suscribirse al evento correcto
    private void OnEnable()
    {
        playerInputManager.onPlayerJoined += OnPlayerJoined;
    }

    // Desuscribirse del evento correcto
    private void OnDisable()
    {
        playerInputManager.onPlayerJoined -= OnPlayerJoined;
    }
}
