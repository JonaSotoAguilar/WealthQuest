using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public abstract class Square : MonoBehaviour
{
    public List<PlayerMovement> playersInSquare = new List<PlayerMovement>();
    public List<CardBase> selectedCards;

    public int PlayersCount => playersInSquare.Count;

    #region Methods Square

    public abstract void GetCards();

    public void SetupSquare(IPlayer player, PlayerCanvas canvas)
    {
        CardsPanel panel = canvas.CardsPanel;
        if (panel == null) return;
        panel.SetupCards(player, selectedCards);
    }

    #endregion

    #region Methods Players

    public void AddPlayer(PlayerMovement player)
    {
        if (playersInSquare.Contains(player)) return;

        playersInSquare.Add(player);
    }

    public void RemovePlayer(PlayerMovement player)
    {
        if (!playersInSquare.Contains(player)) return;
        
        playersInSquare.Remove(player);
    }

    public int GetPlayerIndex(PlayerMovement player) => playersInSquare.IndexOf(player);

    public void UpdateSquarePositions(IPlayer player)
    {
        int squarePosition = player.Position;
        RemovePlayer(player.Movement);
        player.Movement.CenterPosition(squarePosition);

        int totalPlayers = playersInSquare.Count;
        for (int i = 0; i < totalPlayers; i++)
            playersInSquare[i].CornerPosition(squarePosition);
    }

    #endregion

}
