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

    public void Initialize(string clientID)
    {
        PlayerNetManager playerManager = GameNetManager.GetPlayer(clientID);
        PlayerNetData data = playerManager.Data;

        CultureInfo chileanCulture = new CultureInfo("es-CL");

        playerName.text = data.Nickname;
        kpf.text = data.Points.ToString();
        money.text = data.Money.ToString("C0", chileanCulture);
        invest.text = data.Invest.ToString("C0", chileanCulture);
        debt.text = data.Debt.ToString("C0", chileanCulture);
        income.text = data.Income.ToString("C0", chileanCulture);
        expense.text = data.Expense.ToString("C0", chileanCulture);
    }
}