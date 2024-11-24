using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    private readonly CultureInfo CI = new CultureInfo("es-CL");

    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private RawImage icon;
    [SerializeField] private TextMeshProUGUI points;
    [SerializeField] private TextMeshProUGUI money;
    [SerializeField] private TextMeshProUGUI invest;
    [SerializeField] private TextMeshProUGUI debt;
    [SerializeField] private TextMeshProUGUI income;
    [SerializeField] private TextMeshProUGUI expense;

    public void InitHUD(IPlayer player)
    {
        playerName.text = player.Nickname;
        points.text = player.Points.ToString();
        money.text = player.Money.ToString("C0", CI);
        invest.text = player.Invest.ToString("C0", CI);
        debt.text = player.Debt.ToString("C0", CI);
        income.text = player.Income.ToString("C0", CI);
        expense.text = player.Expense.ToString("C0", CI);
    }

    #region Setters

    public void SetIcon(Sprite sprite)
    {
        icon.texture = sprite.texture;
    }

    public void SetName(string name)
    {
        playerName.text = name;
    }

    public void SetPoints(int pointsValue)
    {
        points.text = pointsValue.ToString();
    }

    public void SetMoney(int moneyValue)
    {
        money.text = moneyValue.ToString("C0", CI);
    }

    public void SetInvest(int investValue)
    {
        invest.text = investValue.ToString("C0", CI);
    }

    public void SetDebt(int debtValue)
    {
        debt.text = debtValue.ToString("C0", CI);
    }

    public void SetIncome(int incomeValue)
    {
        income.text = incomeValue.ToString("C0", CI);
    }

    public void SetExpense(int expenseValue)
    {
        expense.text = expenseValue.ToString("C0", CI);
    }

    #endregion

}
