using System.Collections.Generic;
using FishNet.Serializing;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [SerializeField] private string uid;
    [SerializeField] private int index;
    [SerializeField] private string nickName;
    [SerializeField] private int position;
    [SerializeField] private int points;
    [SerializeField] private int money;
    [SerializeField] private int invest;
    [SerializeField] private int debt;
    [SerializeField] private int salary;
    [SerializeField] private int income;
    [SerializeField] private int expense;
    [SerializeField] private List<Investment> investments;
    [SerializeField] private List<Expense> expenses;
    [SerializeField] private int characterID;

    public int Index { get => index; set => index = value; }
    public string UID { get => uid; set => uid = value; }
    public string NickName { get => nickName; set => nickName = value; }
    public int Position { get => position; set => position = value; }

    public int Points { get => points; set => points = value; }
    public int Money { get => money; set => money = value; }
    public int Invest { get => invest; set => invest = value; }
    public int Debt { get => debt; set => debt = value; }
    public int Salary { get => salary; set => salary = value; }
    public int Income { get => income; set => income = value; }
    public int Expense { get => expense; set => expense = value; }
    public List<Investment> Investments { get => investments; }
    public List<Expense> Expenses { get => expenses; }
    public int CharacterID { get => characterID; set => characterID = value; }

    public void NewPlayer(int playerIndex, string name, int model, string uidProfile = "")
    {
        uid = uidProfile;
        index = playerIndex;
        nickName = name;
        characterID = model;

        position = 0;
        points = 0;
        money = 10000;
        invest = 0;
        debt = 0;
        salary = 0;
        income = 0;
        expense = 0;
        investments = new List<Investment>();
        expenses = new List<Expense>();
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

    // #region Methods Write and Read

    // public static void WritePlayerInvestment(Writer writer, Investment investment)
    // {
    //     writer.WriteString(investment.nameInvestment);
    //     writer.WriteInt32(investment.turns);
    //     writer.WriteInt32(investment.capital);
    //     writer.WriteInt32(investment.nextDividend);

    //     // Serializar la lista pctChanges
    //     writer.WriteInt32(investment.pctChanges.Count);
    //     foreach (var change in investment.pctChanges)
    //     {
    //         writer.WriteSingle(change);
    //     }

    //     // Serializar la lista pctDividend
    //     writer.WriteInt32(investment.pctDividend.Count);
    //     foreach (var dividend in investment.pctDividend)
    //     {
    //         writer.WriteSingle(dividend);
    //     }
    // }

    // public static Investment ReadPlayerInvestment(Reader reader)
    // {
    //     string name = reader.ReadString();
    //     int turns = reader.ReadInt32();
    //     int capital = reader.ReadInt32();
    //     int nextDividend = reader.ReadInt32();

    //     // Deserializar la lista pctChanges
    //     int pctChangesCount = reader.ReadInt32();
    //     List<float> pctChanges = new List<float>();
    //     for (int i = 0; i < pctChangesCount; i++)
    //     {
    //         pctChanges.Add(reader.ReadSingle());
    //     }

    //     // Deserializar la lista pctDividend
    //     int pctDividendCount = reader.ReadInt32();
    //     List<float> pctDividend = new List<float>();
    //     for (int i = 0; i < pctDividendCount; i++)
    //     {
    //         pctDividend.Add(reader.ReadSingle());
    //     }

    //     return new Investment(name, turns, capital, nextDividend, pctChanges, pctDividend);
    // }

    // #endregion

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

    //     #region Methods Write and Read

    //     public static void WritePlayerExpense(PooledWriter writer, Expense expense)
    //     {
    //         writer.WriteInt32(expense.turns);
    //         writer.WriteInt32(expense.amount);
    //     }

    //     public static Expense ReadPlayerExpense(PooledReader reader)
    //     {
    //         int turns = reader.ReadInt32();
    //         int amount = reader.ReadInt32();
    //         return new Expense(turns, amount);
    //     }

    //     #endregion

    // 
}

