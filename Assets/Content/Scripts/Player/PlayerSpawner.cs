using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using UnityEngine.InputSystem;


public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private PlayerStorage playerStorage;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private HUDManager hudManager;

    void Start()
    {
        if (playerStorage.players != null && playerStorage.players.Count > 0)
        {
            for (int i = 0; i < playerStorage.players.Count; i++)
            {
                SpawnPlayer(playerStorage.players[i]);
            }
            GameData.Instance.Players = GameData.Instance.Players.Where(p => p != null).ToArray();
            hudManager.InitPlayersHUD();
            GameManager.Instance.InitTurn();
            //Destroy(gameObject);
        }
    }

    private void SpawnPlayer(NewPlayer player)
    {
        // Instancia el playerPrefab
        GameObject playerInstance = Instantiate(playerPrefab);
        playerInstance.name = "Player_" + (player.index + 1);
        //playerInstance.SetActive(false);
        Instantiate(player.model, playerInstance.transform);

        // Asigna el dispositivo al PlayerInput
        var playerInput = playerInstance.GetComponent<PlayerInput>();
        playerInput.SwitchCurrentControlScheme(player.device);
        playerInput.actions.FindActionMap("Player").Disable();
        playerInput.SwitchCurrentActionMap("UI");

        // Inicializa playerData
        var playerData = playerInput.GetComponent<PlayerData>();
        playerData.NewPlayer(player.index, player.name, player.model.name);
        GameData.Instance.Players[player.index] = playerData;

        // Inicializa el PlayerController
        var playerController = playerInstance.GetComponent<PlayerController>();
        playerController.InitializePlayer(playerData, playerInput);

        // Initializa la posición
        playerController.InitPosition();
        //playerInstance.SetActive(true);
    }
}
