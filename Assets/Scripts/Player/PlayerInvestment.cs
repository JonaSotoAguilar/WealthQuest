using UnityEngine;

public class PlayerInvestment : MonoBehaviour
{
    [SerializeField] private int turns;
    [SerializeField] private int capital;
    [SerializeField] private int interest;
    [SerializeField] private int dividend;

    public int Turns { get => turns; set => turns = value; }
    public int Capital { get => capital; set => capital = value; }
    public int Interest { get => interest; set => interest = value; }
    public int Dividend { get => dividend; set => dividend = value; }

    
}