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

[System.Serializable]
public class PlayerInvestment
{
    [SerializeField] private string nameInvestment;
    [SerializeField] private int turns;
    [SerializeField] private int capital;
    [SerializeField] private int dividend;
    [SerializeField] private List<float> pctChanges;
    [SerializeField] private List<float> pctDividend;

    public string NameInvestment { get => nameInvestment; set => nameInvestment = value; }
    public int Turns { get => turns; set => turns = value; }
    public int Capital { get => capital; set => capital = value; }
    public int Dividend { get => dividend; set => dividend = value; }
    public List<float> PctChanges { get => pctChanges; set => pctChanges = value; }
    public List<float> PctDividend { get => pctDividend; set => pctDividend = value; }

    public PlayerInvestment(string name, int turnsInvest, int capitalInvest, int dividendInvest,
                            List<float> pctChangesInvest, List<float> pctDividendInvest)
    {
        nameInvestment = name;
        turns = turnsInvest;
        capital = capitalInvest;
        dividend = dividendInvest;
        pctChanges = pctChangesInvest;
        pctDividend = pctDividendInvest;
    }

    public void UpdateInvestment()
    {
        capital += (int)(capital * pctChanges[0]);
        dividend = (int)(capital * pctDividend[0]);
        pctChanges.RemoveAt(0);
        pctDividend.RemoveAt(0);
    }
}

[System.Serializable]
public class PlayerExpense
{
    [SerializeField] private int turns;
    [SerializeField] private int amount;

    public int Turns { get => turns; set => turns = value; }
    public int Amount { get => amount; set => amount = value; }

    public PlayerExpense(int playerTurns, int amountTurn)
    {
        turns = playerTurns;
        amount = amountTurn;
    }
}
