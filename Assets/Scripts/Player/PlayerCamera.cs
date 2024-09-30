using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField]
    private Transform player;
    public Transform Player { get => player; set { player = value; StartCoroutine(UpdateCamera()); } }
    [SerializeField]
    private float distance = 4.0f;  // Distancia desde el jugador
    [SerializeField]
    private float height = 3.0f;     // Altura respecto al jugador
    [SerializeField]
    private float angle = 10.0f;    // Ángulo de inclinación hacia abajo
    [SerializeField]
    private float smoothSpeed = 0.125f;  // Velocidad de la interpolación

    private Vector3 offset;

    void Start()
    {
        CalculateOffset();
    }

    // Calcula el offset de la cámara
    private void CalculateOffset()
    {
        // Establece el offset para que la cámara esté a la derecha del jugador
        offset = Quaternion.Euler(angle, -90, 0) * new Vector3(0, 0, -distance);
        offset += Vector3.up * height;
    }

    // Actualiza la posición de la cámara en movimiento
    void LateUpdate()
    {
        if (player != null)
        {
            // Actualiza la posición de la cámara para que siga al jugador con el offset
            transform.position = player.position + offset;
            transform.LookAt(player.position);
        }
    }

    // Actualiza la posición de la cámara de forma suave
    private IEnumerator UpdateCamera()
    {
        if (player == null) yield break;

        Vector3 targetPosition = player.position + offset;
        Quaternion targetRotation = Quaternion.LookRotation(player.position - transform.position);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * smoothSpeed;  // Controla la velocidad de la transición
            transform.position = Vector3.Lerp(transform.position, targetPosition, t);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);
            yield return null;  // Esperar al siguiente frame
        }

        // Asegurarse de que la cámara está exactamente en la posición y rotación correctas al final
        transform.position = targetPosition;
        transform.LookAt(player.position);
    }
}
