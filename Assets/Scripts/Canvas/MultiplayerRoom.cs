using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.SceneManagement;

public class MultiplayerRoom : MonoBehaviour
{
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private GameData gameData;
    [SerializeField] private TMPro.TextMeshProUGUI[] playerNames;
    private PlayerInput[] playerInputs;

    [Header("Componentes escena tablero")]
    [SerializeField] private GameObject[] playerPieces;
    [SerializeField] private DiceController diceController;
    [SerializeField] private HUDController hudController;
    [SerializeField] private SquareLoader squareLoader;

    private void Start()
    {
        playerInputs = new PlayerInput[playerInputManager.maxPlayerCount];
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

    // Detectar cuando un jugador se une
    private void OnPlayerJoined(PlayerInput playerInput)
    {
        // Imprime el action map actual del PlayerInput
        playerInput.SwitchCurrentActionMap("UI");
        Debug.Log(playerInput.currentActionMap.name);


        int index = playerInput.playerIndex; // Obtener el índice del PlayerInput
        DontDestroyOnLoad(playerInput.gameObject); // No destruir el objeto al cargar una nueva escena
        playerInputs[index] = playerInput; // Asignar el PlayerInput al arreglo de PlayerInput
        InitializePlayer(playerInput); // Inicializar el jugador
    }

    // Inicializar el jugador y asignar su pieza correspondiente
    private void InitializePlayer(PlayerInput playerInput)
    {
        var playerData = playerInput.GetComponent<PlayerData>(); // Obtener el PlayerData
        var playerMovement = playerInput.GetComponent<PlayerMovement>(); // Obtener el PlayerMovement
        var canvasPlayer = playerInput.GetComponentInChildren<CanvasPlayer>(); // Obtener el CanvasPlayer
        var playerController = playerInput.GetComponent<PlayerInputHandler>(); // Obtener el PlayerController

        if (playerController != null)
        {
            playerController.InitializePlayer(playerData, playerInput, playerMovement, canvasPlayer); // Inicializar el PlayerController
            //playerController.InitializeComponents(diceController, hudController, squareLoader); // Inicializar los componentes del PlayerController
        }
    }

    public void UpdatePlayerNames()
    {
        for (int i = 0; i < playerInputs.Length; i++)
        {
            if (playerInputs[i] != null)
            {
                PlayerData player = playerInputs[i].GetComponent<PlayerData>();
                player.PlayerName = playerNames[i].text;
            }
        }
    }

    private void CreateGame()
    {
        UpdatePlayerNames();
    }
}
