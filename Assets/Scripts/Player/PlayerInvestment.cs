using UnityEngine;

[System.Serializable]
public class PlayerInvestment
{
    [SerializeField] private int turns;
    [SerializeField] private int capital;
    [SerializeField] private float interest;
    [SerializeField] private float dividend;

    public int Turns { get => turns; set => turns = value; }
    public int Capital { get => capital; set => capital = value; }
    public float Interest { get => interest; set => interest = value; }
    public float Dividend { get => dividend; set => dividend = value; }

    public PlayerInvestment(int turns, int capital, float interest = 0, float dividend = 0)
    {
        Turns = turns;
        Capital = capital;
        Interest = interest;
        Dividend = dividend;
    }

}