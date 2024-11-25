using UnityEngine;
using TMPro;
using System.Collections.Generic;  // Importar la librería necesaria para TextMeshPro

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject hudPrefab;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private TextMeshProUGUI yearText;

    public void InitPlayersHUD()
    {
        for (int i = 0; i < GameManager.Instance.Players.Count; i++)
        {
            PlayerController player = GameManager.Instance.Players[i];
            GameObject hudInstance = Instantiate(hudPrefab, hudPanel.transform);
            hudInstance.name = "HUD_" + player.PlayerData.Index;
            PlayerHUD playerHUD = hudInstance.GetComponent<PlayerHUD>();
            playerHUD.InitHUD(player);
            player.PlayerHUD = playerHUD;
        }
    }

    public void UpdateYear(int year)
    {
        yearText.text = "Año " + year;
    }
}
