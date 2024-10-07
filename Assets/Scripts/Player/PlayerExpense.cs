using UnityEngine;

[System.Serializable]
public class PlayerExpense
{
    [SerializeField] private int turns;
    [SerializeField] private int capital;

    public int Turns { get => turns; set => turns = value; }
    public int Capital { get => capital; set => capital = value; }

    public PlayerExpense(int turns, int capital)
    {
        Turns = turns;
        Capital = capital;
    }

}