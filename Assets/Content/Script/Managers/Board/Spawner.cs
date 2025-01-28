using UnityEngine;
using UnityEngine.InputSystem;

public class Spawner : MonoBehaviour
{
    enum Mode { Single, LocalPass, LocalMulti, Online }

    [Header("Database")]
    [SerializeField] private GameData gameData;
    [SerializeField] private CharactersDatabase charactersDB;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerSinglePrefab;
    [SerializeField] private GameObject playerMultiPrefab;

    [Header("Mode")]
    [SerializeField] private GameObject eventSystem;
    [SerializeField] private PlayerInput inputDevice;
    private Mode mode;

    private void Start()
    {
        SetMode();
        switch (mode)
        {
            case Mode.Single:
                SpawnSingle();
                eventSystem.SetActive(true);
                inputDevice.enabled = true;
                break;
            case Mode.LocalPass:
                SpawnLocalPass();
                eventSystem.SetActive(true);
                inputDevice.enabled = true;
                break;
            case Mode.LocalMulti:
                SpawnLocalMulti();
                eventSystem.SetActive(false);
                inputDevice.enabled = false;
                break;
        }

        GameLocalManager.InitializeGame();
        Destroy(gameObject);
    }

    private void SetMode()
    {
        mode = (Mode)gameData.mode;
    }

    private void SpawnSingle()
    {
        SpawnPlayer(0);
    }

    private void SpawnLocalPass()
    {
        for (int i = 0; i < gameData.playersData.Count; i++)
        {
            SpawnPlayer(i);
        }
    }


    private void SpawnLocalMulti()
    {
        for (int i = 0; i < gameData.playersData.Count; i++)
        {
            SpawnPlayer(i, InputStorage.devices[i]);
        }
    }

    private void SpawnPlayer(int index, InputDevice device = null)
    {
        // 1. Instancia el playerPrefab
        var playerInstance = SpawnPrefab();
        playerInstance.name = "Player_" + (index + 1);
        // 2. Instancia el personaje
        int idChar = gameData.playersData[index].CharacterID;
        var character = Instantiate(charactersDB.GetModel(idChar), playerInstance.transform);
        character.name = "Character";
        // 3. Asigna dispositivo al PlayerInput
        PlayerInput input = null;
        if (device != null)
        {
            input = playerInstance.GetComponent<PlayerInput>();
            input.SwitchCurrentControlScheme(device);
            input.actions.FindActionMap("Player").Disable();
            input.SwitchCurrentActionMap("UI");
        }
        // 4. Guardo jugador en Manager
        var playerManager = playerInstance.GetComponent<PlayerLocalManager>();
        playerManager.Initialize(gameData.playersData[index], input);
        GameLocalManager.PlayerJoined(playerManager);
        // 5. Inicializo HUD
        GameUIManager.InitializeHUD(playerManager.Data.UID, true);
    }

    private GameObject SpawnPrefab()
    {
        GameObject prefab = null;
        switch (mode)
        {
            case Mode.Single:
            case Mode.LocalPass:
                prefab = Instantiate(playerSinglePrefab);
                break;
            case Mode.LocalMulti:
                prefab = Instantiate(playerMultiPrefab);
                break;
        }
        return prefab;
    }
}