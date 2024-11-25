using UnityEngine;
using System.Globalization;

[CreateAssetMenu(fileName = "ExpenseCard", menuName = "Cards/ExpenseCard")]
public class ExpenseCard : CardBase
{
    [Min(1)] public int duration;           // Duración en turnos del costo recurrente
    [Min(1)] public int cost;               // Costo de la tarjeta
    [Min(0)] public int KFPForDiscount;     // KPF necesario para obtener un descuento
    [Range(0, 1)] public float discounted;  // Porcentaje de descuento 
    private CultureInfo chileanCulture = new CultureInfo("es-CL");

    public override string GetFormattedText(int scoreKFP)
    {
        if (scoreKFP >= KFPForDiscount)
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

    public override void ApplyEffect(PlayerController player, int capital = 0)
    {
        bool hasDiscount = player.PlayerData.ScoreKFP >= KFPForDiscount;
        int finalCapital = hasDiscount ? Mathf.CeilToInt(cost * (1 - discounted)) : cost;
        PlayerExpense expense = new PlayerExpense(duration, finalCapital);
        player.CreateExpense(expense, expense.Turns > 1);
    }

    public override void RemoveFromGameData()
    {
        GameManager.Instance.GameData.expenseCards.Remove(this);
    }
}
