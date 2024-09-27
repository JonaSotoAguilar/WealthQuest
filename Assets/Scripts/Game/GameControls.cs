using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq; // Asegúrate de importar LINQ

public class GameControls : MonoBehaviour
{
    public static GameControls Instance { get; private set; } // Singleton instance
    // Controladores
    public DiceController diceController; // Controlador del dado
    // Cameras
    public Camera playerCamera; // Cámara para el jugador
    public Camera diceCamera; // Cámara para el dado
    // Input actions
    public PlayerInputManager playerInputManager; // Referencia al PlayerInputManager

    public List<Player> players; // Lista de jugadores
    // Variables
    private int currentPlayer = 0; // Jugador actual
    public int numPlayers = 2;
    public GameState gameState = GameState.EnCurso; // Estado del juego
    // Flag
    private bool canThrowDice;  // Controla si es posible lanzar el dado

    // Inicialización
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

        // InitializePlayers();
        playerInputManager.onPlayerJoined += OnPlayerJoined;
    }

    void Start()
    {
        StartCoroutine(WaitForPlayers()); // Esperar a que se conecten los jugadores
        changePlayerView(); // Cambiar a la vista del jugador

        // currentPlayer = 0; // Establecer el primer jugador
        // canThrowDice = true; // Permitir lanzar el dado
        // HUDController.instance.UpdateHUD(players[currentPlayer]); // Actualizar el HUD
    }

    public void InitializePlayers()
    {
        players = new List<Player>(numPlayers);

        for (int i = 0; i < numPlayers; i++)
        {
            GameObject playerObject = GameObject.Find("Player_" + (i + 1));
            if (playerObject != null)
            {
                players.Add(playerObject.GetComponent<Player>()); // Añadir el jugador a la lista
            }
            else
            {
                Debug.LogError("Player_" + (i + 1) + " no se encontró en la escena.");
            }
        }
    }

    // Evento disparado cuando un jugador se une
    private void OnPlayerJoined(PlayerInput pi)
    {
        Player player = pi.GetComponent<Player>();

        if (player != null && players.Count < numPlayers)
        {
            // Suscribirse al evento onControlsChanged de cada jugador para monitorear los cambios de controles
            // pi.onControlsChanged += OnControlsChanged;

            // Asignar teclado y ratón al primer jugador
            if (players.Count == 0 && (pi.devices.Contains(Keyboard.current) || pi.devices.Contains(Mouse.current)))
            {
                pi.SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
                player.InitializePlayer("Player 1", 0, 0, players.Count);
            }
            else
            {
                // Verificar si el dispositivo ya está siendo utilizado por otro jugador
                bool isDeviceUnique = true;

                if (pi.devices.Count > 0)
                {
                    foreach (var existingPlayer in players)
                    {
                        if (existingPlayer.GetComponent<PlayerInput>().devices[0] == pi.devices[0])
                        {
                            isDeviceUnique = false;
                            break;
                        }
                    }
                }

                if (!isDeviceUnique)
                {
                    Debug.LogWarning("El dispositivo ya está asignado a otro jugador.");
                    Destroy(pi.gameObject);  // Destruir el objeto PlayerInput para evitar duplicación
                    return;
                }

                // Inicializar al nuevo jugador
                player.InitializePlayer($"Player {players.Count + 1}", 0, 0, players.Count);
            }

            // Agregar el jugador a la lista
            players.Add(player);
        }
        else
        {
            Debug.LogWarning("No se pudo crear el jugador. O el jugador ya existe o se ha alcanzado el límite.");
            Destroy(pi.gameObject);  // Destruir si hay un problema en la creación del jugador
        }
    }
    // Esperar que se conecten los dispositivos
    private IEnumerator WaitForPlayers()
    {
        while (players.Count < numPlayers)
        {
            yield return null;
        }

        // Activar HUD
        HUDController.instance.gameObject.SetActive(true);
        HUDController.instance.UpdateHUD(players[currentPlayer]);  // Actualizar el HUD
        canThrowDice = true;
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Added:
                Debug.Log($"Nuevo dispositivo conectado: {device.displayName}");
                // El jugador debe presionar un botón en este dispositivo para unirse
                break;
            case InputDeviceChange.Disconnected:
                Debug.Log($"Dispositivo desconectado: {device.displayName}");
                break;
        }
    }


    // Lanzar el dado
    public IEnumerator ThrowDice()
    {
        canThrowDice = false;
        // Esperar hasta que el dado se detenga y obtener el resultado
        changeDiceView(); // Cambiar a la vista del jugador
        diceController.LaunchDice(); // Lanzar el dado
        while (!diceController.IsDiceStopped())
        {
            yield return null;
        }

        StartCoroutine(MovePlayer()); // Mover al jugador actual
    }

    // Mover al jugador actual
    private IEnumerator MovePlayer()
    {
        // Mover al jugador actual y esperar hasta que se detenga
        changePlayerView();
        Player player = players[currentPlayer];
        player.playerMovement.MovePlayer(diceController.diceRoll);
        while (!player.playerMovement.IsPlayerStopped())
        {
            yield return null;
        }

        StartCoroutine(ActiveSquare());
    }

    private IEnumerator ActiveSquare()
    {
        // Esperar hasta que la casilla se detenga
        Player player = players[currentPlayer];
        Square square = SquareManager.Instance.Squares[player.playerMovement.GetCurrentPosition()].GetComponent<Square>();

        // Aquí pasamos el jugador y su dispositivo
        square.ActiveSquare(player); // Pasamos el dispositivo del jugador actual

        while (!square.IsSquareStopped())
        {
            yield return null;
        }

        HUDController.instance.UpdateHUD(player); // Actualizar el HUD
        UpdateTurn(); // Actualizar el turno
    }


    // Actualizar el turno
    private void UpdateTurn()
    {
        // Comprobar si todos los jugadores han terminado
        if (CheckIfAllPlayersFinished())
        {
            gameState = GameState.Finalizado; // Cambiar el estado del juego
            Debug.Log("Todos los jugadores han terminado. El juego ha finalizado.");
        }
        else
        {
            // Buscar el siguiente jugador en curso
            do
            {
                // Cambiar al siguiente jugador Array
                currentPlayer = (currentPlayer + 1) % players.Count;
            } while (players[currentPlayer].playerState != GameState.EnCurso);
            HUDController.instance.UpdateHUD(players[currentPlayer]); // Actualizar el HUD
            canThrowDice = true; // Permite lanzar el dado
        }
    }

    // Comprobar si todos los jugadores han terminado
    private bool CheckIfAllPlayersFinished()
    {
        foreach (Player player in players)
        {
            if (player.playerState == GameState.EnCurso)
            {
                return false; // Si hay algún jugador en curso, el juego no ha terminado
            }
        }
        return true; // Todos los jugadores están en estado FINALIZADO
    }

    private void changeDiceView()
    {
        playerCamera.enabled = false;
        diceCamera.enabled = true;
    }

    private void changePlayerView()
    {
        playerCamera.enabled = true;
        diceCamera.enabled = false;
    }

    public bool canPlayTurn()
    {
        return canThrowDice;
    }

    public int getCurrentPlayer()
    {
        return currentPlayer;
    }
}
