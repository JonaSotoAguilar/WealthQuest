using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Globalization;

public class HUD : MonoBehaviour
{
    private static CultureInfo chileanCulture = new CultureInfo("es-CL");
    [SerializeField] private TextMeshProUGUI nickname;
    [SerializeField] private RawImage icon;
    [SerializeField] private TextMeshProUGUI points;
    [SerializeField] private TextMeshProUGUI money;
    [SerializeField] private TextMeshProUGUI invest;
    [SerializeField] private TextMeshProUGUI debt;
    [SerializeField] private TextMeshProUGUI income;
    [SerializeField] private TextMeshProUGUI expense;

    public void Initialize(string clientID, bool isLocalGame = true)
    {
        if (isLocalGame) UpdateUI(GameLocalManager.GetPlayer(clientID).Data);
        else UpdateUI(GameNetManager.GetPlayer(clientID).Data);
    }

    private void UpdateUI(PlayerLocalData data)
    {
        nickname.text = data.Nickname;
        points.text = data.Points.ToString();
        money.text = data.Money.ToString("C0", chileanCulture);
        invest.text = data.Invest.ToString("C0", chileanCulture);
        debt.text = data.Debt.ToString("C0", chileanCulture);
        income.text = data.Income.ToString("C0", chileanCulture);
        expense.text = data.Expense.ToString("C0", chileanCulture);
    }

    private void UpdateUI(PlayerNetData data)
    {
        nickname.text = data.Nickname;
        points.text = data.Points.ToString();
        money.text = data.Money.ToString("C0", chileanCulture);
        invest.text = data.Invest.ToString("C0", chileanCulture);
        debt.text = data.Debt.ToString("C0", chileanCulture);
        income.text = data.Income.ToString("C0", chileanCulture);
        expense.text = data.Expense.ToString("C0", chileanCulture);
    }


    public void UpdatePoints(int points)
    {
        this.points.text = points.ToString();
    }

    public void UpdateMoney(int money)
    {
        this.money.text = money.ToString("C0", chileanCulture);
    }

    public void UpdateInvest(int invest)
    {
        this.invest.text = invest.ToString("C0", chileanCulture);
    }

    public void UpdateDebt(int debt)
    {
        this.debt.text = debt.ToString("C0", chileanCulture);
    }

    public void UpdateIncome(int income)
    {
        this.income.text = income.ToString("C0", chileanCulture);
    }

    public void UpdateExpense(int expense)
    {
        this.expense.text = expense.ToString("C0", chileanCulture);
    }
}