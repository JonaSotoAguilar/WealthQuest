using UnityEngine;
using System.Collections.Generic;

public class SquareExpense : Square
{
    private bool squareSleeping;
    private CanvasPlayer canvasPlayer;

    public override bool SquareSleeping() => squareSleeping;

    public override void ActiveSquare(PlayerData player, CanvasPlayer canvas)
    {
        squareSleeping = false;
        canvasPlayer = canvas;

        // Obtener una referencia al CardsPanel desde el Canvas
        CardsPanel panel = canvasPlayer.CardsPanel;

        if (panel != null)
        {
            // Configurar el panel con las tarjetas de gasto para el jugador
            panel.SetupExpense(player);

            // Subscribirse al evento cuando el jugador selecciona una opción
            panel.OnCardSelected += HandleExpenseSelected;
        }
        else
        {
            Debug.LogError("No se encontró el CardsPanel en el canvas.");
        }
    }

    private void HandleExpenseSelected()
    {
        // Una vez que se ha seleccionado la opción, desconectar el evento
        canvasPlayer.CardsPanel.OnCardSelected -= HandleExpenseSelected;

        // Marcar la casilla como dormida
        squareSleeping = true;
    }
}
