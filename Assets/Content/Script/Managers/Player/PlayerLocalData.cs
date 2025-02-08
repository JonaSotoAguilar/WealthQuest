using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerLocalData : MonoBehaviour
{
    // PlayerData
    [SerializeField] private PlayerData playerData;

    // Variables
    [SerializeField] private float interest = 0.05f;
    private int resultPosition = 4;

    #region Getters

    public PlayerData PlayerData { get => playerData; set => playerData = value; }

    public string UID { get => playerData.UID; set => playerData.UID = value; }
    public string Nickname { get => playerData.Nickname; set => playerData.Nickname = value; }
    public int CharacterID { get => playerData.CharacterID; set => playerData.CharacterID = value; }
    public int FinalScore { get => playerData.FinalScore; set => playerData.FinalScore = value; }

    public int Position { get => playerData.Position; set => playerData.Position = value; }
    public int Points { get => playerData.Points; set => playerData.Points = value; }
    public int Level { get => playerData.Level; set => playerData.Level = value; }

    public int Money { get => playerData.Money; set => playerData.Money = value; }
    public int Salary { get => playerData.Salary; set => playerData.Salary = value; }
    public int Invest { get => playerData.Invest; set => playerData.Invest = value; }
    public int Debt { get => playerData.Debt; set => playerData.Debt = value; }
    public List<Investment> Investments { get => playerData.Investments; }
    public List<Expense> Expenses { get => playerData.Expenses; }

    public int Income { get => playerData.Income; set => playerData.Income = value; }
    public int Expense { get => playerData.Expense; set => playerData.Expense = value; }

    public int ResultPosition { get => resultPosition; set => resultPosition = value; }

    #endregion

    #region Change Values

    public void SetFinalScore()
    {
        double pointsCapital = 0;
        int capital = GetFinalCapital();

        if (capital > 0) Math.Log(capital + 1);

        FinalScore = (int)Math.Round(Points + pointsCapital, 2);
        playerData.FinalScore = FinalScore;
    }

    public int GetFinalCapital()
    {
        return Money + Invest - Debt;
    }

    public void SetResultPosition(int position)
    {
        resultPosition = position;
    }

    public void NewPosition(int newPosition)
    {
        Position = newPosition;
    }

    public void AddPoints(int addPoints)
    {
        Points += addPoints;
        OnChangePoints(Points);
        UpdateLevel();
    }

    private void UpdateLevel()
    {
        int newLevel = 1 + Mathf.FloorToInt(Points / 6);
        if (newLevel != Level) Level = Mathf.Clamp(newLevel, 1, 4);
    }

    public void AddMoney(int amount)
    {
        Money += amount;
        OnChangeMoney(Money);
    }

    public void NewSalary(int newSalary)
    {
        int oldSalary = Salary;
        Salary = newSalary;
        AddIncome(Salary - oldSalary);
    }

    public void AddInvest(int amount)
    {
        Invest += amount;
        OnChangeInvest(Invest);
    }

    public void AddDebt(int amount)
    {
        Debt += amount;
        OnChangeDebt(Debt);
    }

    public void AddIncome(int amount)
    {
        Income += amount;
        OnChangeIncome(Income);
    }

    public void AddExpense(int amount)
    {
        Expense += amount;
        OnChangeExpense(Expense);
    }

    public void NewExpense(Expense newExpense, bool isRecurrent)
    {
        if (isRecurrent) AddExpense(newExpense, false);
        else
        {
            if (Money >= newExpense.Amount)
                AddMoney(-newExpense.Amount);
            else
                AddExpense(newExpense, true);
        }
    }

    private void AddExpense(Expense newExpense, bool withInterest)
    {
        if (withInterest)
        {
            newExpense.Amount += (int)(newExpense.Amount * interest);
        }
        AddDebt(newExpense.Amount * newExpense.Turns);
        AddExpense(newExpense.Amount);
        Expenses.Add(newExpense);
    }

    public void AddInvestment(Investment newInvestment)
    {
        if (newInvestment.Capital > Money) return;
        AddMoney(-newInvestment.Capital);
        AddInvest(newInvestment.Capital);
        AddIncome(newInvestment.Dividend);
        Investments.Add(newInvestment);
    }

    private void UpdateInvestment(int index, int oldDividend, int oldCapital)
    {
        if (index < 0 || index >= Investments.Count) return;

        // Obtiene dividendo anual
        Investment currInvestment = Investments[index];
        if (currInvestment.Dividend != 0) AddMoney(currInvestment.Dividend);

        // Actualiza Capital y Proximo Dividendo
        currInvestment.UpdateInvestment();
        AddInvest(currInvestment.Capital - oldCapital);
        int nextDividend = currInvestment.Dividend - oldDividend;
        if (nextDividend != 0) AddIncome(nextDividend);
        currInvestment.Turns--;
        if (currInvestment.Turns != 0)
        {
            Investments[index] = currInvestment;
            return;
        }

        // Terminó la inversión
        AddMoney(currInvestment.Capital);
        AddInvest(-currInvestment.Capital);
        Investments.RemoveAt(index);
    }

    private void UpdateExpense(int index, int amountInterest)
    {
        if (index < 0 || index >= Expenses.Count) return;

        // No tiene para pagar: Suma interes
        Expense currExpense = Expenses[index];
        if (amountInterest > 0)
        {
            currExpense.Amount += amountInterest;
            AddDebt(amountInterest);
            AddExpense(amountInterest);
            Expenses[index] = currExpense;
            return;
        }

        // Tiene para pagar: Paga la cuota
        AddMoney(-currExpense.Amount);
        AddDebt(-currExpense.Amount);
        currExpense.Turns--;
        if (currExpense.Turns != 0)
        {
            Expenses[index] = currExpense;
            return;
        }

        // Terminó de pagar: Elimina la deuda
        AddExpense(-currExpense.Amount);
        Expenses.RemoveAt(index);
    }

    #endregion

    #region Process Finances

    public void ProccessFinances()
    {
        ProcessSalary();
        ProcessInvestments();
        ProcessRecurrentExpenses();
    }

    private void ProcessSalary() => Money += Salary;

    private void ProcessInvestments()
    {
        if (Investments.Count == 0) return;

        for (int i = Investments.Count - 1; i >= 0; i--)
        {
            var invest = Investments[i];
            UpdateInvestment(i, invest.Dividend, invest.Capital);
        }
    }

    private void ProcessRecurrentExpenses()
    {
        if (Expenses.Count == 0) return;

        for (int i = Expenses.Count - 1; i >= 0; i--)
        {
            var expense = Expenses[i];
            if (Money >= expense.Amount)
                UpdateExpense(i, 0);
            else
                UpdateExpense(i, (int)(expense.Amount * interest));
        }
    }

    #endregion

    #region OnChange Values

    private void OnChangePoints(int newPoints)
    {
        GameUIManager.GetHUD(UID).UpdatePoints(newPoints);
    }

    private void OnChangeMoney(int newMoney)
    {
        GameUIManager.GetHUD(UID).UpdateMoney(newMoney);
    }

    private void OnChangeInvest(int newInvest)
    {
        GameUIManager.GetHUD(UID).UpdateInvest(newInvest);
    }

    private void OnChangeDebt(int newDebt)
    {
        GameUIManager.GetHUD(UID).UpdateDebt(newDebt);
    }
    private void OnChangeIncome(int newIncome)
    {
        GameUIManager.GetHUD(UID).UpdateIncome(newIncome);
    }

    private void OnChangeExpense(int newExpense)
    {
        GameUIManager.GetHUD(UID).UpdateExpense(newExpense);
    }

    #endregion

}
