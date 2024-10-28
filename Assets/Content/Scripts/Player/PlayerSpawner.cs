using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private HUDManager hudManager;
    [SerializeField] private GameData gameData;

    void Start()
    {
        if (PlayerStorage.players.Count > 0)
        {
            for (int i = 0; i < PlayerStorage.players.Count; i++)
            {
                SpawnPlayer(PlayerStorage.players[i]);
            }
            hudManager.InitPlayersHUD();
            GameManager.Instance.InitTurn();
            StartCoroutine(SaveSystem.SaveGame(gameData));
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
        PlayerData playerData;
        if (gameData.PlayersData.Count <= player.index)
        {
            playerData = new PlayerData();
            playerData.NewPlayer(player.index, player.name, player.model.characterID);
            gameData.PlayersData.Add(playerData);
        }
        else
        {
            playerData = gameData.PlayersData[player.index];
        }

        // Inicializa el PlayerController
        var playerController = playerInstance.GetComponent<PlayerController>();
        playerController.InitializePlayer(playerData, playerInput);
    }
}