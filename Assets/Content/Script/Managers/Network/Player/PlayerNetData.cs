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
    [SyncVar] private int salary = 0;
    [SyncVar(hook = nameof(OnChangeInvest))] private int invest = 0;
    [SyncVar(hook = nameof(OnChangeDebt))] private int debt = 0;
    private readonly SyncList<Investment> investments = new SyncList<Investment>();
    private readonly SyncList<Expense> expenses = new SyncList<Expense>();

    // Finances Turn
    [SyncVar(hook = nameof(OnChangeIncome))] private int income = 0;
    [SyncVar(hook = nameof(OnChangeExpense))] private int expense = 0;

    // Variables
    [SerializeField] private float interest = 0.05f;

    #region Getters

    public string UID { get => uid; set => uid = value; }
    public string Nickname { get => nickName; }
    public int CharacterID { get => characterID; }
    public int FinalScore { get => finalScore; }

    public int Position { get => position; }
    public int Points { get => points; }
    public int Level { get => level; }

    public int Money { get => money; }
    public int Salary { get => salary; }
    public int Invest { get => invest; }
    public int Debt { get => debt; }

    public int Income { get => income; }
    public int Expense { get => expense; }

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
        salary = playerData.Salary;

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
        double pointsMoney;
        float finalMoney = money + invest - debt;

        if (finalMoney <= 0) pointsMoney = -Math.Log10(-finalMoney + 1);
        else pointsMoney = Math.Log10(finalMoney + 1);

        finalScore = (int)Math.Round(points + pointsMoney, 2);
        playerData.FinalScore = finalScore;
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
        level = Mathf.Clamp(Level, 1, 3);
        int additionalLevels = Mathf.FloorToInt(Points / 8);
        level = Mathf.Clamp(Level + additionalLevels, 1, 3);
    }

    [Server]
    public void AddMoney(int amount)
    {
        money += amount;
        playerData.Money = money;
    }

    [Server]
    public void NewSalary(int newSalary)
    {
        int oldSalary = salary;
        salary = newSalary;
        income += salary - oldSalary;
        playerData.Salary = salary;
        playerData.Income = income;
    }

    [Server]
    public void AddInvest(int amount)
    {
        invest += amount;
        playerData.Invest = amount;
    }

    [Server]
    public void AddDebt(int amount)
    {
        debt += amount;
        playerData.Debt = debt;
    }

    [Server]
    public void AddIncome(int amount)
    {
        income += amount;
        playerData.Income = income;
    }

    [Server]
    public void AddExpense(int amount)
    {
        expense += amount;
        playerData.Expense = expense;
    }

    [Server]
    public void NewExpense(Expense newExpense, bool isRecurrent)
    {
        if (isRecurrent) AddExpense(newExpense, false);
        else
        {
            if (money >= newExpense.Amount)
                AddMoney(-newExpense.Amount);
            else
                AddExpense(newExpense, true);
        }
    }

    [Server]
    private void AddExpense(Expense newExpense, bool withInterest)
    {
        if (withInterest)
        {
            newExpense.Amount += (int)(newExpense.Amount * interest);
        }
        AddDebt(newExpense.Amount * newExpense.Turns);
        AddExpense(newExpense.Amount);
        expenses.Add(newExpense);
        playerData.Expenses.Add(newExpense);
    }

    [Server]
    public void AddInvestment(Investment newInvestment)
    {
        if (newInvestment.Capital > money) return;
        AddMoney(-newInvestment.Capital);
        AddInvest(newInvestment.Capital);
        AddIncome(newInvestment.Dividend);
        investments.Add(newInvestment);
        playerData.Investments.Add(newInvestment);
    }

    [Server]
    private void UpdateInvestment(int index, int oldDividend, int oldCapital)
    {
        if (index < 0 || index >= investments.Count) return;

        // Obtiene dividendo anual
        Investment currInvestment = investments[index];
        if (currInvestment.Dividend != 0) AddMoney(currInvestment.Dividend);

        // Actualiza Capital y Proximo Dividendo
        currInvestment.UpdateInvestment();
        AddInvest(currInvestment.Capital - oldCapital);
        int nextDividend = currInvestment.Dividend - oldDividend;
        if (nextDividend != 0) AddIncome(nextDividend);
        currInvestment.Turns--;
        if (currInvestment.Turns != 0)
        {
            investments[index] = currInvestment;
            playerData.Investments[index] = currInvestment;
            return;
        }

        // Terminó la inversión
        AddMoney(currInvestment.Capital);
        AddInvest(-currInvestment.Capital);
        investments.RemoveAt(index);
        playerData.Investments.RemoveAt(index);
    }

    [Server]
    private void UpdateExpense(int index, int amountInterest)
    {
        if (index < 0 || index >= expenses.Count) return;

        // No tiene para pagar: Suma interes
        Expense currExpense = expenses[index];
        if (interest > 0)
        {
            currExpense.Amount += amountInterest;
            AddDebt(amountInterest * currExpense.Turns);
            AddExpense(amountInterest);
            expenses[index] = currExpense;
            playerData.Expenses[index] = currExpense;
            return;
        }

        // Tiene para pagar: Paga la cuota
        AddMoney(-currExpense.Amount);
        AddDebt(-currExpense.Amount);
        currExpense.Turns--;
        if (currExpense.Turns != 0)
        {
            expenses[index] = currExpense;
            playerData.Expenses[index] = currExpense;
            return;
        }

        // Terminó de pagar: Elimina la deuda
        AddExpense(-currExpense.Amount);
        expenses.RemoveAt(index);
        playerData.Expenses.RemoveAt(index);
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
    private void ProcessSalary() => money += salary;

    [Server]
    private void ProcessInvestments()
    {
        if (investments.Count == 0) return;

        for (int i = investments.Count - 1; i >= 0; i--)
        {
            var invest = investments[i];
            UpdateInvestment(i, invest.Dividend, invest.Capital);
        }
    }

    [Server]
    private void ProcessRecurrentExpenses()
    {
        if (expenses.Count == 0) return;

        for (int i = expenses.Count - 1; i >= 0; i--)
        {
            var expense = expenses[i];
            if (money >= expense.Amount)
                UpdateExpense(i, 0);
            else
                UpdateExpense(i, (int)(expense.Amount * interest));
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
