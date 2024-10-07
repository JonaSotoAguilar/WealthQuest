using UnityEngine;

[CreateAssetMenu(fileName = "ExpenseCard", menuName = "Cards/ExpenseCard")]
public class ExpenseCard : ScriptableObject
{
    public string description;  // Descripción del gasto
    public Sprite image;        // Imagen de la tarjeta
    public int immediateCost;   // Costo inmediato, si existe
    public int recurrentCost;   // Costo recurrente, si existe
    public int duration;        // Duración en turnos del costo recurrente

    // Crear un PlayerExpense basado en los valores de la tarjeta y el score del jugador
    public PlayerExpense CreateExpense(int playerKFP)
    {
        bool hasDiscount = playerKFP >= 5;

        int finalCapital = immediateCost > 0
            ? (hasDiscount ? Mathf.CeilToInt(immediateCost * 0.9f) : immediateCost)
            : (hasDiscount ? Mathf.CeilToInt(recurrentCost * 0.9f) : recurrentCost);

        int finalTurns = immediateCost > 0 ? 0 : duration;

        // Crear y retornar el gasto
        return new PlayerExpense(finalTurns, finalCapital);
    }


    // Método que construye automáticamente el texto basado en los costos y el score del jugador
    public string GetFormattedText(int playerKFP)
    {
        if (playerKFP >= 5)
        {

            // Aplicar un descuento del 10% si el jugador tiene 5 o más puntos de score
            int discountedImmediateCost = Mathf.CeilToInt(immediateCost * 0.9f);
            int discountedRecurrentCost = Mathf.CeilToInt(recurrentCost * 0.9f);

            // FIXME: Puede ser fijo y recurrente al mismo tiempo
            if (immediateCost > 0)
            {
                // Texto con el costo inmediato original tachado y el costo con descuento
                return $"Pierde <s>${immediateCost}</s> ${discountedImmediateCost} de dinero.";
            }
            else if (recurrentCost > 0 && duration > 0)
            {
                // Texto con el costo recurrente original tachado y el costo con descuento
                return $"Paga <s>${recurrentCost}</s> ${discountedRecurrentCost} durante {duration} turnos.";
            }
        }
        else
        {
            // Si el jugador tiene menos de 5 puntos de score, mostrar el costo normal
            if (immediateCost > 0)
            {
                return $"Pierde ${immediateCost} de dinero.";
            }
            else if (recurrentCost > 0 && duration > 0)
            {
                return $"Paga ${recurrentCost} durante {duration} turnos.";
            }
        }

        return "Sin costo."; // En caso de que no haya ni costo inmediato ni recurrente
    }
}
