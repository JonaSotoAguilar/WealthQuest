using UnityEngine;
using System.Collections.Generic;
using System.Globalization;

[CreateAssetMenu(fileName = "InvestmentCard", menuName = "Cards/InvestmentCard")]
public class InvestmentCard : CardBase
{
    [Min(2), Tooltip("Duracion de pago.")] public int duration;
    [Range(-1, 100), Tooltip("Porcentaje de cambio.")] public List<float> pctChange;
    [Range(0, 1), Tooltip("Porcentaje de dividendos.")] public List<float> pctDividend;
    private CultureInfo chileanCulture = new CultureInfo("es-CL");


    public override string GetFormattedText(int scoreKFP)
    {
        return $"Invertir durante {duration} años";
    }

    public override void ApplyEffect(PlayerController player, int capital)
    {
        if (capital <= 0)
            return;

        PlayerInvestment investment = new PlayerInvestment(title, duration, capital, new List<float>(pctChange), new List<float>(pctDividend));
        player.CreateInvestment(investment);
    }


    public override void RemoveFromGameData()
    {
        GameManager.Instance.GameData.InvestmentCards.Remove(this);
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
