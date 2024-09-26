using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform jugador;
    public float distancia = 4.0f;  // Distancia desde el jugador
    public float altura = 3.0f;     // Altura respecto al jugador
    public float angulo = 10.0f;    // Ángulo de inclinación hacia abajo

    private Vector3 offset;

    void Start()
    {
        // Establece el offset para que la cámara esté a la derecha del jugador
        // Quaternion.Euler(angulo, 90, 0) coloca la cámara a la derecha
        offset = Quaternion.Euler(angulo, -90, 0) * new Vector3(0, 0, -distancia);
        offset += Vector3.up * altura;
    }

    void LateUpdate()
    {
        if (jugador != null)
        {
            // Actualiza la posición de la cámara para que siga al jugador con el offset
            transform.position = jugador.position + offset;

            // La cámara debe mirar hacia el jugador
            transform.LookAt(jugador.position);
        }
    }
}
