using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameData gameData;
    [SerializeField] private CharactersDatabase charactersDB;

    private int slotData = 2;

    void Start()
    {
        if (PlayerStorage.players.Count > 0)
        {
            for (int i = 0; i < PlayerStorage.players.Count; i++)
            {
                SpawnPlayer(PlayerStorage.players[i]);
            }
            uiManager.InitPlayersHUD();
            uiManager.UpdateYear(gameData.currentYear);
            StartCoroutine(SaveSystem.SaveGame(gameData, slotData));
            GameManager.Instance.InitTurn();
            Destroy(gameObject);
        }
    }

    private void SpawnPlayer(NewPlayer player)
    {
        // Instancia el playerPrefab
        GameObject playerInstance = Instantiate(playerPrefab);
        playerInstance.name = "Player_" + (player.index + 1);
        Instantiate(charactersDB.GetModel(player.character), playerInstance.transform);

        // Asigna el dispositivo al PlayerInput
        var playerInput = playerInstance.GetComponent<PlayerInput>();
        playerInput.SwitchCurrentControlScheme(player.device);
        playerInput.actions.FindActionMap("Player").Disable();
        playerInput.SwitchCurrentActionMap("UI");

        // Crea un nuevo PlayerData
        PlayerData playerData;
        if (gameData.playersData.Count <= player.index)
        {
            playerData = new PlayerData();
            playerData.NewPlayer(player.index, player.name, player.character);
            gameData.playersData.Add(playerData);
        }
        else
        {
            playerData = gameData.playersData[player.index];
        }

        // Inicializa el PlayerController
        var playerController = playerInstance.GetComponent<PlayerController>();
        playerController.InitializePlayer(playerData, playerInput);
    }
}