using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Square : MonoBehaviour
{
    [SerializeField] protected GameData data;
    public List<PlayerMovement> players = new List<PlayerMovement>();

    public int PlayersCount => players.Count;

    #region Methods Square

    public abstract List<Card> GetCards();

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
