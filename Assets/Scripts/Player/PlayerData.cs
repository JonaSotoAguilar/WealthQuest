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
    public int ExpenseTurn { get => expenseTurn; set => expenseTurn = value; }
    public List<PlayerInvestment> Investments { get => investments; }
    public List<PlayerExpense> Expenses { get => expenses; }

    // Inicializar datos del jugador
    public void InitializePlayer(int playerIndex, string name, int position, GameState playerState, int playerScoreKFP,
                                int playerMoney, int playerIncomesTurn, int playerExpenseTurn, List<PlayerInvestment> playerInvestments, List<PlayerExpense> playerExpenses)
    {
        index = playerIndex;
        playerName = name;
        currentPosition = position;
        state = playerState;
        scoreKFP = playerScoreKFP;
        money = playerMoney;
        incomeTurn = playerIncomesTurn;
        expenseTurn = playerExpenseTurn;
        investments = playerInvestments;
        expenses = playerExpenses;
    }

    // Método para aplicar un gasto
    public void ApplyExpense(PlayerExpense expense, bool isRecurrent)
    {
        if (isRecurrent) // Gasto recurrente
        {
            // Añadir el gasto recurrente a la lista de gastos
            expenseTurn += expense.Capital;
            expenses.Add(expense);
            Debug.Log($"{PlayerName} ha adquirido un gasto recurrente de {expense.Capital} durante {expense.Turns} turnos");
        }
        else // Gasto fijo
        {
            // Restar el capital directamente si es un gasto único (fijo)
            if (money >= expense.Capital)
            {
                money -= expense.Capital;
                Debug.Log($"{PlayerName} ha pagado un gasto único de {expense.Capital}. Dinero restante: {money}.");
            }
            else // Se crea gasto de un turno adicional por no tener suficiente dinero 
            {
                int interest = (int)(expense.Capital * 0.1f); // Calcular el 10% de interés
                expense.Capital += interest; // Añadir el interés al capital como penalización
                expense.Turns++; // Añadir un turno adicional
                expenses.Add(expense);
                expenseTurn += expense.Capital; // Añadir monto al total de gastos por turno
                Debug.LogError($"{PlayerName} no tiene suficiente dinero para pagar el gasto, se crea deuda de {expense.Capital}.");
            }
        }
    }

    // Procesar gastos recurrentes al final de cada turno
    public void ProcessRecurrentExpenses()
    {
        List<PlayerExpense> expensesToRemove = new List<PlayerExpense>();

        foreach (var expense in expenses)
        {
            // Restar el capital por cada turno
            if (money >= expense.Capital)
            {
                money -= expense.Capital;
                expense.Turns--;

                Debug.Log($"{PlayerName} ha pagado {expense.Capital} por un gasto recurrente. Le quedan {expense.Turns} turnos de pago.");

                // Si se han completado todos los turnos, marcar para remover el gasto/deuda
                if (expense.Turns == 0)
                {
                    expensesToRemove.Add(expense);
                    Debug.Log($"El gasto recurrente de {expense.Capital} de {PlayerName} ha finalizado.");
                }
            }
            else // Se agrega un turno adicional e interés por no tener suficiente dinero
            {
                int interest = (int)(expense.Capital * 0.05f);
                expense.Capital += interest; // Añadir el interés al capital como penalización
                expense.Turns++; // Añadir un turno adicional
                expenseTurn += interest; // Añadir el interés al total de gastos por turno
                Debug.LogError($"{PlayerName} no tiene suficiente dinero para pagar el gasto recurrente de {expense.Capital}. Dinero disponible: {money}.");
            }
        }

        // Remover los gastos que han sido completados
        foreach (var expense in expensesToRemove)
        {
            expenseTurn -= expense.Capital;
            expenses.Remove(expense);
        }
    }
}

