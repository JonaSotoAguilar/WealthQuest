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
        int addLevel = Mathf.FloorToInt(Points / 6);
        int newLevel = Level + addLevel;
        if (newLevel != Level) Level = Mathf.Clamp(newLevel, 1, 4);
    }

    public void AddMoney(int amount)
    {
        Money += amount;
        if (Money < 0) Money = 0;
        OnChangeMoney(Money);
    }

    public void AddInvest(int amount)
    {
        Invest += amount;
        if (Invest < 0) Invest = 0;
        OnChangeInvest(Invest);
    }

    public void AddDebt(int amount)
    {
        Debt += amount;
        if (Debt < 0) Debt = 0;
        OnChangeDebt(Debt);
    }

    public void AddIncome(int amount)
    {
        Income += amount;
        if (Income < 0) Income = 0;
        OnChangeIncome(Income);
    }

    public void AddExpense(int amount)
    {
        Expense += amount;
        if (Expense < 0) Expense = 0;
        OnChangeExpense(Expense);
    }

    public void NewExpense(Expense newExpense, bool isRecurrent)
    {
        if (isRecurrent) NewDebt(newExpense, false);
        else
        {
            if (Money >= -newExpense.Cost)
                AddMoney(newExpense.Cost);
            else
                NewDebt(newExpense, true);
        }
    }

    private void NewDebt(Expense newExpense, bool withInterest = false)
    {
        // Aplica interes a la deuda
        if (withInterest) newExpense.Cost += (int) (newExpense.Cost * interest);

        // Agrega deuda
        AddDebt(newExpense.GetDebt());
        AddExpense(-newExpense.Cost);
        Expenses.Add(newExpense);
    }

    public void AddInvestment(Investment newInvestment)
    {
        if (newInvestment.Capital > Money) return;
        AddMoney(-newInvestment.Capital);
        AddInvest(newInvestment.Capital);
        Investments.Add(newInvestment);
    }

    private void UpdateInvestment(int index)
    {
        if (index < 0 || index >= Investments.Count) return;

        Investment currInvestment = Investments[index];
        int oldCapital = currInvestment.Capital;

        // Obtiene dividendo anual
        if (currInvestment.Dividend() != 0) AddMoney(currInvestment.Dividend());

        // Actualiza Capital
        currInvestment.UpdateInvestment();
        AddInvest(currInvestment.Capital - oldCapital);

        // Quedan turnos
        if (currInvestment.Turns != 0)
        {
            Investments[index] = currInvestment;
        }
        else
        {
            AddMoney(currInvestment.Capital);
            AddInvest(-currInvestment.Capital);
            Investments.RemoveAt(index);
        }   
    }

    private void UpdateExpense(int index, bool applyInterest)
    {
        // No existe el gasto
        if (index < 0 || index >= Expenses.Count) return;
        Expense currExpense = Expenses[index];
        int oldDebt = currExpense.GetDebt();

        // Evalua si puede pagar
        if (applyInterest)
        {
            // 1. No tiene para pagar: Suma interes
            int amountInterest = (int)(currExpense.Cost * interest);
            currExpense.Cost += amountInterest;
            AddDebt(currExpense.GetDebt() - oldDebt);
            AddExpense(-amountInterest);
        }
        else
        {
            // 2. Puede pagar: Paga la cuota
            AddMoney(currExpense.Cost);
            AddDebt(currExpense.Cost);
            currExpense.UpdateExpense();
        }

        // Quedan turnos
        if (currExpense.Turns != 0)
        {
            // Actualiza gasto
            Expenses[index] = currExpense;
        }
        else
        {
            // Elimina gasto
            AddExpense(currExpense.Cost);
            Expenses.RemoveAt(index);
        }
    }

    #endregion

    #region Process Finances

    public void ProccessFinances()
    {
        ProcessSalary();
        ProcessInvestments();
        ProcessRecurrentExpenses();
    }

    private void ProcessSalary()
    {
        AddMoney(Income);
    }

    private void ProcessInvestments()
    {
        if (Investments.Count == 0) return;

        for (int i = Investments.Count - 1; i >= 0; i--)
        {
            UpdateInvestment(i);
        }
    }

    private void ProcessRecurrentExpenses()
    {
        if (Expenses.Count == 0) return;

        for (int i = Expenses.Count - 1; i >= 0; i--)
        {
            var expense = Expenses[i];
            if (Money >= -expense.Cost)
                UpdateExpense(i, false);
            else
                UpdateExpense(i, true);
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
