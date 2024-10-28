using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Square : MonoBehaviour
{
    public List<PlayerMovement> playersInSquare = new List<PlayerMovement>();
    public List<CardBase> selectedCards;

    public List<PlayerMovement> PlayersInSquare { get => playersInSquare; }
    public int PlayersCount => playersInSquare.Count;

    public abstract void GetCards(PlayerController player);

    public IEnumerator ActiveSquare(PlayerController player)
    {
        var canvasPlayer = player.PlayerCanvas;

        // Obtener una referencia al CardsPanel desde el Canvas
        CardsPanel panel = canvasPlayer.CardsPanel;

        if (panel != null)
        {
            GetCards(player);
            panel.SetupCards(player, selectedCards);
            bool cardSelected = false;

            System.Action onCardSelected = () => cardSelected = true;
            panel.OnCardSelected += onCardSelected;
            yield return new WaitUntil(() => cardSelected);

            panel.OnCardSelected -= onCardSelected;
        }
        else
        {
            Debug.LogError("No se encontró el CardsPanel en el canvas.");
        }
    }

    public void AddPlayer(PlayerMovement player)
    {
        if (!playersInSquare.Contains(player))
        {
            playersInSquare.Add(player);
        }
    }

    // Método para remover un jugador de la casilla
    public void RemovePlayer(PlayerMovement player)
    {
        if (playersInSquare.Contains(player))
        {
            playersInSquare.Remove(player);
        }
    }

    public int GetPlayerIndex(PlayerMovement player)
    {
        return playersInSquare.IndexOf(player);
    }

    public void UpdateSquare(PlayerController player)
    {
        int squarePosition = player.PlayerData.CurrentPosition;
        RemovePlayer(player.PlayerMovement);
        player.PlayerMovement.CenterPosition(squarePosition);

        int totalPlayers = playersInSquare.Count;
        for (int i = 0; i < totalPlayers; i++)
        {
            playersInSquare[i].CornerPosition(squarePosition);
        }
    }

}
