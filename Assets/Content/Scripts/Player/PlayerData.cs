using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [SerializeField] private int index;                             // Índice del jugador
    [SerializeField] private string playerName;                     // Nombre del jugador
    [SerializeField] private int currentPosition;                   // Posición actual del jugador

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
    [SerializeField] private int characterID;                       // Modelo del jugador

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
    public int CharacterID { get => characterID; set => characterID = value; }

    public void NewPlayer(int playerIndex, string name, int model)
    {
        index = playerIndex;
        playerName = name;
        characterID = model;

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
}

