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

    public void InitHUD(IPlayer player)
    {
        CultureInfo chileanCulture = new CultureInfo("es-CL");

        playerName.text = player.PlayerName;
        kpf.text = player.Points.ToString();
        money.text = player.Money.ToString("C0", chileanCulture);
        invest.text = player.Invest.ToString("C0", chileanCulture);
        debt.text = player.Debt.ToString("C0", chileanCulture);
        income.text = player.Income.ToString("C0", chileanCulture);
        expense.text = player.Expense.ToString("C0", chileanCulture);
    }
}