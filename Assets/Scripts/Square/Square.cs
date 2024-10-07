using UnityEngine;

public abstract class Square : MonoBehaviour
{
    public abstract void ActiveSquare(PlayerData player, CanvasPlayer canvasPlayer);

    public abstract bool SquareSleeping();
}
