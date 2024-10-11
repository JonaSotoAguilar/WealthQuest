using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// FIXME: Agregar cartas de inversión no habilitadas por fondo insuficiente
public class SquareInvestment : Square
{
    private PlayerCanvas canvasPlayer;

    // Implementación de ActiveSquare como una corrutina
    public override IEnumerator ActiveSquare(PlayerData player, PlayerCanvas canvas)
    {
        canvasPlayer = canvas;
        CardsPanel panel = canvasPlayer.CardsPanel;

        if (panel != null)
        {
            // Obtener las InvestmentCards desde GameData y convertirlas a CardBase
            List<CardBase> selectedCards = GameData.Instance.GetRandomInvestmentCards(2).Cast<CardBase>().ToList();

            // Configurar el panel con las tarjetas para el jugador
            panel.SetupCards(player, selectedCards);

            // Variable para indicar si la tarjeta fue seleccionada
            bool cardSelected = false;

            // Subscribirse al evento cuando se selecciona una opción
            System.Action onCardSelected = () => cardSelected = true;
            panel.OnCardSelected += onCardSelected;

            // Esperar hasta que una tarjeta sea seleccionada
            yield return new WaitUntil(() => cardSelected);

            // Desuscribirse del evento para evitar fugas de memoria
            panel.OnCardSelected -= onCardSelected;
        }
        else
        {
            Debug.LogError("No se encontró el CardsPanel en el canvas.");
        }
    }

}