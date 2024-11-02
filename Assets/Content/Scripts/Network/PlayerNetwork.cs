using UnityEngine;
using Mirror;
using System.Globalization;
using System;
using System.Collections.Generic;
using Mirror.Examples.Basic;

public class PlayerNetwork : NetworkBehaviour
{
    // Inspector Variables
    [SerializeField] private PlayerHUD playerHUD;
    [SerializeField] private float interest = 0.05f;

    // Private Variables
    private readonly CultureInfo chileanCulture = new CultureInfo("es-CL");

    // Player Data
    [SyncVar] private int index;
    [SyncVar(hook = nameof(OnChangeNickName))] private string nickName;
    [SyncVar] private int squarePosition;

    // Player Finances
    [SyncVar] private int salary;
    [SyncVar(hook = nameof(OnChangePoints))] private int points;
    [SyncVar(hook = nameof(OnChangeMoney))] private int money;
    [SyncVar(hook = nameof(OnChangeInvest))] private int invest;
    [SyncVar(hook = nameof(OnChangeDebt))] private int debt;
    private readonly SyncList<PlayerInvestment> investments = new SyncList<PlayerInvestment>();
    private readonly SyncList<PlayerExpense> expenses = new SyncList<PlayerExpense>();

    // Player Finances Turn
    [SyncVar(hook = nameof(OnChangeIncome))] private int income;
    [SyncVar(hook = nameof(OnChangeExpense))] private int expense;

    // Player Character
    [SyncVar] private int characterID;

    #region Methods OnChange Values
    private void OnChangeNickName(string oldNickName, string newNickName)
    {
        playerHUD.PlayerName.text = newNickName;
    }

    private void OnChangePoints(int oldPoints, int newPoints)
    {
        playerHUD.Kpf.text = newPoints.ToString();
    }

    private void OnChangeMoney(int oldMoney, int newMoney)
    {
        playerHUD.Money.text = newMoney.ToString("C0", chileanCulture);
    }

    private void OnChangeInvest(int oldInvest, int newInvest)
    {
        playerHUD.Invest.text = newInvest.ToString("C0", chileanCulture);
    }

    private void OnChangeDebt(int oldDebt, int newDebt)
    {
        playerHUD.Debt.text = newDebt.ToString("C0", chileanCulture);
    }

    private void OnChangeIncome(int oldIncome, int newIncome)
    {
        playerHUD.Income.text = newIncome.ToString("C0", chileanCulture);
    }

    private void OnChangeExpense(int oldExpense, int newExpense)
    {
        playerHUD.Expense.text = newExpense.ToString("C0", chileanCulture);
    }

    #endregion

    #region Methods Server
    [Command]
    private void CmdChangeNickName(string nickName)
    {
        this.nickName = nickName;
    }

    [Command]
    private void CmdChangeSalary(int salary)
    {
        this.income += salary - this.salary;
        this.salary = salary;
    }

    [Command]
    private void CmdAddPoints(int points)
    {
        this.points += points;
    }

    [Command]
    private void CmdAddMoney(int money)
    {
        this.money += money;
    }

    [Command]
    private void CmdAddInvest(int invest)
    {
        this.invest += invest;
    }

    [Command]
    private void CmdAddDebt(int debt)
    {
        this.debt += debt;
    }

    [Command]
    private void CmdAddInvestment(string name, int turns, int capital, int dividend, List<float> pctChanges, List<float> pctDividend)
    {
        PlayerInvestment newInvestment = new PlayerInvestment(name, turns, capital, dividend, pctChanges, pctDividend);
        this.money -= newInvestment.Capital;
        this.invest += newInvestment.Capital;
        this.income += newInvestment.Dividend;
        investments.Add(newInvestment);
    }

    [Command]
    private void CmdUpdateInvestment(int index, int oldDividend, int oldCapital)
    {
        if (index < 0 || index >= investments.Count) return;
        PlayerInvestment investment = investments[index];

        // Obtiene dividendo anual
        if (investment.Dividend != 0) this.money += investment.Dividend;

        // Actualiza Capital y Proximo Dividendo
        investment.UpdateInvestment();
        this.invest += investment.Capital - oldCapital;
        int nextDividend = investment.Dividend - oldDividend;
        if (nextDividend != 0) this.income += nextDividend;
        investment.Turns--;
        if (investment.Turns != 0)
        {
            investments[index] = investment;
            return;
        }

        // Terminó la inversión
        this.money += investment.Capital;
        this.invest -= investment.Capital;
        investments.Remove(investment);
    }

    [Command]
    private void CmdAddExpense(int turns, int amount, int interest)
    {
        PlayerExpense newExpense = new PlayerExpense(turns, amount);

        if (interest > 0)
        {
            newExpense.Amount += interest;
            newExpense.Turns++;
        }
        this.debt += newExpense.Amount * newExpense.Turns;
        this.expense += newExpense.Amount;
        expenses.Add(newExpense);
    }

    [Command]
    private void CmdUpdateExpense(int index, int interest)
    {
        if (index < 0 || index >= expenses.Count) return;
        PlayerExpense expense = expenses[index];

        // No tiene para pagar: Suma interes
        if (interest > 0)
        {
            expense.Amount += interest;
            expense.Turns++;
            this.debt += interest * expense.Turns;
            this.expense += interest;
            expenses[index] = expense;
            return;
        }

        // Tiene para pagar: Paga la cuota
        this.money -= expense.Amount;
        this.debt -= expense.Amount;
        expense.Turns--;
        if (expense.Turns != 0)
        {
            expenses[index] = expense;
            return;
        }

        // Terminó de pagar: Elimina la deuda
        this.expense -= expense.Amount;
        expenses.Remove(expense);
    }
    #endregion

    #region Methods Change Values
    public void ChangeNickName(string nickName)
    {
        CmdChangeNickName(nickName);
    }

    public void ChangeSalary(int salary)
    {
        CmdChangeSalary(salary);
    }

    public void AddPoints(int points)
    {
        CmdAddPoints(points);
    }

    public void AddMoney(int money)
    {
        CmdAddMoney(money);
    }

    public void AddInvest(int invest)
    {
        CmdAddInvest(invest);
    }

    public void ChangeDebt(int debt)
    {
        CmdAddDebt(debt);
    }

    public void AddInvestment(PlayerInvestment investment)
    {
        if (money < investment.Capital) return;
        CmdAddInvestment(investment.NameInvestment, investment.Turns, investment.Capital, investment.Dividend, investment.PctChanges, investment.PctDividend);
    }

    public void AddExpense(PlayerExpense expense, bool isRecurrent)
    {
        if (isRecurrent) CmdAddExpense(expense.Turns, expense.Amount, 0);
        else
        {
            if (money >= expense.Amount)
                CmdAddMoney(-expense.Amount);
            else
                CmdAddExpense(expense.Turns, expense.Amount, (int)(expense.Amount * interest));
        }
    }
    #endregion

    #region Methods Process Finances
    public void ProccessFinances()
    {
        ProcessSalary();
        ProcessInvestments();
        ProcessRecurrentExpenses();
    }

    public void ProcessSalary() => CmdAddMoney(salary);

    public void ProcessInvestments()
    {
        if (investments.Count == 0) return;

        for (int i = investments.Count - 1; i >= 0; i--)
        {
            var invest = investments[i];
            CmdUpdateInvestment(i, invest.Dividend, invest.Capital);
        }
    }

    public void ProcessRecurrentExpenses()
    {
        if (expenses.Count == 0) return;

        for (int i = expenses.Count - 1; i >= 0; i--)
        {
            var expense = expenses[i];
            if (money >= expense.Amount)
                CmdUpdateExpense(i, 0);
            else
                CmdUpdateExpense(i, (int)(expense.Amount * interest));
        }
    }
    #endregion

    #region Methods Getters
    public int Index { get => index; }
    public string PlayerName { get => nickName; }
    #endregion
}