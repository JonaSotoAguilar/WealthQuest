using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class MultiplayerLocal : MonoBehaviour
{
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private PlayerInput[] playerInputs;
    public event Action<int> OnPlayerJoinedEvent; // Evento que envía el índice del jugador

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
        playerInput.actions.FindActionMap("Player").Disable();
        playerInput.SwitchCurrentActionMap("UI");

        int index = playerInput.playerIndex; // Obtener el índice del PlayerInput
        DontDestroyOnLoad(playerInput.gameObject); // No destruir el objeto al cargar una nueva escena
        playerInputs[index] = playerInput; // Asignar el PlayerInput al arreglo de PlayerInput
        InitializePlayer(playerInput); // Inicializar el jugador

        OnPlayerJoinedEvent?.Invoke(index);
    }

    // Inicializar el jugador y asignar su pieza correspondiente
    private void InitializePlayer(PlayerInput playerInput)
    {
        playerInput.actions.FindActionMap("Player").Disable();
        playerInput.SwitchCurrentActionMap("UI");

        var index = playerInput.playerIndex; // Obtener el índice del PlayerInput

        var playerData = playerInput.GetComponent<PlayerData>();
        var playerMovement = playerInput.GetComponent<PlayerMovement>();
        var playerInputHandler = playerInput.GetComponent<PlayerManager>();
        var playerCanvas = playerInput.GetComponentInChildren<PlayerCanvas>();
        var playerDice = playerInput.GetComponentInChildren<PlayerDice>();

        playerData.InitializePlayer(index, "Jugador " + (index + 1), 0, GameState.EnCurso, 0, 10000, 0, 0, new List<PlayerInvestment>(), new List<PlayerExpense>());
        playerInputHandler.InitializePlayer(playerData, playerInput, playerMovement, playerCanvas, playerDice);

        playerInputs[index] = playerInput;
        GameData.Instance.Players[index] = playerData;
    }

    public void UpdatePlayerName(int index, string name)
    {
        if (playerInputs[index] != null)
        {
            PlayerData player = playerInputs[index].GetComponent<PlayerData>();
            player.PlayerName = name;
        }
    }
}