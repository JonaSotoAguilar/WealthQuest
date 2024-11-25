using System.Collections.Generic;
using UnityEngine;
using Mirror;

[System.Serializable]
public class PlayerData
{
    private string uid;
    private int index;
    private string playerName;
    private int currentPosition;
    private int scoreKFP;
    private int money;
    private int invest;
    private int debt;
    private int salary;
    private int incomeTurn;
    private int expenseTurn;
    private List<PlayerInvestment> investments;
    private List<PlayerExpense> expenses;
    private int characterID;

    public string UID { get => uid; set => uid = value; }
    public int Index { get => index; set => index = value; }
    public string PlayerName { get => playerName; set => playerName = value; }
    public int CurrentPosition { get => currentPosition; set => currentPosition = value; }

    public int ScoreKFP { get => scoreKFP; set => scoreKFP = value; }
    public int Money { get => money; set => money = value; }
    public int Invest { get => invest; set => invest = value; }
    public int Debt { get => debt; set => debt = value; }
    public int Salary { get => salary; set => salary = value; }
    public int IncomeTurn { get => incomeTurn; set => incomeTurn = value; }
    public int ExpenseTurn { get => debt; set => debt = value; }
    public List<PlayerInvestment> Investments { get => investments; }
    public List<PlayerExpense> Expenses { get => expenses; }
    public int CharacterID { get => characterID; set => characterID = value; }

    public PlayerData() { }

    public PlayerData(string uid, int playerIndex, string name, int model)
    {
        this.uid = uid;
        index = playerIndex;
        playerName = name;
        characterID = model;

        currentPosition = 0;
        scoreKFP = 0;
        money = 10000;
        invest = 0;
        debt = 0;
        salary = 0;
        incomeTurn = 0;
        expenseTurn = 0;
        investments = new List<PlayerInvestment>();
        expenses = new List<PlayerExpense>();
    }

    public void NewPlayer(int playerIndex, string name, int model, string uid = "")
    {
        this.uid = uid;
        index = playerIndex;
        playerName = name;
        characterID = model;

        currentPosition = 0;
        scoreKFP = 0;
        money = 10000;
        invest = 0;
        debt = 0;
        salary = 0;
        incomeTurn = 0;
        expenseTurn = 0;
        investments = new List<PlayerInvestment>();
        expenses = new List<PlayerExpense>();
    }
}

[System.Serializable]
public class PlayerInvestment
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

    public PlayerInvestment() { }

    public PlayerInvestment(string name, int turnsInvest, int capitalInvest, int nextDividendInvest,
                        List<float> pctChangesInvest, List<float> pctDividendInvest)
    {
        nameInvestment = name;
        turns = turnsInvest;
        capital = capitalInvest;
        nextDividend = nextDividendInvest;
        pctChanges = pctChangesInvest;
        pctDividend = pctDividendInvest;
    }

    public PlayerInvestment(string name, int turnsInvest, int capitalInvest,
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
    public static void WritePlayerInvestment(NetworkWriter writer, PlayerInvestment investment)
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

    public static PlayerInvestment ReadPlayerInvestment(NetworkReader reader)
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

        return new PlayerInvestment(name, turns, capital, nextDividend, pctChanges, pctDividend);
    }
    #endregion
}


[System.Serializable]
public class PlayerExpense
{
    private int turns;
    private int amount;

    public int Turns { get => turns; set => turns = value; }
    public int Amount { get => amount; set => amount = value; }

    public PlayerExpense() { }
    public PlayerExpense(int playerTurns, int amountTurn)
    {
        turns = playerTurns;
        amount = amountTurn;
    }

    #region Methods Write and Read

    public static void WritePlayerExpense(NetworkWriter writer, PlayerExpense expense)
    {
        writer.WriteInt(expense.turns);
        writer.WriteInt(expense.amount);
    }

    public static PlayerExpense ReadPlayerExpense(NetworkReader reader)
    {
        int turns = reader.ReadInt();
        int amount = reader.ReadInt();
        return new PlayerExpense(turns, amount);
    }

    #endregion
}

