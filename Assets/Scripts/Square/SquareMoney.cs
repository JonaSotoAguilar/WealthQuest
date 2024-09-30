using UnityEngine;

public class SquareMoney : Square
{
    [SerializeField]
    private int amountMoney = 50; // Cantidad de dinero que otorga la casilla
    private bool squareSleeping;
    public override bool SquareSleeping() => squareSleeping;

    public override void ActiveSquare(PlayerData player, CanvasPlayer canvasPlayer) => ActiveSquare(player);

    public override void ActiveSquare(PlayerData player)
    {
        squareSleeping = false;
        player.Money = amountMoney;
        squareSleeping = true;
    }
}
