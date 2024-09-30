using UnityEngine;

public class PlayerCorner
{
    // Definir el array corners como estático
    public static readonly Vector3[] corners = new Vector3[]
    {
        new Vector3(-0.5f, 0f, 0.5f),
        new Vector3(0.5f, 0f, 0.5f),
        new Vector3(-0.5f, 0f, -0.5f),
        new Vector3(0.5f, 0f, -0.5f)
    };

    // Método estático para obtener una esquina según el índice
    public static Vector3 GetCorner(int index)
    {
        if (index >= 0 && index < corners.Length)
        {
            return corners[index];
        }
        else
        {
            Debug.LogError("Índice fuera de rango.");
            return Vector3.zero; // Devuelve un valor por defecto si el índice no es válido
        }
    }
}
