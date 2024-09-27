using UnityEngine;

public abstract class Square : MonoBehaviour
{
    public abstract void ActiveSquare(PlayerController player);

    public abstract bool IsSquareStopped();
}
