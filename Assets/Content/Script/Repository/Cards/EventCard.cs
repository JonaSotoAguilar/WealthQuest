using UnityEngine;
using System.Globalization;
using Mirror;

[CreateAssetMenu(fileName = "EventCard", menuName = "Cards/EventCard")]
public class EventCard : Card
{
    [Tooltip("Monto a ganar/perder")] public int amount;

    private CultureInfo chileanCulture = new CultureInfo("es-CL");

    public override SquareType GetCardType()
    {
        return SquareType.Event;
    }

    public override string GetFormattedText(int playerKFP)
    {
        if (amount > 0)
        {
            return $"Todos ganan: <color=green>{amount.ToString("C0", chileanCulture)}</color>.";
        }
        else
        {
            int absAmount = -amount;
            return $"Todos pagan: <color=red>{absAmount.ToString("C0", chileanCulture)}</color>.";
        }
    }

    public override void ApplyEffect(int capital = 0, bool isLocalGame = true)
    {
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
                    Expense expense = new Expense(1, amount);
                    player.Data.NewExpense(expense, false);
                }
            }
        }
    }

    private void OnValidate()
    {
        if (amount == 0)
        {
            amount = 1;
        }
    }

}