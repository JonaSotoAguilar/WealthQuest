using System.Collections.Generic;
using UnityEngine;
using Mirror;

[System.Serializable]
public class PlayerData
{
    private string uid;
    private string nickName;
    private int characterID;
    private int finalScore;

    private int position;
    private int points;
    private int level;

    private int money;
    private int salary;
    private int invest;
    private int debt;
    private List<Investment> investments;
    private List<Expense> expenses;

    private int income;
    private int expense;


    public string UID { get => uid; set => uid = value; }
    public string Nickname { get => nickName; set => nickName = value; }
    public int CharacterID { get => characterID; set => characterID = value; }
    public int FinalScore { get => finalScore; set => finalScore = value; }

    public int Position { get => position; set => position = value; }
    public int Points { get => points; set => points = value; }
    public int Level { get => level; set => level = value; }

    public int Money { get => money; set => money = value; }
    public int Salary { get => salary; set => salary = value; }
    public int Invest { get => invest; set => invest = value; }
    public int Debt { get => debt; set => debt = value; }
    public List<Investment> Investments { get => investments; }
    public List<Expense> Expenses { get => expenses; }

    public int Income { get => income; set => income = value; }
    public int Expense { get => expense; set => expense = value; }


    public PlayerData() { }

    public PlayerData(string uid, string nickName, int characterID)
    {
        this.uid = uid;
        this.nickName = nickName;
        this.characterID = characterID;

        position = 0;
        points = 0;
        level = 1;

        money = 0;
        salary = 0;
        invest = 0;
        debt = 0;
        investments = new List<Investment>();
        expenses = new List<Expense>();

        income = 0;
        expense = 0;
    }
}

[System.Serializable]
public class Investment
{
    private string nameInvestment;
    private int turns;
    private int capital;
    private int nextDividend;
    private List<float> pctChanges;
    private List<float> pctDividend;

    public string NameInvestment { get => nameInvestment; set => nameInvestment = value; }
    public int Turns { get => turns; set => turns = value; }
    public int Capital { get => capital; set => capital = value; }
    public int Dividend { get => nextDividend; set => nextDividend = value; }
    public List<float> PctChanges { get => pctChanges; set => pctChanges = value; }
    public List<float> PctDividend { get => pctDividend; set => pctDividend = value; }

    public Investment() { }

    public Investment(string name, int turnsInvest, int capitalInvest, int nextDividendInvest,
                        List<float> pctChangesInvest, List<float> pctDividendInvest)
    {
        nameInvestment = name;
        turns = turnsInvest;
        capital = capitalInvest;
        nextDividend = nextDividendInvest;
        pctChanges = pctChangesInvest;
        pctDividend = pctDividendInvest;
    }

    public Investment(string name, int turnsInvest, int capitalInvest,
                            List<float> pctChangesInvest, List<float> pctDividendInvest)
    {
        nameInvestment = name;
        turns = turnsInvest;
        capital = capitalInvest;
        pctChanges = pctChangesInvest;
        pctDividend = pctDividendInvest;

        nextDividend = (int)(capital * pctDividend[0]);
        pctDividend.RemoveAt(0);
    }

    public void UpdateInvestment()
    {
        // Actualizar capital
        if (pctChanges.Count == 0) return;
        capital += (int)(capital * pctChanges[0]);
        pctChanges.RemoveAt(0);

        // Siguiente dividendo
        if (pctDividend.Count == 0)
            nextDividend = 0;
        else
        {
            nextDividend = (int)(capital * pctDividend[0]);
            pctDividend.RemoveAt(0);
        }
    }

    #region Write and Read

    public static void WritePlayerInvestment(NetworkWriter writer, Investment investment)
    {
        writer.WriteString(investment.nameInvestment);
        writer.WriteInt(investment.turns);
        writer.WriteInt(investment.capital);
        writer.WriteInt(investment.nextDividend);

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
        int nextDividend = reader.ReadInt();

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

        return new Investment(name, turns, capital, nextDividend, pctChanges, pctDividend);
    }

    #endregion
}


[System.Serializable]
public class Expense
{
    private int turns;
    private int amount;

    public int Turns { get => turns; set => turns = value; }
    public int Amount { get => amount; set => amount = value; }

    public Expense() { }
    public Expense(int playerTurns, int amountTurn)
    {
        turns = playerTurns;
        amount = amountTurn;
    }

    #region Methods Write and Read

    public static void WritePlayerExpense(NetworkWriter writer, Expense expense)
    {
        writer.WriteInt(expense.turns);
        writer.WriteInt(expense.amount);
    }

    public static Expense ReadPlayerExpense(NetworkReader reader)
    {
        int turns = reader.ReadInt();
        int amount = reader.ReadInt();
        return new Expense(turns, amount);
    }

    #endregion
}

