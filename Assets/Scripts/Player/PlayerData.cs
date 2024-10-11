using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [SerializeField] private int index;                             // Índice del jugador
    private string playerName;                                      // Nombre del jugador
    private int currentPosition;                                    // Posición actual del jugador
    private GameState state;                                        // Estado del jugador      

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

    public string PlayerName { get => playerName; set => playerName = value; }
    public int Index { get => index; set => index = value; }
    public int CurrentPosition { get => currentPosition; set => currentPosition = value; }
    public GameState State { get => state; set => state = value; }
    public int ScoreKFP { get => scoreKFP; set => scoreKFP = value; }
    public int Money { get => money; set => money = value; }
    public int IncomeTurn { get => incomeTurn; set => incomeTurn = value; }
    public int ExpenseTurn { get => debt; set => debt = value; }
    public List<PlayerInvestment> Investments { get => investments; }
    public List<PlayerExpense> Expenses { get => expenses; }

    // Inicializar datos del jugador
    public void InitializePlayer(int playerIndex, string name, int position, GameState playerState,
                                 int playerScoreKFP, int playerMoney, int playerInvest, int playerDebt,
                                 int playerSalary, int playerIncomeTurn, int playerExpenseTurn,
                                 List<PlayerInvestment> playerInvestments, List<PlayerExpense> playerExpenses)
    {
        index = playerIndex;
        playerName = name;
        currentPosition = position;
        state = playerState;
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

    public void ProcessIncome()
    {
        money += incomeTurn;
    }

    public void ChangeSalary(int newSalary)
    {
        incomeTurn += newSalary - salary;
        salary = newSalary;
    }

    // Método para aplicar un gasto
    public void CreateExpense(PlayerExpense expense, bool isRecurrent)
    {
        if (isRecurrent)
        {
            // Añadir el gasto recurrente a la lista de gastos
            expenses.Add(expense);
            debt += expense.Amount * expense.Turns;
            expenseTurn += expense.Amount;
            Debug.Log($"{PlayerName} ha adquirido un gasto recurrente de {expense.Amount} durante {expense.Turns} turnos");
        }
        else // Gasto fijo
        {
            // Restar el capital directamente si es un gasto único (fijo)
            if (money >= expense.Amount)
            {
                money -= expense.Amount;
                Debug.Log($"{PlayerName} ha pagado un gasto único de {expense.Amount}. Dinero restante: {money}.");
            }
            else // Se crea gasto pendiente con interes por no pagar a tiempo
            {
                int interestMount = (int)(expense.Amount * 0.1f);
                expense.Amount += interestMount;
                expense.Turns++;
                expenses.Add(expense);
                debt += expense.Amount * expense.Turns;
                expenseTurn += expense.Amount;
                Debug.LogError($"{PlayerName} no tiene suficiente dinero para pagar el gasto, se crea deuda de {expense.Amount}.");
            }
        }
    }

    public void CreateInvestment(PlayerInvestment investment)
    {
        if (money < investment.Capital)
            return;
        // Añadir la inversión a la lista de inversiones
        investments.Add(investment);

        // Muevo el dinero a inversion
        money -= investment.Capital;
        invest += investment.Capital;

        // Añadir el dividendo de la inversión a los ingresos por turno
        incomeTurn += investment.Dividend;
    }

    // Procesar gastos recurrentes al final de cada turno
    public void ProcessRecurrentExpenses()
    {
        foreach (var expense in expenses)
        {
            // Restar el capital por cada turno
            if (money >= expense.Amount)
            {
                money -= expense.Amount;
                debt -= expense.Amount;
                expense.Turns--;

                Debug.Log($"{PlayerName} ha pagado {expense.Amount} por un gasto recurrente. Le quedan {expense.Turns} turnos de pago.");

                // Si se han completado todos los turnos, remover el gasto/deuda
                if (expense.Turns == 0)
                {
                    expenseTurn -= expense.Amount;
                    expenses.Remove(expense);
                }
            }
            else // Se agrega un turno adicional e interés por no pagar el gasto a tiempo
            {
                int interestMount = (int)(expense.Amount * 0.05f);
                expense.Amount += interestMount;        // Añadir interés al capital como penalización
                expense.Turns++;                         // Añadir un turno adicional
                debt += interestMount * expense.Turns;   // Añadir monto al total de gastos por turno
                expenseTurn += interestMount;            // Añadir monto al total de gastos por turno
                Debug.LogError($"{PlayerName} no tiene suficiente dinero para pagar el gasto recurrente de {expense.Amount}. Dinero disponible: {money}.");
            }
        }
    }

    public void ProcessInvestments()
    {
        foreach (var investment in investments)
        {
            if (investment.Turns == 0)
            {
                incomeTurn -= investment.Dividend;
                money += investment.Capital;
                invest -= investment.Capital;
                investments.Remove(investment);
            } 
            else 
            {
                int beforeDividend = investment.Dividend;
                int beforeCapital = investment.Capital;

                investment.UpdateInvestment();
                investment.Turns--;

                invest += investment.Capital - beforeCapital;
                incomeTurn += investment.Dividend - beforeDividend;
            }
        }
    }
}

