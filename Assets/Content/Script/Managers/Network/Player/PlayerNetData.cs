using System;
using Mirror;
using UnityEngine;

public class PlayerNetData : NetworkBehaviour
{
    // PlayerData
    private PlayerData playerData;

    // User Data
    [SyncVar] private string uid = "";
    [SyncVar] private string nickName = "";
    [SyncVar(hook = nameof(OnCharacterIDChanged))] private int characterID = 0;
    [SyncVar] private int finalScore = 0;

    // Game Data
    [SyncVar] private int position = 0;
    [SyncVar(hook = nameof(OnChangePoints))] private int points = 0;
    [SyncVar] private int level = 1;

    // Finances
    [SyncVar(hook = nameof(OnChangeMoney))] private int money = 0;
    [SyncVar(hook = nameof(OnChangeInvest))] private int invest = 0;
    [SyncVar(hook = nameof(OnChangeDebt))] private int debt = 0;
    private readonly SyncList<Investment> investments = new SyncList<Investment>();
    private readonly SyncList<Expense> expenses = new SyncList<Expense>();

    // Finances Turn
    [SyncVar(hook = nameof(OnChangeIncome))] private int income = 0;
    [SyncVar(hook = nameof(OnChangeExpense))] private int expense = 0;

    // Variables
    [SerializeField] private float interest = 0.05f;
    [SyncVar] private int resultPosition = 4;

    #region Getters

    public string UID { get => uid; set => uid = value; }
    public string Nickname { get => nickName; }
    public int CharacterID { get => characterID; }
    public int FinalScore { get => finalScore; }

    public int Position { get => position; }
    public int Points { get => points; }
    public int Level { get => level; }

    public int Money { get => money; }
    public int Invest { get => invest; }
    public int Debt { get => debt; }

    public int Income { get => income; }
    public int Expense { get => expense; }

    public int ResultPosition { get => resultPosition; set => resultPosition = value; }

    #endregion

    #region Initialization

    public void Initialize()
    {
        uid = playerData.UID;
        nickName = playerData.Nickname;
        characterID = playerData.CharacterID;

        position = playerData.Position;
        points = playerData.Points;
        level = playerData.Level;

        money = playerData.Money;

        invest = playerData.Invest;
        debt = playerData.Debt;

        income = playerData.Income;
        expense = playerData.Expense;

        for (int i = 0; i < playerData.Investments.Count; i++)
            investments.Add(playerData.Investments[i]);

        for (int i = 0; i < playerData.Expenses.Count; i++)
            expenses.Add(playerData.Expenses[i]);
    }

    #endregion

    #region Server Change Values 

    [Server]
    public void SetPlayerData(PlayerData playerData)
    {
        this.playerData = playerData;
    }

    [Server]
    public void SetFinalScore()
    {
        double pointsCapital = 0;
        int capital = GetFinalCapital();

        if (capital > 0) Math.Log(capital + 1);

        finalScore = (int)Math.Round(points + pointsCapital, 2);
        playerData.FinalScore = finalScore;
    }

    public int GetFinalCapital()
    {
        return Money + Invest - Debt;
    }

    public void SetResultPosition(int position)
    {
        resultPosition = position;
    }

    [Server]
    public void NewPosition(int newPosition)
    {
        position = newPosition;
        playerData.Position = newPosition;
    }

    [Server]
    public void AddPoints(int addPoints)
    {
        points += addPoints;
        playerData.Points = points;
        UpdateLevel();
    }

    [Server]
    private void UpdateLevel()
    {
        int addLevel = Mathf.FloorToInt(Points / 6);
        int newLevel = level + addLevel;
        if (newLevel != level) level = Mathf.Clamp(newLevel, 1, 4);
    }

    [Server]
    public void AddMoney(int amount)
    {
        money += amount;
        if (money < 0) money = 0;
        playerData.Money = money;
    }

    [Server]
    public void AddInvest(int amount)
    {
        invest += amount;
        if (invest < 0) invest = 0;
        playerData.Invest = amount;
    }

    [Server]
    public void AddDebt(int amount)
    {
        debt += amount;
        if (debt < 0) debt = 0;
        playerData.Debt = debt;
    }

    [Server]
    public void AddIncome(int amount)
    {
        income += amount;
        if (income < 0) income = 0;
        playerData.Income = income;
    }

    [Server]
    public void AddExpense(int amount)
    {
        expense += amount;
        if (expense < 0) expense = 0;
        playerData.Expense = expense;
    }

    [Server]
    public void NewExpense(Expense newExpense, bool isRecurrent)
    {
        int absCost = Mathf.Abs(newExpense.Cost);
        if (isRecurrent) NewDebt(newExpense, false);
        else
        {
            if (money >= absCost)
                AddMoney(newExpense.Cost);
            else
                NewDebt(newExpense, true);
        }
    }

    [Server]
    private void NewDebt(Expense newExpense, bool withInterest)
    {
        // Aplica interes a la deuda
        if (withInterest) newExpense.Cost += (int)(newExpense.Cost * interest);

        // Agrega la deuda
        AddDebt(newExpense.GetDebt());
        AddExpense(-newExpense.Cost);
        expenses.Add(newExpense);
        playerData.Expenses.Add(newExpense);
    }

    [Server]
    public void AddInvestment(Investment newInvestment)
    {
        if (newInvestment.Capital > money) return;
        AddMoney(-newInvestment.Capital);
        AddInvest(newInvestment.Capital);
        investments.Add(newInvestment);
        playerData.Investments.Add(newInvestment);
    }

    [Server]
    private void UpdateInvestment(int index)
    {
        if (index < 0 || index >= investments.Count) return;

        Investment currInvestment = investments[index];
        int oldCapital = currInvestment.Capital;

        // Obtiene dividendo anual
        if (currInvestment.Dividend() != 0) AddMoney(currInvestment.Dividend());

        // Actualiza Capital 
        currInvestment.UpdateInvestment();
        AddInvest(currInvestment.Capital - oldCapital);

        // Quedan turnos
        if (currInvestment.Turns != 0)
        {
            investments[index] = currInvestment;
            playerData.Investments[index] = currInvestment;
        }
        else
        {
            AddMoney(currInvestment.Capital);
            AddInvest(-currInvestment.Capital);
            investments.RemoveAt(index);
            playerData.Investments.RemoveAt(index);
        }
    }

    [Server]
    private void UpdateExpense(int index, bool applyInterest)
    {
        // No existe el gasto
        if (index < 0 || index >= expenses.Count) return;
        Expense currExpense = expenses[index];
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
            // Actualiza el gasto
            expenses[index] = currExpense;
            playerData.Expenses[index] = currExpense;
        }
        else
        {
            // Elimina el gasto
            AddExpense(currExpense.Cost);
            expenses.RemoveAt(index);
            playerData.Expenses.RemoveAt(index);
        }
    }

    #endregion

    #region Process Finances

    [Server]
    public void ProccessFinances()
    {
        ProcessSalary();
        ProcessInvestments();
        ProcessRecurrentExpenses();
    }

    [Server]
    private void ProcessSalary()
    {
        AddMoney(income);
    }

    [Server]
    private void ProcessInvestments()
    {
        if (investments.Count == 0) return;

        for (int i = investments.Count - 1; i >= 0; i--)
        {
            UpdateInvestment(i);
        }
    }

    [Server]
    private void ProcessRecurrentExpenses()
    {
        if (expenses.Count == 0) return;

        for (int i = expenses.Count - 1; i >= 0; i--)
        {
            var expense = expenses[i];
            if (money >= -expense.Cost)
                UpdateExpense(i, false);
            else
                UpdateExpense(i, true);
        }
    }

    #endregion

    #region OnChange Values

    private void OnChangePoints(int oldPoints, int newPoints)
    {
        HUD hud = GameUIManager.GetHUD(uid);
        if (hud != null) hud.UpdatePoints(newPoints);
    }

    private void OnChangeMoney(int oldMoney, int newMoney)
    {
        HUD hud = GameUIManager.GetHUD(uid);
        if (hud != null) hud.UpdateMoney(newMoney);
    }

    private void OnChangeInvest(int oldInvest, int newInvest)
    {
        HUD hud = GameUIManager.GetHUD(uid);
        if (hud != null) hud.UpdateInvest(newInvest);
    }

    private void OnChangeDebt(int oldDebt, int newDebt)
    {
        HUD hud = GameUIManager.GetHUD(uid);
        if (hud != null) hud.UpdateDebt(newDebt);
    }

    private void OnChangeIncome(int oldIncome, int newIncome)
    {
        HUD hud = GameUIManager.GetHUD(uid);
        if (hud != null) hud.UpdateIncome(newIncome);
    }

    private void OnChangeExpense(int oldExpense, int newExpense)
    {
        HUD hud = GameUIManager.GetHUD(uid);
        if (hud != null) hud.UpdateExpense(newExpense);
    }

    private void OnCharacterIDChanged(int oldID, int newID)
    {
        if (newID == 0) return;
        RelayService.Instance.UpdateCharacter(this, newID);
    }

    #endregion
}
