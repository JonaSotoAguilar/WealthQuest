using UnityEngine;

public class SquareMoney : Square
{
    [SerializeField]
    private int amountMoney = 50; // Cantidad de dinero que otorga la casilla
    private bool squareSleeping;

    public override void ActiveSquare(PlayerController player)
    {
        squareSleeping = false;
        player.ModifyMoney(amountMoney);
        squareSleeping = true;
    }


    public override bool IsSquareStopped()
    {
        return squareSleeping;
    }
}
