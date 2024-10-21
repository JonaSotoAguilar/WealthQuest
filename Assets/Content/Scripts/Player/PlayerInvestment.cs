using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerInvestment
{
    [SerializeField] private string nameInvestment;
    [SerializeField] private int turns;
    [SerializeField] private int capital;
    [SerializeField] private int dividend;
    [SerializeField] private List<float> pctChanges;
    [SerializeField] private List<float> pctDividend;

    public string NameInvestment { get => nameInvestment; set => nameInvestment = value; }
    public int Turns { get => turns; set => turns = value; }
    public int Capital { get => capital; set => capital = value; }
    public int Dividend { get => dividend; set => dividend = value; }
    public List<float> PctChanges { get => pctChanges; set => pctChanges = value; }
    public List<float> PctDividend { get => pctDividend; set => pctDividend = value; }

    public PlayerInvestment(string name, int turnsInvest, int capitalInvest, int dividendInvest,
                            List<float> pctChangesInvest, List<float> pctDividendInvest)
    {
        nameInvestment = name;
        turns = turnsInvest;
        capital = capitalInvest;
        dividend = dividendInvest;
        pctChanges = pctChangesInvest;
        pctDividend = pctDividendInvest;
    }

    public void UpdateInvestment()
    {
        capital += (int)(capital * pctChanges[0]);
        dividend = (int)(capital * pctDividend[0]);
        pctChanges.RemoveAt(0);
        pctDividend.RemoveAt(0);
    }
}
