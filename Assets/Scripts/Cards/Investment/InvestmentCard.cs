using UnityEngine;  
using System.Collections.Generic;

[CreateAssetMenu(fileName = "InvestmentCard", menuName = "Cards/InvestmentCard")]
public class InvestmentCard : CardBase
{
    [Min(2)] public int duration;           // Duración en turnos del costo recurrente
    [Range(-1, 10)] public List<float> pctChange; // Porcentaje de cambio anual
    [Range(0, 1)] public List<float> pctDividend;    // Porcentaje de dividendos anual

    // Método que construye automáticamente el texto basado en los costos y el score del jugador
    public override string GetFormattedText(int scoreKFP)
    {
        return $"Invertir durante {duration} años";
    }

    // Crear un PlayerExpense basado en los valores de la tarjeta y el score del jugador
    public override void ApplyEffect(PlayerData player, int capital)
    {
        if (capital <= 0)
            return;
        int dividend = (int)(capital * pctDividend[0]);
        PlayerInvestment investment = new PlayerInvestment(duration, capital, dividend, pctChange, pctDividend);
        player.CreateInvestment(investment);
    }

    public override void RemoveFromGameData()
    {
        GameData.Instance.InvestmentCards.Remove(this);
    }

    private void OnValidate()
    {
        AdjustListSize(ref pctChange, duration, 0);  // Ajusta pctChange a la duración con un valor por defecto de 0
        AdjustListSize(ref pctDividend, duration, 0);  // Ajusta pctDividend a la duración con un valor por defecto de 0
    }

    private void AdjustListSize<T>(ref List<T> list, int newSize, T defaultValue)
    {
        if (list == null) list = new List<T>();
        while (list.Count < newSize) list.Add(defaultValue);
        while (list.Count > newSize) list.RemoveAt(list.Count - 1);
    }
}
