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

        int index = playerInput.playerIndex; 
        DontDestroyOnLoad(playerInput.gameObject); 
        playerInputs[index] = playerInput; 
        InitializePlayer(playerInput);

        OnPlayerJoinedEvent?.Invoke(index);
    }

    // Inicializar el jugador y asignar su pieza correspondiente
    private void InitializePlayer(PlayerInput playerInput)
    {
        playerInput.actions.FindActionMap("Player").Disable();
        playerInput.SwitchCurrentActionMap("UI");

        var index = playerInput.playerIndex; 

        var playerData = playerInput.GetComponent<PlayerData>();
        var playerController = playerInput.GetComponent<PlayerController>();

        playerData.InitializePlayer(index, "Jugador " + (index + 1), 0,
                                    0, 10000, 0, 0, 0, 0, 0,
                                    new List<PlayerInvestment>(), new List<PlayerExpense>());
        playerController.InitializePlayer(playerData, playerInput);

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