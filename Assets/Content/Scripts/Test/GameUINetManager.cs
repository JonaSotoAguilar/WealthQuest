using System.Collections.Generic;
using FishNet.Object;
using TMPro;
using UnityEngine;

public class GameUINetManager : NetworkBehaviour
{
    private static GameUINetManager instance;

    [SerializeField] private TextMeshProUGUI yearText;

    [SerializeField] private PlayerHUD playerHUDPrefab;
    [SerializeField] private Transform playerHUDParent;


    private Dictionary<string, PlayerHUD> playerHUDs = new Dictionary<string, PlayerHUD>();


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public static void UpdateYear(int year)
    {
        instance.RpcUpdateYear(year);
    }

    [ObserversRpc]
    public void RpcUpdateYear(int year)
    {
        yearText.text = "Año " + year.ToString();
    }

    public static void PlayerJoined(string clientID)
    {
        PlayerHUD newHUD = Instantiate(instance.playerHUDPrefab, instance.playerHUDParent);
        instance.playerHUDs.Add(clientID, newHUD);
        newHUD.name = "PlayerHUD_" + clientID;
        newHUD.Initialize(clientID);
    }
}
