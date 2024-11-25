using UnityEngine;
using System.Globalization;

[CreateAssetMenu(fileName = "EventCard", menuName = "Cards/EventCard")]
public class EventCard : CardBase
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


    public override void ApplyEffect(PlayerController player, int capital = 0)
    {
        foreach (PlayerController p in GameManager.Instance.Players)
        {
            if (amount > 0)
                p.ChangeMoney(amount);
            else
            {
                PlayerExpense expense = new PlayerExpense(1, amount);
                p.CreateExpense(expense, false);
            }
        }
    }

    public override void RemoveFromGameData()
    {
        GameManager.Instance.GameData.eventCards.Remove(this);
    }
}
