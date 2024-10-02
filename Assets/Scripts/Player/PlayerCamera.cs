using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCamera : MonoBehaviour
{
    private Vector3 offset;
    [SerializeField] private Transform player;
    [SerializeField] private float distance = 4.0f; 
    [SerializeField] private float height = 3.0f; 
    [SerializeField] private float angle = 10.0f;  
    [SerializeField] private float smoothSpeed = 0.125f; 

    public Transform Player { get => player; set { player = value; StartCoroutine(UpdateCamera()); } }

    void Start()
    {
        CalculateOffset();
    }

    // Calcula el offset de la cámara
    private void CalculateOffset()
    {
        offset = Quaternion.Euler(angle, -90, 0) * new Vector3(0, 0, -distance);
        offset += Vector3.up * height;
    }

    // Actualiza la posición de la cámara en movimiento
    void LateUpdate()
    {
        if (player != null)
        {
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
            t += Time.deltaTime * smoothSpeed; 
            transform.position = Vector3.Lerp(transform.position, targetPosition, t);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);
            yield return null;  
        }

        transform.position = targetPosition;
        transform.LookAt(player.position);
    }
}
