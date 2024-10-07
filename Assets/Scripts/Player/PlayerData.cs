using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [SerializeField] private int index;
    private string playerName;
    private int currentPosition;
    private GameState state;

    [Header("Finances")]
    [SerializeField] private int scoreKFP; // financial knowledge points: puntos de conocimiento financiero
    [SerializeField] private int money;
    [SerializeField] private int incomeTurn;
    [SerializeField] private int expenseTurn;
    [SerializeField] private List<PlayerInvestment> investments;
    [SerializeField] private List<PlayerExpense> expenses;

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
        // FIXME: Puede ser fijo y recurrente al mismo tiempo
        if (isRecurrent)
        {
            // Añadir el gasto recurrente a la lista de gastos
            expenses.Add(expense);
            Debug.Log($"{PlayerName} ha adquirido un gasto recurrente de {expense.Capital} durante {expense.Turns} turnos con un interés de {expense.Interest}%.");
        }
        else
        {
            // Restar el capital directamente si es un gasto único (fijo)
            if (money >= expense.Capital)
            {
                money -= expense.Capital;
                Debug.Log($"{PlayerName} ha pagado un gasto único de {expense.Capital}. Dinero restante: {money}.");
            }
            else
            {
                // FIXME: Implementar un sistema de deudas
                Debug.LogError($"{PlayerName} no tiene suficiente dinero para pagar el gasto de {expense.Capital}. Dinero disponible: {money}.");
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

                // Si se han completado todos los turnos, marcar para remover el gasto
                if (expense.Turns <= 0)
                {
                    expensesToRemove.Add(expense);
                    Debug.Log($"El gasto recurrente de {expense.Capital} de {PlayerName} ha finalizado.");
                }
            }
            else
            {
                Debug.LogError($"{PlayerName} no tiene suficiente dinero para pagar el gasto recurrente de {expense.Capital}. Dinero disponible: {money}.");
            }
        }

        // Remover los gastos que han sido completados
        foreach (var expense in expensesToRemove)
        {
            expenses.Remove(expense);
        }
    }
}

