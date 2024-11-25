using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Globalization;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private RawImage icon;
    [SerializeField] private TextMeshProUGUI kpf;
    [SerializeField] private TextMeshProUGUI money;
    [SerializeField] private TextMeshProUGUI invest;
    [SerializeField] private TextMeshProUGUI debt;
    [SerializeField] private TextMeshProUGUI income;
    [SerializeField] private TextMeshProUGUI expense;

    public TextMeshProUGUI PlayerName { get => playerName; set => playerName = value; }
    public RawImage Icon { get => icon; set => icon = value; }
    public TextMeshProUGUI Kpf { get => kpf; set => kpf = value; }
    public TextMeshProUGUI Money { get => money; set => money = value; }
    public TextMeshProUGUI Invest { get => invest; set => invest = value; }
    public TextMeshProUGUI Debt { get => debt; set => debt = value; }
    public TextMeshProUGUI Income { get => income; set => income = value; }
    public TextMeshProUGUI Expense { get => expense; set => expense = value; }

    public void InitHUD(PlayerController player)
    {
        CultureInfo chileanCulture = new CultureInfo("es-CL");

        playerName.text = player.PlayerData.PlayerName;
        kpf.text = player.PlayerData.ScoreKFP.ToString();
        money.text = player.PlayerData.Money.ToString("C0", chileanCulture);
        invest.text = player.PlayerData.Invest.ToString("C0", chileanCulture);
        debt.text = player.PlayerData.Debt.ToString("C0", chileanCulture);
        income.text = player.PlayerData.IncomeTurn.ToString("C0", chileanCulture);
        expense.text = player.PlayerData.ExpenseTurn.ToString("C0", chileanCulture);
    }
}