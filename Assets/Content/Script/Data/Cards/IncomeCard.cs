using UnityEngine;
using System.Globalization;
using Mirror.Examples.Basic;
using Mirror;

[CreateAssetMenu(fileName = "IncomeCard", menuName = "Cards/IncomeCard")]
public class IncomeCard : Card
{
    [TextArea(minLines: 2, maxLines: 4), Tooltip("Descripcion de carta.")] public string description;
    [Tooltip("Afecta al salario")] public bool affectSalary;
    [Tooltip("affectSalary = true: Aplica salaryChange al salario")] public float salaryChange;
    [Tooltip("affectSalary = false: Agrega Income al dinero")] public int income;
    private CultureInfo chileanCulture = new CultureInfo("es-CL");

    public override string GetFormattedText(int playerKFP)
    {
        if (affectSalary)
            return $"{description}. Tu salario aumenta un <color=green>{salaryChange * 100}%</color>.";
        else
            return $"{description}. Recibes <color=green>{income.ToString("C0", chileanCulture)}</color>.";
    }

    public override void ApplyEffect(int capital = 0, bool isLocalGame = true)
    {
        if (isLocalGame)
        {
            PlayerLocalData player = GameLocalManager.CurrentPlayer.Data;
            if (affectSalary)
            {
                int newSalary = (int)(player.Salary * (1 + salaryChange));
                player.NewSalary(newSalary);
            }
            else
                player.AddMoney(income);
        }
        else
        {
            PlayerNetData player = GameNetManager.CurrentPlayer.Data;
            if (affectSalary)
            {
                int newSalary = (int)(player.Salary * (1 + salaryChange));
                player.NewSalary(newSalary);
            }
            else
                player.AddMoney(income);
        }
    }

    private void OnValidate()
    {
        if (affectSalary)
        {
            income = 0;
            salaryChange = Mathf.Clamp(salaryChange, 0.01f, 1);
        }
        else
        {
            salaryChange = 0;
            income = Mathf.Max(1, income);
        }
    }
}

