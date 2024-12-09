using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using Mirror;

[CreateAssetMenu(fileName = "InvestmentCard", menuName = "Cards/InvestmentCard")]
public class InvestmentCard : Card
{
    [Min(2), Tooltip("Duracion de pago.")] public int duration;
    [Range(-1, 100), Tooltip("Porcentaje de cambio.")] public List<float> pctChange;
    [Range(0, 1), Tooltip("Porcentaje de dividendos.")] public List<float> pctDividend;

    public override string GetFormattedText(int scoreKFP)
    {
        return $"Invertir durante {duration} años";
    }

    public override void ApplyEffect(int capital, bool isLocalGame = true)
    {
        if (capital <= 0) return;

        Investment investment = new Investment(title, duration, capital, new List<float>(pctChange), new List<float>(pctDividend));
        if (isLocalGame)
            GameLocalManager.CurrentPlayer.Data.AddInvestment(investment);
        else
        {
            GameNetManager.CurrentPlayer.Data.AddInvestment(investment);
        }
    }

    private void OnValidate()
    {
        AdjustListSize(ref pctChange, duration, 0);
        AdjustListSize(ref pctDividend, duration, 0);
    }

    private void AdjustListSize<T>(ref List<T> list, int newSize, T defaultValue)
    {
        if (list == null) list = new List<T>();
        while (list.Count < newSize) list.Add(defaultValue);
        while (list.Count > newSize) list.RemoveAt(list.Count - 1);
    }
}