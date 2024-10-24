using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

[System.Serializable]
public class PlayerData : MonoBehaviour
{
    [SerializeField] private int index;                             // Índice del jugador
    private string playerName;                                      // Nombre del jugador
    private int currentPosition;                                    // Posición actual del jugador

    [Header("Finances")]
    [SerializeField] private int scoreKFP;                          // financial knowledge points: puntos de conocimiento financiero
    [SerializeField] private int money;                             // Dinero del jugador
    [SerializeField] private int invest;                            // Inversion del jugador
    [SerializeField] private int debt;                              // Deuda del jugador
    [SerializeField] private int salary;                            // Salario del jugador
    [SerializeField] private int incomeTurn;                        // Ingresos por turno
    [SerializeField] private int expenseTurn;                       // Gastos por turno
    [SerializeField] private List<PlayerInvestment> investments;    // Lista de inversiones
    [SerializeField] private List<PlayerExpense> expenses;          // Lista de gastos

    [Header("Player HUD")]
    [SerializeField] private PlayerHUD playerHUD;                   // HUD del jugador
    private CultureInfo chileanCulture = new CultureInfo("es-CL");  // Cultura chilena para formato de moneda

    [Header("Player Model")]
    [SerializeField] private string modelName;                        // Modelo del jugador

    public int Index { get => index; set => index = value; }
    public string PlayerName { get => playerName; set => playerName = value; }
    public int CurrentPosition { get => currentPosition; set => currentPosition = value; }

    public int ScoreKFP { get => scoreKFP; set => scoreKFP = value; }
    public int Money { get => money; set => money = value; }
    public int Invest { get => invest; set => invest = value; }
    public int Debt { get => debt; set => debt = value; }
    public int Salary { get => salary; set => salary = value; }
    public int IncomeTurn { get => incomeTurn; set => incomeTurn = value; }
    public int ExpenseTurn { get => debt; set => debt = value; }
    public List<PlayerInvestment> Investments { get => investments; }
    public List<PlayerExpense> Expenses { get => expenses; }

    public PlayerHUD PlayerHUD { get => playerHUD; set => playerHUD = value; }

    public void NewPlayer(int playerIndex, string name, string model)
    {
        index = playerIndex;
        playerName = name;
        modelName = model;

        currentPosition = 0;
        scoreKFP = 0;
        money = 10000;
        invest = 0;
        debt = 0;
        salary = 0;
        incomeTurn = 0;
        expenseTurn = 0;
        investments = new List<PlayerInvestment>();
        expenses = new List<PlayerExpense>();
    }

    // Inicializar datos del jugador
    public void InitializePlayer(int playerIndex, string name, int position,
                                 int playerScoreKFP, int playerMoney, int playerInvest, int playerDebt,
                                 int playerSalary, int playerIncomeTurn, int playerExpenseTurn,
                                 List<PlayerInvestment> playerInvestments, List<PlayerExpense> playerExpenses)
    {
        index = playerIndex;
        playerName = name;
        currentPosition = position;
        scoreKFP = playerScoreKFP;
        money = playerMoney;
        invest = playerInvest;
        debt = playerDebt;
        salary = playerSalary;
        incomeTurn = playerIncomeTurn;
        expenseTurn = playerExpenseTurn;
        investments = playerInvestments;
        expenses = playerExpenses;
    }

    // TODO: Método para cambiar datos financieros

    public void ChangeMoney(int amount)
    {
        money += amount;
        playerHUD.Money.text = money.ToString("C0", chileanCulture);
    }

    public void ChangeKFP(int score)
    {
        scoreKFP += score;
        playerHUD.Kpf.text = scoreKFP.ToString();
    }

    public void ChangeDebt(int amount)
    {
        debt += amount;
        playerHUD.Debt.text = debt.ToString("C0", chileanCulture);
    }

    public void ChangeInvest(int amount)
    {
        invest += amount;
        playerHUD.Invest.text = invest.ToString("C0", chileanCulture);
    }

    public void ChangeIncome(int amount)
    {
        incomeTurn += amount;
        playerHUD.Income.text = incomeTurn.ToString("C0", chileanCulture);
    }

    public void ChangeExpense(int amount)
    {
        expenseTurn += amount;
        playerHUD.Expense.text = expenseTurn.ToString("C0", chileanCulture);
    }

    public void ChangeSalary(int newSalary)
    {
        incomeTurn += newSalary - salary;
        salary = newSalary;
        playerHUD.Income.text = incomeTurn.ToString("C0", chileanCulture);
    }

    // TODO: Métodos para crear gastos e inversiones
    public void CreateInvestment(PlayerInvestment investment)
    {
        if (money < investment.Capital)
            return;
        investments.Add(investment);
        ChangeMoney(-investment.Capital);
        ChangeInvest(investment.Capital);
        ChangeIncome(investment.Dividend);
    }

    public void CreateExpense(PlayerExpense expense, bool isRecurrent)
    {
        if (isRecurrent)
        {
            expenses.Add(expense);
            ChangeDebt(expense.Amount * expense.Turns);
            ChangeExpense(expense.Amount);
        }
        else
        {
            if (money >= expense.Amount)
                ChangeMoney(-expense.Amount);
            else
            {
                int interestMount = (int)(expense.Amount * 0.1f);
                expense.Amount += interestMount;
                expense.Turns++;
                expenses.Add(expense);
                ChangeDebt(interestMount * expense.Turns);
                ChangeExpense(interestMount);
            }
        }
    }

    // TODO: Método para procesar datos financieros

    public void ProcessIncome()
    {
        ChangeMoney(incomeTurn);
    }

    public void ProcessInvestments()
    {
        if (investments.Count == 0)
            return;
        List<PlayerInvestment> toRemove = new List<PlayerInvestment>();
        foreach (var investment in investments)
        {
            if (investment.Turns == 0)
            {
                ChangeIncome(-investment.Dividend);
                ChangeMoney(investment.Capital);
                ChangeInvest(-investment.Capital);
                toRemove.Add(investment);
            }
            else
            {
                int beforeDividend = investment.Dividend;
                int beforeCapital = investment.Capital;
                investment.UpdateInvestment();
                investment.Turns--;
                ChangeInvest(investment.Capital - beforeCapital);
                ChangeIncome(investment.Dividend - beforeDividend);
            }
        }

        foreach (var item in toRemove)
        {
            investments.Remove(item);
        }
    }

    public void ProcessRecurrentExpenses()
    {
        if (expenses.Count == 0)
            return;

        List<PlayerExpense> toRemove = new List<PlayerExpense>();

        foreach (var expense in expenses)
        {
            if (money >= expense.Amount)
            {
                ChangeMoney(-expense.Amount);
                ChangeDebt(-expense.Amount);
                expense.Turns--;
                if (expense.Turns == 0)
                {
                    ChangeExpense(-expense.Amount);
                    toRemove.Add(expense);
                }
            }
            else
            {
                int interestMount = (int)(expense.Amount * 0.05f);
                expense.Amount += interestMount;
                expense.Turns++;
                ChangeDebt(interestMount * expense.Turns);
                ChangeExpense(interestMount);
            }
        }

        foreach (var item in toRemove)
        {
            expenses.Remove(item);
        }
    }

}

