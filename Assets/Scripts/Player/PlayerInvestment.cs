using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerInvestment
{
    [SerializeField] private int turns;
    [SerializeField] private int capital;
    [SerializeField] private int dividend;
    [SerializeField] private List<float> pctChanges; // Lista de cambios porcentuales por año
    [SerializeField] private List<float> pctDividend;

    public int Turns { get => turns; set => turns = value; }
    public int Capital { get => capital; set => capital = value; }
    public int Dividend { get => dividend; set => dividend = value; }
    public List<float> PctChanges { get => pctChanges; set => pctChanges = value; }
    public List<float> PctDividend { get => pctDividend; set => pctDividend = value; }

    public PlayerInvestment(int turnsInvest, int capitalInvest, int dividendInvest,
                            List<float> pctChangesInvest, List<float> pctDividendInvest)
    {
        turns = turnsInvest;
        capital = capitalInvest;
        dividend = dividendInvest;
        pctChanges = pctChangesInvest;
        pctDividend = pctDividendInvest;
    }

    // Calcula y actualiza la capitalización según los pctChanges anuales
    public void UpdateInvestment()
    {
        capital += (int)(capital * pctChanges[0]);
        dividend = (int)(capital * pctDividend[0]);
        pctChanges.RemoveAt(0);
        pctDividend.RemoveAt(0);
    }
}
