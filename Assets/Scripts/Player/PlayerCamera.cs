using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform player;
    public float distance = 4.0f;  // Distancia desde el jugador
    public float height = 3.0f;     // Altura respecto al jugador
    public float angle = 10.0f;    // Ángulo de inclinación hacia abajo

    private Vector3 offset;

    void Start()
    {
        // Establece el offset para que la cámara esté a la derecha del jugador
        // Quaternion.Euler(angulo, 90, 0) coloca la cámara a la derecha
        offset = Quaternion.Euler(angle, -90, 0) * new Vector3(0, 0, -distance);
        offset += Vector3.up * height;
    }

    void LateUpdate()
    {
        if (player != null)
        {
            // Actualiza la posición de la cámara para que siga al jugador con el offset
            transform.position = player.position + offset;

            // La cámara debe mirar hacia el jugador
            transform.LookAt(player.position);
        }
    }
}
