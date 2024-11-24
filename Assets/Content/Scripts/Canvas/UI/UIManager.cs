using UnityEngine;
using TMPro;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;  // Importar la librería necesaria para TextMeshPro

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private GameObject hudPrefab;
    //[SerializeField] private GameObject hudPrefabOnline;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private TextMeshProUGUI yearText;

    [SerializeField] List<HUD> HUDs = new List<HUD>();

    public HUD HUD(int index) => HUDs[index];

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void InitPlayersHUD(IGameManager game)
    {
        for (int i = 0; i < game.Players.Count; i++)
        {
            IPlayer player = game.Players[i];
            GameObject hudInstance = Instantiate(hudPrefab, hudPanel.transform);
            hudInstance.name = "HUD_" + player.Index;
            PlayerHUD playerHUD = hudInstance.GetComponent<PlayerHUD>();
            //playerHUD.InitHUD(player);
            //player.HUD = playerHUD;
        }
    }

    public void UpdateYear(int year)
    {
        yearText.text = "Año " + year;
    }
}
