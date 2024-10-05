using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class MultiplayerLocal : MonoBehaviour
{
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private GameObject[] playerPieces;

    // Referencias a los componentes que ya están en la escena
    [SerializeField] private DiceController diceController;
    [SerializeField] private HUDController hudController;
    [SerializeField] private SquareLoader squareLoader;

    // Detectar cuando un jugador se une
    private void OnPlayerJoined(PlayerInput playerInput)
    {
        if (playerInputManager.playerCount > GameData.Instance.NumPlayers)
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
        //playerInput.actions.FindActionMap("Player").Disable();
        //playerInput.SwitchCurrentActionMap("UI");
        //Debug.Log("Current Action Map: " + playerInput.currentActionMap.name);

        var index = playerInput.playerIndex; // Obtener el índice del PlayerInput
        var playerPiece = playerPieces[index]; // Obtener la pieza correspondiente de la lista

        // Obtener los componentes de PlayerData 
        var playerData = playerPiece.GetComponent<PlayerData>();
        var playerMovement = playerPiece.GetComponent<PlayerMovement>();

        // Inicializar el PlayerInputHandler
        var playerController = playerInput.GetComponent<PlayerInputHandler>(); // Obtener el PlayerController
        var canvasPlayer = playerInput.GetComponentInChildren<CanvasPlayer>(); // Obtener el CanvasPlayer

        playerController.InitializePlayer(playerData, playerInput, playerMovement, canvasPlayer); // Inicializar el PlayerController
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
