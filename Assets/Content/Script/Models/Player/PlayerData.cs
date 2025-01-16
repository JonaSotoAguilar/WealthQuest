using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [SerializeField] private string uid;
    [SerializeField] private string nickName;
    [SerializeField] private int characterID;
    [SerializeField] private int finalScore;

    [SerializeField] private int position;
    [SerializeField] private int points;
    [SerializeField] private int level;

    [SerializeField] private int money;
    [SerializeField] private int salary;
    [SerializeField] private int invest;
    [SerializeField] private int debt;
    [SerializeField] private int income;
    [SerializeField] private int expense;

    [SerializeField] private List<Investment> investments = new List<Investment>();
    [SerializeField] private List<Expense> expenses = new List<Expense>();

    // Propiedades sin cambios
    public string UID { get => uid; set => uid = value; }
    public string Nickname { get => nickName; set => nickName = value; }
    public int CharacterID { get => characterID; set => characterID = value; }
    public int FinalScore { get => finalScore; set => finalScore = value; }
    public int Position { get => position; set => position = value; }
    public int Points { get => points; set => points = value; }
    public int Level { get => level; set => level = value; }
    public int Money { get => money; set => money = value; }
    public int Salary { get => salary; set => salary = value; }
    public int Invest { get => invest; set => invest = value; }
    public int Debt { get => debt; set => debt = value; }
    public List<Investment> Investments { get => investments; }
    public List<Expense> Expenses { get => expenses; }
    public int Income { get => income; set => income = value; }
    public int Expense { get => expense; set => expense = value; }

    public PlayerData() { }

    public PlayerData(string uid, string nickName, int characterID)
    {
        this.uid = uid;
        this.nickName = nickName;
        this.characterID = characterID;

        position = 0;
        points = 0;
        level = 1;

        money = 1000;
        salary = 1000;
        invest = 0;
        debt = 0;

        income = 0 + salary;
        expense = 0;
    }
}
