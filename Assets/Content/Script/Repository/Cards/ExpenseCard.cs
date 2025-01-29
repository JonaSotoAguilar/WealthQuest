using UnityEngine;
using System.Globalization;

[CreateAssetMenu(fileName = "ExpenseCard", menuName = "Cards/ExpenseCard")]
public class ExpenseCard : Card
{
    [Range(1, 25)] public int duration;      // Duración en turnos del costo recurrente
    [Min(1)] public int cost;               // Costo de la tarjeta

    private CultureInfo chileanCulture = new CultureInfo("es-CL");

    public override SquareType GetCardType()
    {
        return SquareType.Expense;
    }

    public override string GetFormattedText(int score)
    {
        if (duration <= 1)
            return $"Paga <color=red>{cost.ToString("C0", chileanCulture)}</color>.";
        else
            return $"Pagas <color=red>{cost.ToString("C0", chileanCulture)}</color> durante <color=red>{duration}</color> años.";
    }

    public override void ApplyEffect(int capital = 0, bool isLocalGame = true)
    {
        if (isLocalGame)
        {
            PlayerLocalData player = GameLocalManager.CurrentPlayer.Data;
            int finalCapital = cost;
            Expense expense = new Expense(duration, finalCapital);
            player.NewExpense(expense, expense.Turns > 1);
        }
        else
        {
            PlayerNetData player = GameNetManager.CurrentPlayer.Data;
            int finalCapital = cost;
            Expense expense = new Expense(duration, finalCapital);

            player.NewExpense(expense, expense.Turns > 1);
        }
    }
}
