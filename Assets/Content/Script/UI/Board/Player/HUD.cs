using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Globalization;

public class HUD : MonoBehaviour
{
    private static CultureInfo chileanCulture = new CultureInfo("es-CL");

    [Header("Turn")]
    [SerializeField] private GameObject activeTurn;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI nickname;
    [SerializeField] private TextMeshProUGUI points;
    [SerializeField] private TextMeshProUGUI money;
    [SerializeField] private TextMeshProUGUI invest;
    [SerializeField] private TextMeshProUGUI debt;
    [SerializeField] private TextMeshProUGUI income;
    [SerializeField] private TextMeshProUGUI expense;

    public void InitializeUILocal(string clientID)
    {
        var data = GameLocalManager.GetPlayer(clientID).Data;
        nickname.text = data.Nickname;
        points.text = data.Points.ToString();
        money.text = data.Money.ToString("C0", chileanCulture);
        invest.text = data.Invest.ToString("C0", chileanCulture);
        debt.text = data.Debt.ToString("C0", chileanCulture);
        income.text = data.Income.ToString("C0", chileanCulture);
        expense.text = data.Expense.ToString("C0", chileanCulture);
    }

    public void InitializeUINet(string clientID)
    {
        var data = GameNetManager.GetPlayer(clientID).Data;
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

    public void SetActiveTurn(bool active)
    {
        activeTurn.SetActive(active);
    }
}