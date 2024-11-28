using System;
using Mirror;
using UnityEngine;

public class PlayerNetData : NetworkBehaviour
{
    // PlayerData
    private PlayerData playerData;

    // User Data
    [SyncVar] private string uid;
    [SyncVar] private string nickName;
    [SyncVar] private int characterID;
    [SyncVar(hook = nameof(OnChangeFinalScore))] private int finalScore;

    // Game Data
    [SyncVar(hook = nameof(OnChangePosition))] private int position;
    [SyncVar(hook = nameof(OnChangePoints))] private int points;

    // Finances
    [SyncVar(hook = nameof(OnChangeMoney))] private int money;
    [SyncVar(hook = nameof(OnChangeSalary))] private int salary;
    [SyncVar(hook = nameof(OnChangeInvest))] private int invest;
    [SyncVar(hook = nameof(OnChangeDebt))] private int debt;
    private readonly SyncList<Investment> investments = new SyncList<Investment>();
    private readonly SyncList<Expense> expenses = new SyncList<Expense>();

    // Finances Turn
    [SyncVar(hook = nameof(OnChangeIncome))] private int income;
    [SyncVar(hook = nameof(OnChangeExpense))] private int expense;

    // Variables
    [SerializeField] private float interest = 0.1f;

    #region Getters

    public PlayerData PlayerData { get => playerData; set => playerData = value; }

    public string UID { get => uid; set => uid = value; }
    public string Nickname { get => nickName; }
    public int CharacterID { get => characterID; }
    public int FinalScore { get => finalScore; }

    public int Position { get => position; }
    public int Points { get => points; }

    public int Money { get => money; }
    public int Salary { get => salary; }
    public int Invest { get => invest; }
    public int Debt { get => debt; }

    public int Income { get => income; }
    public int Expense { get => expense; }

    #endregion

    #region Initialization

    // FIXME: Igualar a PlayerData
    public void Initialize(string uid, string nickName, int characterID)
    {
        this.uid = uid;
        this.nickName = nickName;
        this.characterID = characterID;

        position = 0;
        points = 0;

        money = 10000;
        salary = 1000;
        invest = 0;
        debt = 0;

        income = 0;
        expense = 0;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        investments.OnAdd += OnAddInvest;
        investments.OnSet += OnChangeInvest;
        investments.OnRemove += OnRemoveInvest;

        expenses.OnAdd += OnAddExpense;
        expenses.OnSet += OnChangeExpense;
        expenses.OnRemove += OnRemoveExpense;
    }

    #endregion

    #region Server Change Values 

    [Server]
    public void SetFinalScore()
    {
        double pointsMoney;
        float finalMoney = money + invest - debt;

        if (finalMoney <= 0) pointsMoney = -Math.Log10(-finalMoney + 1);
        else pointsMoney = Math.Log10(finalMoney + 1);

        finalScore = (int) Math.Round(points + pointsMoney, 2);
    }

    [Server]
    public void NewPosition(int newPosition)
    {
        position = newPosition;
    }

    [Server]
    public void AddPoints(int addPoints)
    {
        points += addPoints;
    }

    [Server]
    public void AddMoney(int amount)
    {
        money += amount;
    }

    [Server]
    public void NewSalary(int newSalary)
    {
        int oldSalary = salary;
        salary = newSalary;
        income += salary - oldSalary;
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
            newExpense.Turns++;
        }
        debt += newExpense.Amount * newExpense.Turns;
        expense += newExpense.Amount;
        expenses.Add(newExpense);
    }

    [Server]
    public void AddInvestment(Investment newInvestment)
    {
        if (newInvestment.Capital > money) return;
        money -= newInvestment.Capital;
        invest += newInvestment.Capital;
        income += newInvestment.Dividend;
        investments.Add(newInvestment);
    }

    [Server]
    private void UpdateInvestment(int index, int oldDividend, int oldCapital)
    {
        if (index < 0 || index >= investments.Count) return;

        // Obtiene dividendo anual
        Investment currInvestment = investments[index];
        if (currInvestment.Dividend != 0) money += currInvestment.Dividend;

        // Actualiza Capital y Proximo Dividendo
        currInvestment.UpdateInvestment();
        invest += currInvestment.Capital - oldCapital;
        int nextDividend = currInvestment.Dividend - oldDividend;
        if (nextDividend != 0) income += nextDividend;
        currInvestment.Turns--;
        if (currInvestment.Turns != 0)
        {
            investments[index] = currInvestment;
            return;
        }

        // Terminó la inversión
        money += currInvestment.Capital;
        invest -= currInvestment.Capital;
        investments.RemoveAt(index);
    }

    [Server]
    private void UpdateExpense(int index, int interest)
    {
        if (index < 0 || index >= expenses.Count) return;

        // No tiene para pagar: Suma interes
        Expense currExpense = expenses[index];
        if (interest > 0)
        {
            currExpense.Amount += interest;
            currExpense.Turns++;
            debt += interest * currExpense.Turns;
            expense += interest;
            expenses[index] = currExpense;
            return;
        }

        // Tiene para pagar: Paga la cuota
        money -= currExpense.Amount;
        debt -= currExpense.Amount;
        currExpense.Turns--;
        if (currExpense.Turns != 0)
        {
            expenses[index] = currExpense;
            return;
        }

        // Terminó de pagar: Elimina la deuda
        expense -= currExpense.Amount;
        expenses.RemoveAt(index);
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

    private void OnChangeFinalScore(int oldScore, int newScore)
    {
        if (playerData != null) playerData.FinalScore = newScore;
    }

    private void OnChangePosition(int oldPosition, int newPosition)
    {
        if (playerData != null) playerData.Position = newPosition;
    }

    private void OnChangePoints(int oldPoints, int newPoints)
    {
        if (playerData != null) playerData.Points = newPoints;

        HUD hud = GameUIManager.GetHUD(uid);
        if (hud != null) hud.UpdatePoints(newPoints);
    }

    private void OnChangeMoney(int oldMoney, int newMoney)
    {
        if (playerData != null) playerData.Money = newMoney;

        HUD hud = GameUIManager.GetHUD(uid);
        if (hud != null) hud.UpdateMoney(newMoney);
    }

    private void OnChangeSalary(int oldSalary, int newSalary)
    {
        if (playerData != null) playerData.Salary = newSalary;
    }

    private void OnChangeInvest(int oldInvest, int newInvest)
    {
        if (playerData != null) playerData.Invest = newInvest;

        HUD hud = GameUIManager.GetHUD(uid);
        if (hud != null) hud.UpdateInvest(newInvest);
    }

    private void OnChangeDebt(int oldDebt, int newDebt)
    {
        if (playerData != null) playerData.Debt = newDebt;

        HUD hud = GameUIManager.GetHUD(uid);
        if (hud != null) hud.UpdateDebt(newDebt);
    }
    private void OnChangeIncome(int oldIncome, int newIncome)
    {
        if (playerData != null) playerData.Income = newIncome;

        HUD hud = GameUIManager.GetHUD(uid);
        if (hud != null) hud.UpdateIncome(newIncome);
    }

    private void OnChangeExpense(int oldExpense, int newExpense)
    {
        if (playerData != null) playerData.Expense = newExpense;

        HUD hud = GameUIManager.GetHUD(uid);
        if (hud != null) hud.UpdateExpense(newExpense);
    }

    private void OnAddInvest(int index)
    {
        if (playerData != null) playerData.Investments.Add(investments[index]);
    }

    private void OnChangeInvest(int index, Investment oldInvest)
    {
        if (playerData != null) playerData.Investments[index] = investments[index];
    }

    private void OnRemoveInvest(int index, Investment oldInvest)
    {
        if (playerData != null) playerData.Investments.Remove(oldInvest);
    }

    private void OnAddExpense(int index)
    {
        if (playerData != null) playerData.Expenses.Add(expenses[index]);
    }

    private void OnChangeExpense(int index, Expense oldExpense)
    {
        if (playerData != null) playerData.Expenses[index] = expenses[index];
    }

    private void OnRemoveExpense(int index, Expense oldExpense)
    {
        if (playerData != null) playerData.Expenses.Remove(oldExpense);
    }

    #endregion
}
