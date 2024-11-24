using UnityEngine;
using System.Collections.Generic;

public abstract class Square : MonoBehaviour
{
    [SerializeField] protected GameData data;
    public List<CardBase> selectedCards;
    public List<PlayerNetMovement> players = new List<PlayerNetMovement>();

    public int PlayersCount => players.Count;

    #region Methods Square

    public abstract List<CardBase> GetCards();

    public void SetupSquare(IPlayer player)
    {
        //Imprime cartas seleccionadas
        foreach (var card in selectedCards)
            Debug.Log(card.title);

        //FIXME: Revisar
        //UIOnline.Instance.SetupCards(player, selectedCards);
    }

    #endregion

    #region Methods Players
    public int GetPlayerIndex(PlayerNetMovement player) => players.IndexOf(player);

    public void AddPlayer(PlayerNetMovement player, int position)
    {
        if (players.Contains(player)) return;

        players.Add(player);
        UpdateCornerPositions(position);
    }

    public void RemovePlayer(PlayerNetMovement player, int position)
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
