using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Square : MonoBehaviour
{
    [SerializeField] protected GameData data;
    public List<CardBase> selectedCards;
    public List<PlayerNetMovement> players = new List<PlayerNetMovement>();

    public int PlayersCount => players.Count;

    #region Methods Square

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
