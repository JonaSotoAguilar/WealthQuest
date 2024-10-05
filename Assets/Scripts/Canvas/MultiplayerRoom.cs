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
        playerInput.actions.FindActionMap("Player").Disable();
        playerInput.SwitchCurrentActionMap("UI");

        var index = playerInput.playerIndex; // Obtener el índice del PlayerInput

        // Obtener los componentes de PlayerData 
        var playerData = playerInput.GetComponent<PlayerData>();
        var playerMovement = playerInput.GetComponent<PlayerMovement>();

        // Inicializar el PlayerInputHandler
        var playerInputHandler = playerInput.GetComponent<PlayerInputHandler>(); // Obtener el PlayerController
        var canvasPlayer = playerInput.GetComponentInChildren<CanvasPlayer>(); // Obtener el CanvasPlayer

        playerData.InitializePlayer(index, "Jugador " + (index + 1), 0, 0, GameState.EnCurso);
        playerInputHandler.InitializePlayer(playerData, playerInput, playerMovement, canvasPlayer); // Inicializar el PlayerController

        // Asignar el jugador a la lista de jugadores
        //playerMovement.InitPosition();
        playerInputs[index] = playerInput;
        GameData.Instance.Players[index] = playerData;
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

    public void UpdateActionMap()
    {
        for (int i = 0; i < playerInputs.Length; i++)
        {
            if (playerInputs[i] != null)
            {
                playerInputs[i].SwitchCurrentActionMap("Player");
            }
        }
    }

    public void StartGame()
    {
        UpdateActionMap();
        GameData.Instance.NewGame();
        SceneManager.LoadScene("MultiplayerLocalTest");
    }
}
