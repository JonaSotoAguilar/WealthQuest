using UnityEngine;

public class SquareEnd : Square
{
    private bool squareSleeping;

    public override void ActiveSquare(Player player)
    {
        squareSleeping = false;
        player.playerState = GameState.Finalizado; // Cambiar el estado del jugador a FINALIZADO
        squareSleeping = true;
    }

    public override bool IsSquareStopped(){
        return squareSleeping;
    }
}
