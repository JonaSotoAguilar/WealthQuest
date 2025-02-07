using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Square : MonoBehaviour
{
    [SerializeField] private GameData data;
    [SerializeField] private SquareType type;
    private List<PlayerMovement> players = new List<PlayerMovement>();

    #region Getters

    public List<PlayerMovement> Players => players;
    public int PlayersCount => players.Count;

    #endregion

    #region Methods Square

    public List<Card> GetCards()
    {
        switch (type)
        {
            case SquareType.Event:
                return data.GetRandomEventCards(2).Cast<Card>().ToList();
            case SquareType.Expense:
                return data.GetRandomExpenseCards(2).Cast<Card>().ToList();
            case SquareType.Income:
                return data.GetRandomIncomeCards(2).Cast<Card>().ToList();
            case SquareType.Investment:
                return data.GetRandomInvestmentCards(2).Cast<Card>().ToList();
            default:
                return new List<Card>();
        }
    }

    #endregion

    #region Methods Players

    public int GetPlayerIndex(PlayerMovement player) => players.IndexOf(player);

    public void AddPlayer(PlayerMovement player, int position)
    {
        if (players.Contains(player)) return;

        players.Add(player);
        UpdateCornerPositions(position);
    }

    public void RemovePlayer(PlayerMovement player, int position)
    {
        if (!players.Contains(player)) return;

        players.Remove(player);
        UpdateCornerPositions(position);
    }

    public void UpdateCornerPositions(int position)
    {
        for (int i = 0; i < players.Count; i++)
            players[i].CornerPlayer(position);
    }

    #endregion

}
