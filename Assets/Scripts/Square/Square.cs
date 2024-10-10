using UnityEngine;
using System.Collections;

public abstract class Square : MonoBehaviour
{
    // Método abstracto para activar la casilla y devolver una corrutina
    public abstract IEnumerator ActiveSquare(PlayerData player, PlayerCanvas canvasPlayer);
}
