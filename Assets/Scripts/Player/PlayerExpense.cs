using UnityEngine;

[System.Serializable]
public class PlayerExpense
{
    [SerializeField] private int turns;
    [SerializeField] private int amount;

    public int Turns { get => turns; set => turns = value; }
    public int Amount { get => amount; set => amount = value; }

    public PlayerExpense(int playerTurns, int amountTurn)
    {
        turns = playerTurns;
        amount = amountTurn;
    }

}