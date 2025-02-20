using Mirror;
using UnityEngine;

[System.Serializable]
public class Expense
{
    [SerializeField] private int turns;
    [SerializeField] private int cost;

    public int Turns { get => turns; set => turns = value; }
    public int Cost { get => cost; set => cost = value; }

    public Expense() { }

    public Expense(int playerTurns, int amountTurn)
    {
        turns = playerTurns;
        cost = amountTurn;
    }

    public void UpdateExpense()
    {
        turns--;
    }

    public int GetDebt()
    {
        return - cost * turns;
    }

    #region Methods Write and Read

    public static void WritePlayerExpense(NetworkWriter writer, Expense expense)
    {
        writer.WriteInt(expense.turns);
        writer.WriteInt(expense.cost);
    }

    public static Expense ReadPlayerExpense(NetworkReader reader)
    {
        int turns = reader.ReadInt();
        int amount = reader.ReadInt();
        return new Expense(turns, amount);
    }

    #endregion
}
