using UnityEngine;

public class SquareMoney : Square
{
    public int cantidadDinero = 50; // Cantidad de dinero que otorga la casilla

    public override void ActivarCasilla(Player player)
    {
        player.ModifyMoney(cantidadDinero);
        HUDManager.instance.ActualizarHUD(player);
    }
}
