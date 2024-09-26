using UnityEngine;

public class SquareEnd : Square
{
    public override void ActivarCasilla(Player player)
    {
        // Cambiar el estado del jugador a FINALIZADO
        player.playerState = GameState.Finalizado;
        Debug.Log(player.playerName + " ha alcanzado la casilla final y está FINALIZADO.");

        // Actualizar el HUD para reflejar los cambios
        HUDManager.instance.ActualizarHUD(player);
    }
}
