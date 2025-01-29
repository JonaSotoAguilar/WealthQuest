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

    [Header("Character")]
    [SerializeField] private RawImage characterSprite;
    [SerializeField] private CharactersDatabase characterDB;

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

        characterSprite.texture = characterDB.GetCharacter(data.CharacterID).characterIcon.texture;
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

        characterSprite.texture = characterDB.GetCharacter(data.CharacterID).characterIcon.texture;
    }


    public void UpdatePoints(int newPoints)
    {
        int oldPoints = int.Parse(points.text);
        LeanTween.value(oldPoints, newPoints, 1.5f).setOnUpdate((float val) =>
        {
            points.text = Mathf.RoundToInt(val).ToString();
        }).setEaseOutQuad();

        // Efecto de escala al actualizar
        LeanTween.scale(points.gameObject, Vector3.one * 1.2f, 0.3f).setEaseOutBack().setLoopPingPong(5);
    }

    public void UpdateMoney(int newMoney)
    {
        int oldMoney = int.Parse(money.text, NumberStyles.Currency, chileanCulture);
        LeanTween.value(oldMoney, newMoney, 3f).setOnUpdate((float val) =>
        {
            money.text = Mathf.RoundToInt(val).ToString("C0", chileanCulture);
        }).setEaseOutQuad();
    }

    public void UpdateInvest(int newInvest)
    {
        int oldInvest = int.Parse(invest.text, NumberStyles.Currency, chileanCulture);
        LeanTween.value(oldInvest, newInvest, 3f).setOnUpdate((float val) =>
        {
            invest.text = Mathf.RoundToInt(val).ToString("C0", chileanCulture);
        }).setEaseOutQuad();
    }

    public void UpdateDebt(int newDebt)
    {
        int oldDebt = int.Parse(debt.text, NumberStyles.Currency, chileanCulture);
        LeanTween.value(oldDebt, newDebt, 3f).setOnUpdate((float val) =>
        {
            debt.text = Mathf.RoundToInt(val).ToString("C0", chileanCulture);
        }).setEaseOutQuad();
    }

    public void UpdateIncome(int newIncome)
    {
        int oldIncome = int.Parse(income.text, NumberStyles.Currency, chileanCulture);
        LeanTween.value(oldIncome, newIncome, 3f).setOnUpdate((float val) =>
        {
            income.text = Mathf.RoundToInt(val).ToString("C0", chileanCulture);
        }).setEaseOutQuad();
    }

    public void UpdateExpense(int newExpense)
    {
        int oldExpense = int.Parse(expense.text, NumberStyles.Currency, chileanCulture);
        LeanTween.value(oldExpense, newExpense, 3f).setOnUpdate((float val) =>
        {
            expense.text = Mathf.RoundToInt(val).ToString("C0", chileanCulture);
        }).setEaseOutQuad();
    }

    public void SetActiveTurn(bool active)
    {
        activeTurn.SetActive(active);
        if (active)
        {
            // Efecto de escala y brillo al activar el turno
            LeanTween.scale(activeTurn, Vector3.one * 1.2f, 0.3f).setEaseOutBack().setLoopPingPong(5);
        }
    }

}