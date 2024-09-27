using UnityEngine;

public abstract class Square : MonoBehaviour
{
    public abstract void ActiveSquare(Player player);

    public abstract bool IsSquareStopped();
}
