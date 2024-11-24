using FishNet;
using FishNet.Object;
using TMPro;
using UnityEngine;

public class UIOnline : MonoBehaviour
{
    public static UIOnline Instance { get; private set; }

    [Header("Data Game")]
    [SerializeField] private TextMeshProUGUI yearText;

    [Header("HUD")]
    [SerializeField] private NetworkObject hudPanel;
    [SerializeField] private NetworkObject hudPrefab;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    #region HUD

    public void InitializePlayerHUD(IPlayer player)
    {
        NetworkObject hudInstance = Instantiate(hudPrefab, hudPanel.transform);
        hudInstance.SetParent(hudPanel);
        InstanceFinder.ServerManager.Spawn(hudInstance, player.Connection);
    }

    public void UpdateYear(int year)
    {
        yearText.text = "Año " + year.ToString();
    }

    public HUD GetHUD(int index)
    {
        Transform hud = hudPanel.transform.GetChild(index);
        return hud.GetComponent<HUD>();
    }

    #endregion



}