using UnityEngine;
using System.Globalization;
using Mirror;

[CreateAssetMenu(fileName = "EventCard", menuName = "Cards/EventCard")]
public class EventCard : Card
{
    [TextArea(minLines: 2, maxLines: 4), Tooltip("Descripcion de carta.")] public string description;
    [Tooltip("Monto a ganar/perder")] public int amount;
    private CultureInfo chileanCulture = new CultureInfo("es-CL");

    public override string GetFormattedText(int playerKFP)
    {
        if (amount > 0)
        {
            return $"{description}. Todos ganan: <color=green>{amount.ToString("C0", chileanCulture)}</color>.";
        }
        else
        {
            return $"{description}. Todos pagan: <color=red>{amount.ToString("C0", chileanCulture)}</color>.";
        }
    }

    public override void ApplyEffect(int capital = 0, bool isLocalGame = true)
    {
        Debug.Log("Amount: " + amount);
        if (isLocalGame)
        {
            foreach (PlayerLocalManager player in GameLocalManager.Players)
            {
                if (amount > 0)
                    player.Data.AddMoney(amount);
                else
                {
                    Expense expense = new Expense(1, amount);
                    player.Data.NewExpense(expense, false);
                }
            }
        }
        else
        {
            foreach (PlayerNetManager player in GameNetManager.Players)
            {
                if (amount > 0)
                    player.Data.AddMoney(amount);
                else
                {
                    Expense expense = new Expense(1, -amount);
                    player.Data.NewExpense(expense, false);
                }
            }
        }
    }

}