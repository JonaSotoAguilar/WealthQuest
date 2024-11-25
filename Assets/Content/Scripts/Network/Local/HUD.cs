using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Globalization;

public class HUD : MonoBehaviour
{
    private static CultureInfo chileanCulture = new CultureInfo("es-CL");
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private RawImage icon;
    [SerializeField] private TextMeshProUGUI points;
    [SerializeField] private TextMeshProUGUI money;
    [SerializeField] private TextMeshProUGUI invest;
    [SerializeField] private TextMeshProUGUI debt;
    [SerializeField] private TextMeshProUGUI income;
    [SerializeField] private TextMeshProUGUI expense;

    public void Initialize(string clientID)
    {
        //FIXME: Cambiar a Data
        PlayerNetManager player = GameNetManager.GetPlayer(clientID);
        PlayerNetData data = player.Data;

        playerName.text = data.Nickname;
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
}