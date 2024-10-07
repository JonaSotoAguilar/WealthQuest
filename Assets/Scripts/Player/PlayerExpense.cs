using UnityEngine;

public class PlayerExpense : MonoBehaviour 
{
    [SerializeField] private int turns;
    [SerializeField] private int capital;
    [SerializeField] private int interest;

    public int Turns { get => turns; set => turns = value; }
    public int Capital { get => capital; set => capital = value; }
    public int Interest { get => interest; set => interest = value; }

}