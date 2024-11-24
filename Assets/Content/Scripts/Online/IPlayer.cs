using System.Collections;
using FishNet.Connection;
using UnityEngine;


public interface IPlayer
{

    #region Getters

    int Index { get; }
    string Nickname { get; }
    int Points { get; }
    int Money { get; }
    int Invest { get; }
    int Debt { get; }
    int Income { get; }
    int Salary { get; set; }
    int Expense { get; }

    Transform Transform { get; }
    //PlayerMovement Movement { get; }
    NetworkConnection Connection { get; }

    #endregion

    #region Initialize

    void InitializeData(int i, string name, int model);

    #endregion

    #region Change Values

    void AddPoints(int points);
    void AddMoney(int money);
    void AddInvest(int invest);
    void AddDebt(int debt);
    void AddInvestment(Investment newInvestment);
    void AddExpense(Expense newExpense, bool isRecurrent);

    #endregion

    #region Actions

    void ProccessFinances();
    int Position { get; set; }
    void CornerPosition();
    void CreateQuestion();

    #endregion
}
