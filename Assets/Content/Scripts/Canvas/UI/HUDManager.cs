using UnityEngine;
using TMPro;
using System.Collections.Generic;  // Importar la librería necesaria para TextMeshPro

public class HUDManager : MonoBehaviour
{
    [SerializeField] private GameObject hudPrefab;

    private void Awake()
    {
        InitPlayersHUD();
    }

    private void InitPlayersHUD()
    {
        for (int i = 0; i < GameData.Instance.Players.Length; i++)
        {
            PlayerData player = GameData.Instance.Players[i];
            GameObject hudInstance = Instantiate(hudPrefab, transform);
            PlayerHUD playerHUD = hudInstance.GetComponent<PlayerHUD>();
            playerHUD.InitHUD(player);
            player.PlayerHUD = playerHUD;
        }
    }
}
