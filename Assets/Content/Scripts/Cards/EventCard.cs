using UnityEngine;
using System.Globalization;

[CreateAssetMenu(fileName = "EventCard", menuName = "Cards/EventCard")]
public class EventCard : CardBase
{
    [TextArea(minLines: 2, maxLines: 4), Tooltip("Descripcion de carta.")] public string description;
    [Tooltip("Monto a ganar/perder")] public int amount;
    private CultureInfo chileanCulture = new CultureInfo("es-CL");
    private static IGameManager game;

    private void GameInstance()
    {
        if (game != null) return;

        game = GameOnline.Instance != null ?
                      GameOnline.Instance :
                      GameManager.Instance;
    }

    public override string GetFormattedText(int points)
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

    public override void ApplyEffect(IPlayer player, int capital = 0)
    {
        GameInstance();
        foreach (IPlayer p in game.Players)
        {
            if (amount > 0)
                p.AddMoney(amount);
            else
            {
                Expense newExpense = new Expense(1, amount);
                p.AddExpense(newExpense, false);
            }
        }
    }

    public override void RemoveFromGameData()
    {
        GameManager.Instance.GameData.eventCards.Remove(this);
    }
}
