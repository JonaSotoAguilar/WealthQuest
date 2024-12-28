using System.Collections.Generic;
using UnityEngine;
using Mirror;

[System.Serializable]
public class PlayerData
{
    private string uid;
    private string nickName;
    private int characterID;
    private int finalScore;

    private int position;
    private int points;
    private int level;

    private int money;
    private int salary;
    private int invest;
    private int debt;
    private List<Investment> investments;
    private List<Expense> expenses;

    private int income;
    private int expense;


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

        money = 0;
        salary = 0;
        invest = 0;
        debt = 0;
        investments = new List<Investment>();
        expenses = new List<Expense>();

        income = 0;
        expense = 0;
    }
}

