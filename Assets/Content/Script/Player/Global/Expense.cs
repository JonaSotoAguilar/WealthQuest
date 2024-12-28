using Mirror;

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
