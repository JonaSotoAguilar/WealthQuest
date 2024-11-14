using UnityEngine;
using TMPro;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;  // Importar la librería necesaria para TextMeshPro

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject hudPrefab;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private TextMeshProUGUI yearText;
    private static IGameManager gameManager;

    // FIXME: Sincronizar Sever y Client
    void Start()
    {
        if (gameManager != null) return;

        gameManager = GameManagerNetwork.Instance != null ?
                      GameManagerNetwork.Instance :
                      GameManager.Instance;
    }

    public void InitPlayersHUD()
    {
        for (int i = 0; i < gameManager.Players.Count; i++)
        {
            IPlayer player = gameManager.Players[i];
            GameObject hudInstance = Instantiate(hudPrefab, hudPanel.transform);
            hudInstance.name = "HUD_" + player.Index;
            PlayerHUD playerHUD = hudInstance.GetComponent<PlayerHUD>();
            playerHUD.InitHUD(player);
            player.HUD = playerHUD;
        }
    }

    public void UpdateYear(int year)
    {
        yearText.text = "Año " + year;
    }
}
