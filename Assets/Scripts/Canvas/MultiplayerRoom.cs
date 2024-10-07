using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MultiplayerRoom : MonoBehaviour
{
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private TMPro.TextMeshProUGUI[] playerNames;
    [SerializeField] private GameObject[] playerPanels; // Lista de paneles para los jugadores
    [SerializeField] private GameObject connectedPanelPrefab; // Prefab del panel "conectado"
    [SerializeField] private PlayerInput[] playerInputs;

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

        // Reemplazar el panel del jugador con el prefab "conectado"
        ReplacePanelWithConnected(index);
    }

    // Inicializar el jugador y asignar su pieza correspondiente
    private void InitializePlayer(PlayerInput playerInput)
    {
        playerInput.actions.FindActionMap("Player").Disable();
        playerInput.SwitchCurrentActionMap("UI");

        var index = playerInput.playerIndex; // Obtener el índice del PlayerInput

        var playerData = playerInput.GetComponent<PlayerData>();
        var playerMovement = playerInput.GetComponent<PlayerMovement>();
        var playerInputHandler = playerInput.GetComponent<PlayerInputHandler>();
        var canvasPlayer = playerInput.GetComponentInChildren<CanvasPlayer>();

        playerData.InitializePlayer(index, "Jugador " + (index + 1), 0, GameState.EnCurso, 0, 10000, 0, 0, new List<PlayerInvestment>(), new List<PlayerExpense>());
        playerInputHandler.InitializePlayer(playerData, playerInput, playerMovement, canvasPlayer);

        playerInputs[index] = playerInput;
        GameData.Instance.Players[index] = playerData;
    }

    // Reemplazar el panel con el prefab de "conectado"
    private void ReplacePanelWithConnected(int index)
    {
        if (playerPanels[index] != null)
        {
            // Guardar la referencia al padre y la posición en la jerarquía
            Transform parent = playerPanels[index].transform.parent;
            int siblingIndex = playerPanels[index].transform.GetSiblingIndex();

            // Destruir el panel anterior
            Destroy(playerPanels[index]);

            // Instanciar el nuevo prefab "conectado" en la misma posición y bajo el mismo padre
            GameObject newPanel = Instantiate(connectedPanelPrefab, parent);

            // Colocar el nuevo panel en la misma posición en la jerarquía
            newPanel.transform.SetSiblingIndex(siblingIndex);

            // Actualizar la referencia en el array de paneles
            playerPanels[index] = newPanel;
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
        SceneManager.LoadScene("MultiplayerLocal");
    }
}
