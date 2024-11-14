using System.Collections;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public interface IPlayer
{
    #region Methods Getters & Setters

    int Index { get; set; }
    string PlayerName { get; set; }
    public int Position { get; set; }
    int Points { get; }
    int Money { get; }
    int Salary { get; set; }
    int Invest { get; }
    int Debt { get; }
    int Income { get; }
    int Expense { get; }
    PlayerHUD HUD { get; set; }
    Transform Transform { get; }

    #endregion

    #region Methods Change Values

    void AddPoints(int points);
    void AddMoney(int money);
    void AddInvest(int invest);
    void AddDebt(int debt);
    void AddInvestment(Investment investment);
    void AddExpense(Expense expense, bool isRecurrent);

    #endregion

    #region Methods Process Finances

    void ProccessFinances();

    #endregion

    #region Methods Player Controller

    void InitializePosition();
    PlayerMovement Movement { get; }
    IEnumerator CmdCreateQuestion();

    #endregion

}
