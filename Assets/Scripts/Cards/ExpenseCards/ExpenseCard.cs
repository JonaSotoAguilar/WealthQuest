using UnityEngine;

[CreateAssetMenu(fileName = "ExpenseCard", menuName = "Cards/ExpenseCard")]
public class ExpenseCard : CardBase
{
    // Costoo debe ser mayor a 0
    [Min(0)] public int cost;               // Costo inmediato, si existe
    [Min(1)] public int duration;           // Duración en turnos del costo recurrente
    [Min(0)] public int KFPForDiscount;     // Descuento en el costo si el jugador tiene un KFP mayor o igual a este valor
    [Range(0, 1)] public float discounted;  // Porcentaje de descuento 

    // Método que construye automáticamente el texto basado en los costos y el score del jugador
    public override string GetFormattedText(int scoreKFP)
    {
        if (scoreKFP >= KFPForDiscount)
        {

            // Aplicar un descuento del 10% si el jugador tiene 5 o más puntos de score
            int discountedCost = Mathf.CeilToInt(cost * (1 - discounted));

            if (cost > 0 && duration == 1)
            {
                // Texto con el costo inmediato original tachado y el costo con descuento
                return $"Pierde <s>${cost}</s> ${discountedCost} de dinero.";
            }
            else if (cost > 0 && duration > 0)
            {
                // Texto con el costo recurrente original tachado y el costo con descuento
                return $"Paga <s>${discountedCost}</s> ${discountedCost} durante {duration} turnos.";
            }
        }
        else
        {
            // Si el jugador tiene menos de 5 puntos de score, mostrar el costo normal
            if (cost > 0 && duration == 1)
            {
                return $"Pierde ${cost} de dinero.";
            }
            else if (cost > 0 && duration > 0)
            {
                return $"Paga ${cost} durante {duration} turnos.";
            }
        }

        return "Sin costo."; // En caso de que no haya ni costo inmediato ni recurrente
    }


    // Crear un PlayerExpense basado en los valores de la tarjeta y el score del jugador
    public override void ApplyEffect(PlayerData player)
    {
        bool hasDiscount = player.ScoreKFP >= KFPForDiscount;
        int finalCapital = hasDiscount ? Mathf.CeilToInt(cost * (1 -  discounted)) : cost;
        PlayerExpense expense = new PlayerExpense(duration, finalCapital);
        player.ApplyExpense(expense, expense.Turns > 1);
    }

    public override void RemoveFromGameData()
    {
        GameData.Instance.ExpenseCards.Remove(this);
    }
}
