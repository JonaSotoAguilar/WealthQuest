using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "IncomeCard", menuName = "Cards/IncomeCard")]
public class IncomeCard : CardBase
{
    [TextArea(minLines: 2, maxLines: 4), Tooltip("Descripcion de carta.")] public string description;
    [Tooltip("Afecta al salario")] public bool affectSalary;
    [Tooltip("affectSalary = true: Aplica salaryChange al salario")] public float salaryChange;
    [Tooltip("affectSalary = false: Agrega Income al dinero")] public int income;

    public override string GetFormattedText(int playerKFP)
    {
        if (affectSalary)
            return $"{description}. Tu salario aumenta un {salaryChange * 100}%";
        else
            return $"{description}. Recibes: ${income}";
    }

    public override void ApplyEffect(PlayerData player, int capital = 0)
    {
        if (affectSalary)
        {
            int newSalary = (int)(player.Salary * (1 + salaryChange));
            player.ChangeSalary(newSalary);
        }
        else
            player.ChangeMoney(income);
    }

    public override void RemoveFromGameData()
    {
        GameData.Instance.IncomeCards.Remove(this);
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
