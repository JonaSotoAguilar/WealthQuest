using UnityEngine;

[CreateAssetMenu(fileName = "InvestmentCard", menuName = "Cards/InvestmentCard")]
public class InvestmentCard : CardBase
{
    public int immediateCost;   // Costo inmediato, si existe
    public int recurrentCost;   // Costo recurrente, si existe
    public int duration;        // Duración en turnos del costo recurrente

    // Método que construye automáticamente el texto basado en los costos y el score del jugador
    public override string GetFormattedText(int scoreKFP)
    {
        return "";
    }


    // Crear un PlayerExpense basado en los valores de la tarjeta y el score del jugador
    public override void ApplyEffect(PlayerData player)
    {

    }

    public override void RemoveFromGameData()
    {

    }
}
