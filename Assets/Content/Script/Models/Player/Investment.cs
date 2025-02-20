using System.Collections.Generic;
using Mirror;
using UnityEngine;

[System.Serializable]
public class Investment
{
    [SerializeField] private string nameInvestment;
    [SerializeField] private int turns;
    [SerializeField] private int capital;
    [SerializeField] private List<float> pctChanges;
    [SerializeField] private List<float> pctDividend;

    public string NameInvestment { get => nameInvestment; set => nameInvestment = value; }
    public int Turns { get => turns; set => turns = value; }
    public int Capital { get => capital; set => capital = value; }
    public List<float> PctChanges { get => pctChanges; set => pctChanges = value; }
    public List<float> PctDividend { get => pctDividend; set => pctDividend = value; }

    public Investment() { }

    public Investment(string name, int turnsInvest, int capitalInvest,
                        List<float> pctChangesInvest, List<float> pctDividendInvest)
    {
        nameInvestment = name;
        turns = turnsInvest;
        capital = capitalInvest;
        pctChanges = pctChangesInvest;
        pctDividend = pctDividendInvest;
    }

    public void UpdateInvestment()
    {
        // Actualizar capital
        if (pctChanges.Count == 0) return;
        capital += (int)(capital * pctChanges[0]);
        pctChanges.RemoveAt(0);

        // Actualizar dividendo
        pctDividend.RemoveAt(0);

        // Actualizar turnos
        turns--;
    }

    public int Dividend()
    {
        if (pctDividend.Count == 0) return 0;
        return (int)(capital * pctDividend[0]);
    }

    #region Write and Read

    public static void WritePlayerInvestment(NetworkWriter writer, Investment investment)
    {
        writer.WriteString(investment.nameInvestment);
        writer.WriteInt(investment.turns);
        writer.WriteInt(investment.capital);

        // Serializar la lista pctChanges
        writer.WriteInt(investment.pctChanges.Count);
        foreach (var change in investment.pctChanges)
        {
            writer.WriteFloat(change);
        }

        // Serializar la lista pctDividend
        writer.WriteInt(investment.pctDividend.Count);
        foreach (var dividend in investment.pctDividend)
        {
            writer.WriteFloat(dividend);
        }
    }

    public static Investment ReadPlayerInvestment(NetworkReader reader)
    {
        string name = reader.ReadString();
        int turns = reader.ReadInt();
        int capital = reader.ReadInt();

        // Deserializar la lista pctChanges
        int pctChangesCount = reader.ReadInt();
        List<float> pctChanges = new List<float>();
        for (int i = 0; i < pctChangesCount; i++)
        {
            pctChanges.Add(reader.ReadFloat());
        }

        // Deserializar la lista pctDividend
        int pctDividendCount = reader.ReadInt();
        List<float> pctDividend = new List<float>();
        for (int i = 0; i < pctDividendCount; i++)
        {
            pctDividend.Add(reader.ReadFloat());
        }

        return new Investment(name, turns, capital, pctChanges, pctDividend);
    }

    #endregion
}