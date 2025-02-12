using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float transitionDuration;
    private float elapsedTime;

    private void Awake()
    {
        cameraTarget = GameObject.Find("CameraTarget").transform;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void CurrentCamera(Transform currentPlayer)
    {
        cameraTarget.position = currentPlayer.position;
        cameraTarget.rotation = currentPlayer.rotation;
        cameraTarget.SetParent(currentPlayer);

        // Forzar la actualización de Cinemachine para aplicar la nueva orientación inmediatamente
        cinemachineCamera.ForceCameraPosition(cameraTarget.position, cameraTarget.rotation);
    }

    public IEnumerator UpdateCurrentCamera(Transform targetTransform)
    {
        Quaternion initialRotation = cameraTarget.rotation;
        Vector3 initialPosition = cameraTarget.position;
        Quaternion targetRotation = targetTransform.rotation;
        Vector3 targetPosition = targetTransform.position;

        elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;

            cameraTarget.position = Vector3.Lerp(initialPosition, targetPosition, t);
            cameraTarget.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);

            // Forzar la actualización de Cinemachine en cada frame para aplicar los cambios
            cinemachineCamera.ForceCameraPosition(cameraTarget.position, cameraTarget.rotation);

            yield return null;
        }

        // Asegurar la posición y rotación final exacta
        cameraTarget.position = targetPosition;
        cameraTarget.rotation = targetRotation;
        cameraTarget.SetParent(targetTransform);

        // Forzar la actualización final de Cinemachine
        cinemachineCamera.ForceCameraPosition(cameraTarget.position, cameraTarget.rotation);
    }

}