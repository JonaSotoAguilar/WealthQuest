using UnityEngine;

public class DiceRoller : MonoBehaviour
{
    public int LanzarDado()
    {
        // Genera un número aleatorio entre 1 y 6
        int resultadoDado = Random.Range(1, 7);

        // Muestra el resultado en la consola (puedes mostrarlo en UI si lo deseas)
        Debug.Log("Resultado del dado: " + resultadoDado);

        // Devuelve el resultado del dado
        return resultadoDado;
    }
}
