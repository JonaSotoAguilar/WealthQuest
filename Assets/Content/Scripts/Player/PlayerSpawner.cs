using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private PlayerStorage playerStorage;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private HUDManager hudManager;
    [SerializeField] private GameData gameData;

    void Start()
    {
        if (playerStorage.players != null && playerStorage.players.Count > 0)
        {
            for (int i = 0; i < playerStorage.players.Count; i++)
            {
                SpawnPlayer(playerStorage.players[i]);
            }
            hudManager.InitPlayersHUD();
            GameManager.Instance.InitTurn();
            StartCoroutine(SaveSystem.SaveGame(gameData, 1));
            Destroy(gameObject);
        }
    }

    private void SpawnPlayer(NewPlayer player)
    {
        // Instancia el playerPrefab
        GameObject playerInstance = Instantiate(playerPrefab);
        playerInstance.name = "Player_" + (player.index + 1);
        Instantiate(player.model.characterPrefabs, playerInstance.transform);

        // Asigna el dispositivo al PlayerInput
        var playerInput = playerInstance.GetComponent<PlayerInput>();
        playerInput.SwitchCurrentControlScheme(player.device);
        playerInput.actions.FindActionMap("Player").Disable();
        playerInput.SwitchCurrentActionMap("UI");

        // Crea un nuevo PlayerData
        var playerData = new PlayerData();
        playerData.NewPlayer(player.index, player.name, player.model.characterID);
        GameManager.Instance.GameData.PlayersData.Add(playerData);

        // Inicializa el PlayerController
        var playerController = playerInstance.GetComponent<PlayerController>();
        playerController.InitializePlayer(playerData, playerInput);

        // Initializa la posición
        playerController.InitPosition();
    }
}
