using UnityEngine;
using System.Globalization;

[CreateAssetMenu(fileName = "IncomeCard", menuName = "Cards/IncomeCard")]
public class IncomeCard : Card
{
    [Tooltip("Afecta al salario")] public bool affectIncome;
    [Tooltip("affectSalary = true: Aplica salaryChange al salario")] public float incomeChange;
    [Tooltip("affectSalary = false: Agrega Income al dinero")] public int income;

    private CultureInfo chileanCulture = new CultureInfo("es-CL");

    public override SquareType GetCardType()
    {
        return SquareType.Income;
    }

    public override string GetFormattedText(int playerKFP)
    {
        if (affectIncome)
            return $"Tu salario aumenta un <color=green>{incomeChange * 100}%</color>.";
        else
            return $"Recibes <color=green>{income.ToString("C0", chileanCulture)}</color>.";
    }

    public override void ApplyEffect(int capital = 0, bool isLocalGame = true)
    {
        if (isLocalGame)
        {
            PlayerLocalData player = GameLocalManager.CurrentPlayer.Data;
            if (affectIncome)
            {
                int newSalary = (int)(player.Salary * (1 + incomeChange));
                player.NewSalary(newSalary);
            }
            else
                player.AddMoney(income);
        }
        else
        {
            PlayerNetData player = GameNetManager.CurrentPlayer.Data;
            if (affectIncome)
            {
                int newSalary = (int)(player.Salary * (1 + incomeChange));
                player.NewSalary(newSalary);
            }
            else
                player.AddMoney(income);
        }
    }

    private void OnValidate()
    {
        if (affectIncome)
        {
            income = 0;
            incomeChange = Mathf.Clamp(incomeChange, 0.01f, 1);
        }
        else
        {
            incomeChange = 0;
            income = Mathf.Max(1, income);
        }
    }
}