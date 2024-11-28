using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameData gameData;
    [SerializeField] private CharactersDatabase charactersDB;

    void Start()
    {
        if (PlayerStorage.players.Count > 0)
        {
            for (int i = 0; i < PlayerStorage.players.Count; i++)
            {
                SpawnPlayer(PlayerStorage.players[i]);
            }
            GameLocalManager.InitializeGame();
            Destroy(gameObject);
        }
    }

    private void SpawnPlayer(UserPlayer player)
    {
        // Instancia el playerPrefab
        GameObject playerInstance = Instantiate(playerPrefab);
        playerInstance.name = "Player_" + (player.index + 1);

        GameObject character = Instantiate(charactersDB.GetModel(player.model), playerInstance.transform);
        character.name = "Character";

        // Asigna el dispositivo al PlayerInput
        var input = playerInstance.GetComponent<PlayerInput>();
        input.SwitchCurrentControlScheme(player.device);
        input.actions.FindActionMap("Player").Disable();
        input.SwitchCurrentActionMap("UI");

        // Inicializa el PlayerManager
        var playerManager = playerInstance.GetComponent<PlayerLocalManager>();

        if (playerManager == null)
        {
            Debug.LogError("PlayerLocalManager no encontrado");
            return;
        }

        // Crea PlayerData
        PlayerData data = new PlayerData();
        if (gameData.playersData.Count <= player.index)
        {
            data = new PlayerData($"{player.index}", player.name, player.model);
            gameData.playersData.Add(data);
        }
        else
        {
            data = gameData.playersData[player.index];
        }

        playerManager.Initialize(data, input);
    }
}