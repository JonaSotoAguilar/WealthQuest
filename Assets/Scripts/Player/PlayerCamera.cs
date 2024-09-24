using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform jugador; // La pieza del jugador que la cámara debe seguir
    public Vector3 offset; // Distancia entre la cámara y el jugador

    void Start()
    {
        // Si no has asignado manualmente el offset en el inspector, se puede calcular automáticamente
        if (jugador != null)
        {
            offset = transform.position - jugador.position;
        }
    }

    void LateUpdate()
    {
        if (jugador != null)
        {
            // Actualiza la posición de la cámara para que siga al jugador con el mismo offset
            transform.position = jugador.position + offset;
        }
    }
}
