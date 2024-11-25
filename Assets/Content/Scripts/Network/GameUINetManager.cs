using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GameUINetManager : NetworkBehaviour
{
    private static GameUINetManager instance;

    [Header("Data")]
    [SerializeField] private TextMeshProUGUI yearText;
    [SyncVar(hook = nameof(OnChangeYear))] private int year = 0;

    [Header("HUDs")]
    [SerializeField] private Transform HUDParent;
    [SerializeField] private HUD HUDPrefab;
    private Dictionary<string, HUD> HUDs = new Dictionary<string, HUD>();

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    #region Initialization

    public static void PlayerJoined(string clientID)
    {
        HUD newHUD = Instantiate(instance.HUDPrefab, instance.HUDParent);
        instance.HUDs.Add(clientID, newHUD);
        newHUD.name = "HUD [" + clientID + "]";
        newHUD.Initialize(clientID);
    }

    #endregion

    #region Year

    [Server] //FIXME: Hacerlo synvar
    public static void UpdateYear(int year)
    {
        instance.year = year;
    }

    public void OnChangeYear(int oldYear, int newYear)
    {
        yearText.text = "Año " + newYear.ToString();
    }

    #endregion

    #region HUD

    public static void UpdatePoints(string clientID, int points)
    {
        instance.HUDs[clientID].UpdatePoints(points);
    }

    #endregion

}
