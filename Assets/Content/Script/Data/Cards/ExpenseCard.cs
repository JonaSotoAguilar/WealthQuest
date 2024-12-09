using UnityEngine;
using System.Globalization;
using Mirror;

[CreateAssetMenu(fileName = "ExpenseCard", menuName = "Cards/ExpenseCard")]
public class ExpenseCard : Card
{
    [Min(1)] public int duration;           // Duración en turnos del costo recurrente
    [Min(1)] public int cost;               // Costo de la tarjeta
    [Min(0)] public int PointsForDiscount;  // Puntos necesario para obtener un descuento
    [Range(0, 1)] public float discounted;  // Porcentaje de descuento 
    private CultureInfo chileanCulture = new CultureInfo("es-CL");

    public override string GetFormattedText(int scoreKFP)
    {
        if (scoreKFP >= PointsForDiscount)
        {

            // Aplicar un descuento del 10% si el jugador tiene 5 o más puntos de score
            int discountedCost = Mathf.CeilToInt(cost * (1 - discounted));

            if (duration <= 1)
                return $"Paga <s><color=red>{cost.ToString("C0", chileanCulture)}</color></s> <color=red>{discountedCost.ToString("C0", chileanCulture)}</color>.";
            else if (duration > 1)
                return $"Pagas <s><color=red>{cost.ToString("C0", chileanCulture)}</color></s> <color=red>{discountedCost.ToString("C0", chileanCulture)}</color> durante {duration} años.";
        }
        else
        {
            // Si el jugador tiene menos de 5 puntos de score, mostrar el costo normal
            if (duration <= 1)
                return $"Paga <color=red>{cost.ToString("C0", chileanCulture)}</color>.";
            else if (duration > 1)
                return $"Pagas <color=red>{cost.ToString("C0", chileanCulture)}</color> durante {duration} años.";
        }

        return "Sin costo."; // En caso de que no haya ni costo inmediato ni recurrente
    }

    public override void ApplyEffect(int capital = 0, bool isLocalGame = true)
    {
        if (isLocalGame)
        {
            PlayerLocalData player = GameLocalManager.CurrentPlayer.Data;
            bool hasDiscount = player.Points >= PointsForDiscount;
            int finalCapital = hasDiscount ? Mathf.CeilToInt(cost * (1 - discounted)) : cost;
            Expense expense = new Expense(duration, finalCapital);
            player.NewExpense(expense, expense.Turns > 1);
        }
        else
        {
            PlayerNetData player = GameNetManager.CurrentPlayer.Data;
            bool hasDiscount = player.Points >= PointsForDiscount;
            int finalCapital = hasDiscount ? Mathf.CeilToInt(cost * (1 - discounted)) : cost;
            Expense expense = new Expense(duration, finalCapital);

            player.NewExpense(expense, expense.Turns > 1);
        }

    }
}
