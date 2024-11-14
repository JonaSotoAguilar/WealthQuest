using UnityEngine;
using System.Globalization;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;

public class PlayerNetwork : NetworkBehaviour, IPlayer
{
    // Inspector Variables
    [SerializeField] protected GameData gameData;

    [SerializeField, Tooltip("Interest applied to expenses not paid on time")]
    private float interest = 0.05f;

    // Static Variables
    protected PlayerHUD hud;
    private readonly CultureInfo chileanCulture = new CultureInfo("es-CL");

    // Player Data
    private readonly SyncVar<int> index = new SyncVar<int>(-1);
    private readonly SyncVar<string> nickName = new SyncVar<string>();
    private readonly SyncVar<int> points = new SyncVar<int>();
    protected readonly SyncVar<int> position = new SyncVar<int>();
    private readonly SyncVar<int> characterID = new SyncVar<int>();

    // Player Finances
    private readonly SyncVar<int> money = new SyncVar<int>();
    private readonly SyncVar<int> salary = new SyncVar<int>();
    private readonly SyncVar<int> invest = new SyncVar<int>();
    private readonly SyncVar<int> debt = new SyncVar<int>();
    private readonly SyncList<Investment> investments = new SyncList<Investment>();
    private readonly SyncList<Expense> expenses = new SyncList<Expense>();

    // Player Finances Turn
    private readonly SyncVar<int> income = new SyncVar<int>();
    private readonly SyncVar<int> expense = new SyncVar<int>();


    void Awake()
    {
        SuscribeEvents();
    }

    void SuscribeEvents()
    {
        nickName.OnChange += OnChangeNickName;
        position.OnChange += OnChangePosition;
        points.OnChange += OnChangePoints;
        money.OnChange += OnChangeMoney;
        invest.OnChange += OnChangeInvest;
        debt.OnChange += OnChangeDebt;
        income.OnChange += OnChangeIncome;
        expense.OnChange += OnChangeExpense;
        investments.OnChange += OnChangeInvestments;
        expenses.OnChange += OnChangeExpenses;
    }

    #region Methods Player Controller

    public virtual void InitializePosition() { }

    public virtual PlayerMovement Movement { get; }

    public virtual IEnumerator CmdCreateQuestion() { yield return null; }

    #endregion

    #region Methods Getters & Setters

    public int Index { get => index.Value; set => CmdChangeIndex(value); }
    public string PlayerName { get => nickName.Value; set => CmdChangeNickName(value); }
    public int Position { get => position.Value; set => CmdChangePosition(value); }
    public int Points { get => points.Value; }
    public int Money { get => money.Value; }
    public int Salary { get => salary.Value; set => CmdChangeSalary(value); }
    public int Invest { get => invest.Value; }
    public int Debt { get => debt.Value; }
    public int Income { get => income.Value; }
    public int Expense { get => expense.Value; }
    public PlayerHUD HUD { get => hud; set => hud = value; }
    public Transform Transform { get => transform; }

    #endregion

    //FIXME: Agregar PlayerData para GameData
    #region Methods OnChange Values

    private void OnChangeNickName(string oldNickName, string newNickName, bool asServer)
    {
        hud.PlayerName.text = newNickName;
        //gameData.PlayersData[Index].PlayerName = newNickName;
    }

    private void OnChangePosition(int oldPosition, int newPosition, bool asServer)
    {
        gameData.playersData[Index].Position = newPosition;
    }

    private void OnChangePoints(int oldPoints, int newPoints, bool asServer)
    {
        hud.Kpf.text = newPoints.ToString();
        gameData.playersData[Index].Points = newPoints;
    }

    private void OnChangeMoney(int oldMoney, int newMoney, bool asServer)
    {
        hud.Money.text = newMoney.ToString("C0", chileanCulture);
        gameData.playersData[Index].Money = newMoney;
    }

    private void OnChangeInvest(int oldInvest, int newInvest, bool asServer)
    {
        hud.Invest.text = newInvest.ToString("C0", chileanCulture);
        gameData.playersData[Index].Invest = newInvest;
    }

    private void OnChangeDebt(int oldDebt, int newDebt, bool asServer)
    {
        hud.Debt.text = newDebt.ToString("C0", chileanCulture);
        gameData.playersData[Index].Debt = newDebt;
    }

    private void OnChangeIncome(int oldIncome, int newIncome, bool asServer)
    {
        hud.Income.text = newIncome.ToString("C0", chileanCulture);
        gameData.playersData[Index].Income = newIncome;
    }

    private void OnChangeExpense(int oldExpense, int newExpense, bool asServer)
    {
        hud.Expense.text = newExpense.ToString("C0", chileanCulture);
        gameData.playersData[Index].Income = newExpense;
    }

    private void OnChangeInvestments(SyncListOperation op, int index, Investment oldInvestment, Investment newInvestment, bool asServer)
    {
        switch (op)
        {
            case SyncListOperation.Add:
                gameData.playersData[Index].Investments.Add(newInvestment);
                break;
            case SyncListOperation.RemoveAt:
                gameData.playersData[Index].Investments.RemoveAt(index);
                break;
            case SyncListOperation.Insert:
                break;
            case SyncListOperation.Set:
                gameData.playersData[Index].Investments[index] = newInvestment;
                break;
            case SyncListOperation.Clear:
                break;
            case SyncListOperation.Complete:
                break;
        }
    }

    private void OnChangeExpenses(SyncListOperation op, int index, Expense oldExpense, Expense newExpense, bool asServer)
    {
        switch (op)
        {
            case SyncListOperation.Add:
                gameData.playersData[Index].Expenses.Add(newExpense);
                break;
            case SyncListOperation.RemoveAt:
                gameData.playersData[Index].Expenses.RemoveAt(index);
                break;
            case SyncListOperation.Insert:
                break;
            case SyncListOperation.Set:
                gameData.playersData[Index].Expenses[index] = newExpense;
                break;
            case SyncListOperation.Clear:
                break;
            case SyncListOperation.Complete:
                break;
        }
    }

    #endregion

    #region Methods Change Values Client
    public void AddPoints(int points) => CmdAddPoints(points);

    public void AddMoney(int money) => CmdAddMoney(money);

    public void AddInvest(int invest) => CmdAddInvest(invest);

    public void AddDebt(int debt) => CmdAddDebt(debt);

    public void AddInvestment(Investment newInvestment) => CmdAddInvestment(newInvestment);

    public void AddExpense(Expense newExpense, bool isRecurrent)
    {
        if (isRecurrent) CmdAddExpense(newExpense, false);
        else
        {
            if (money.Value >= newExpense.Amount)
                CmdAddMoney(-newExpense.Amount);
            else
                CmdAddExpense(newExpense, true);
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

    private void ProcessSalary() => CmdAddMoney(salary.Value);

    private void ProcessInvestments()
    {
        if (investments.Count == 0) return;

        for (int i = investments.Count - 1; i >= 0; i--)
        {
            var invest = investments[i];
            CmdUpdateInvestment(i, invest.Dividend, invest.Capital);
        }
    }

    private void ProcessRecurrentExpenses()
    {
        if (expenses.Count == 0) return;

        for (int i = expenses.Count - 1; i >= 0; i--)
        {
            var expense = expenses[i];
            if (money.Value >= expense.Amount)
                CmdUpdateExpense(i, 0);
            else
                CmdUpdateExpense(i, (int)(expense.Amount * interest));
        }
    }

    #endregion

    #region Methods Change Values Server

    [ServerRpc]
    private void CmdChangeIndex(int newIndex)
    {
        index.Value = newIndex;
    }

    [ServerRpc]
    private void CmdChangeNickName(string newNickName)
    {
        nickName.Value = newNickName;
    }

    [ServerRpc]
    private void CmdChangePosition(int newPosition)
    {
        position.Value = newPosition;
    }

    [ServerRpc]
    private void CmdChangeSalary(int newSalary)
    {
        income.Value += newSalary - salary.Value;
        salary.Value = newSalary;
    }

    [ServerRpc]
    private void CmdAddPoints(int addPoints)
    {
        points.Value += addPoints;
    }

    [ServerRpc]
    private void CmdAddMoney(int amount)
    {
        money.Value += amount;
    }

    [ServerRpc]
    private void CmdAddInvest(int addInvest)
    {
        invest.Value += addInvest;
    }

    [ServerRpc]
    private void CmdAddDebt(int addDebt)
    {
        debt.Value += addDebt;
    }

    [ServerRpc]
    private void CmdAddInvestment(Investment newInvestment)
    {
        if (newInvestment.Capital > money.Value) return;

        money.Value -= newInvestment.Capital;
        invest.Value += newInvestment.Capital;
        income.Value += newInvestment.Dividend;
        investments.Add(newInvestment);
    }

    [ServerRpc]
    private void CmdUpdateInvestment(int index, int oldDividend, int oldCapital)
    {
        if (index < 0 || index >= investments.Count) return;

        // Obtiene dividendo anual
        Investment currInvestment = investments[index];
        if (currInvestment.Dividend != 0) money.Value += currInvestment.Dividend;

        // Actualiza Capital y Proximo Dividendo
        currInvestment.UpdateInvestment();
        invest.Value += currInvestment.Capital - oldCapital;
        int nextDividend = currInvestment.Dividend - oldDividend;
        if (nextDividend != 0) income.Value += nextDividend;
        currInvestment.Turns--;
        if (currInvestment.Turns != 0)
        {
            investments.Dirty(index);
            return;
        }

        // Terminó la inversión
        money.Value += currInvestment.Capital;
        invest.Value -= currInvestment.Capital;
        investments.RemoveAt(index);
    }

    [ServerRpc]
    private void CmdAddExpense(Expense newExpense, bool withInterest)
    {
        if (withInterest)
        {
            newExpense.Amount += (int)(newExpense.Amount * interest);
            newExpense.Turns++;
        }
        debt.Value += newExpense.Amount * newExpense.Turns;
        expense.Value += newExpense.Amount;
        expenses.Add(newExpense);
    }

    [ServerRpc]
    private void CmdUpdateExpense(int index, int interest)
    {
        if (index < 0 || index >= expenses.Count) return;

        // No tiene para pagar: Suma interes
        Expense currExpense = expenses[index];
        if (interest > 0)
        {
            currExpense.Amount += interest;
            currExpense.Turns++;
            debt.Value += interest * currExpense.Turns;
            expense.Value += interest;
            expenses.Dirty(index);
            return;
        }

        // Tiene para pagar: Paga la cuota
        money.Value -= currExpense.Amount;
        debt.Value -= currExpense.Amount;
        currExpense.Turns--;
        if (currExpense.Turns != 0)
        {
            expenses.Dirty(index);
            return;
        }

        // Terminó de pagar: Elimina la deuda
        expense.Value -= currExpense.Amount;
        expenses.RemoveAt(index);
    }

    #endregion

}