using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    private static GameUIManager instance;

    [Header("Data")]
    [SerializeField] private TextMeshProUGUI yearText;

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

    public static void InitializeHUD(string clientID, bool isLocal = true)
    {
        HUD newHUD = Instantiate(instance.HUDPrefab, instance.HUDParent);
        instance.HUDs.Add(clientID, newHUD);
        newHUD.name = "HUD [" + clientID + "]";

        if (isLocal) newHUD.InitializeUILocal(clientID);
        else newHUD.InitializeUINet(clientID);
    }

    #endregion

    #region Year

    public static void ChangeYear(int newYear)
    {
        instance.yearText.text = "Año " + newYear.ToString();
    }

    #endregion

    #region HUD

    public static HUD GetHUD(string clientID)
    {
        if (!instance.HUDs.ContainsKey(clientID))
        {
            return null;
        }
        return instance.HUDs[clientID];
    }

    #endregion

}
