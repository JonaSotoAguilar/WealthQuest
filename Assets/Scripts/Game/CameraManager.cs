using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{


    [SerializeField] private Transform cameraTarget; // Referencia al objeto CameraTarget (dummy)
    [SerializeField] private Camera diceCamera; // Cámara para el dado
    [SerializeField] private Camera playerCamera; // Cámara para el jugador


    private void Awake()
    {
        InitCameras();
    }

    private void InitCameras()
    {
        cameraTarget = GameObject.Find("CameraTarget").transform;
        diceCamera = GameObject.Find("DiceCamera").GetComponent<Camera>();
        playerCamera = GameObject.Find("CinemachineCamera").GetComponent<Camera>();
    }

    public void CurrentCamera(Transform currentPlayer) {
        cameraTarget.position = currentPlayer.transform.position;
        cameraTarget.SetParent(currentPlayer.transform);
    }

    public IEnumerator UpdateCurrentCamera(Transform targetTransform)
    {
        float transitionDuration = 1.5f; // Ajusta este valor para la velocidad de la transición
        float elapsedTime = 0f;

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

        // Asegurarse de que el CameraTarget llegue exactamente a la posición objetivo
        cameraTarget.position = targetPosition;

        // Hacer que el CameraTarget sea hijo del jugador para seguir su movimiento
        cameraTarget.SetParent(targetTransform);
    }

    // Cambiar a la vista del dado
    public void ChangeDiceView()
    {
        //Camera playerCamera = currentPlayer.GetComponentInChildren<Camera>();
        playerCamera.enabled = false;
        diceCamera.enabled = true;
    }

    public void ChangePlayerView()
    {
        //Camera playerCamera = currentPlayer.GetComponentInChildren<Camera>();
        playerCamera.enabled = true;
        diceCamera.enabled = false;
    }
}