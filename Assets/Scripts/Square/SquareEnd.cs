using UnityEngine;

public class SquareEnd : Square
{
    private bool squareSleeping;
    
    public override bool SquareSleeping() => squareSleeping;

    public override void ActiveSquare(PlayerData player, CanvasPlayer canvasPlayer) => ActiveSquare(player);

    public override void ActiveSquare(PlayerData player)
    {
        squareSleeping = false;
        player.State = GameState.Finalizado; // Cambiar el estado del jugador a FINALIZADO
        squareSleeping = true;
    }
}

