using UnityEngine;
using System.Collections;

public class SquareEnd : Square
{
    // Implementación de ActiveSquare como una corrutina
    public override IEnumerator ActiveSquare(PlayerData player, PlayerCanvas canvasPlayer)
    {
        // Cambiar el estado del jugador a FINALIZADO
        player.State = GameState.Finalizado;
        yield return null; // Esperar un frame
    }
}
