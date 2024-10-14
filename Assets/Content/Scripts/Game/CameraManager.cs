using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{


    [SerializeField] private Transform cameraTarget;    // Referencia al objeto CameraTarget (dummy)
    [SerializeField] private float transitionDuration;  // Ajusta este valor para la velocidad de la transición
    [SerializeField] private float elapsedTime;         // Tiempo transcurrido desde el inicio de la transición

    private void Awake()
    {
        InitCameras();
    }

    private void InitCameras()
    {
        cameraTarget = GameObject.Find("CameraTarget").transform;
    }

    public void CurrentCamera(Transform currentPlayer)
    {
        cameraTarget.position = currentPlayer.transform.position;
        cameraTarget.SetParent(currentPlayer.transform);
    }

    public IEnumerator UpdateCurrentCamera(Transform targetTransform)
    {
        // Posición inicial del CameraTarget
        Vector3 initialPosition = cameraTarget.position;

        // Posición objetivo es la del nuevo jugador
        Vector3 targetPosition = targetTransform.position;

        // Realizar la transición
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;

            // Interpolación suave de la posición del CameraTarget
            cameraTarget.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / transitionDuration);

            yield return null; // Esperar un frame antes de continuar la interpolación
        }
        elapsedTime = 0f; // Reiniciar el tiempo transcurrido

        // Asegurarse de que el CameraTarget llegue exactamente a la posición objetivo
        cameraTarget.position = targetPosition;

        // Hacer que el CameraTarget sea hijo del jugador para seguir su movimiento
        cameraTarget.SetParent(targetTransform);
    }
}