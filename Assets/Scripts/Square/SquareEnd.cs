using UnityEngine;

public class SquareEnd : Square
{
    private bool squareSleeping;

    public override void ActiveSquare(PlayerController player)
    {
        squareSleeping = false;
        player.SetPlayerState(GameState.Finalizado); // Cambiar el estado del jugador a FINALIZADO
        squareSleeping = true;
        // Imprimir el nombre del jugador que ha finalizado
        Debug.Log("El jugador " + player.GetPlayerName() + " ha finalizado.");
    }

    public override bool IsSquareStopped()
    {
        return squareSleeping;
    }
}
