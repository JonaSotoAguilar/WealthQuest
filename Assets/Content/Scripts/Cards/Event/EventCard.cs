using UnityEngine;

[CreateAssetMenu(fileName = "EventCard", menuName = "Cards/EventCard")]
public class EventCard : CardBase
{
    [TextArea(minLines: 2, maxLines: 4), Tooltip("Descripcion de carta.")] public string description;
    [Tooltip("Monto a ganar/perder")] public int amount;

    public override string GetFormattedText(int playerKFP)
    {
        if (amount > 0)
        {
            return $"{description}. Todos reciben: ${amount}";
        }
        else
        {
            return $"{description}. Todos pierden: ${-amount}";
        }
    }


    public override void ApplyEffect(PlayerData player, int capital = 0)
    {
        // A todos los jugadores de la List de jugadores, se les suma o resta el monto de la carta
        foreach (PlayerData p in GameData.Instance.Players)
        {
            p.ChangeMoney(amount);
        }
    }

    public override void RemoveFromGameData()
    {
        GameData.Instance.EventCards.Remove(this);
    }
}
