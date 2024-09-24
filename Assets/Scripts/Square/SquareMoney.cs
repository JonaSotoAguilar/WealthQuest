using UnityEngine;

public class SquareMoney : Square
{
    public int cantidadDinero = 50; // Cantidad de dinero que otorga la casilla

    public override void ActivarCasilla(PlayerStats jugador)
    {
        jugador.ModificarDinero(cantidadDinero);
        //Debug.Log("El jugador ha recibido " + cantidadDinero + " de dinero.");
    }
}
