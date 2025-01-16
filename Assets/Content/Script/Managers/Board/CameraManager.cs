using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

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
        cameraTarget.position = currentPlayer.transform.position;
        cameraTarget.rotation = currentPlayer.rotation;
        cameraTarget.SetParent(currentPlayer.transform);
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
            cameraTarget.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / transitionDuration);

            cameraTarget.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime / transitionDuration);
            yield return null;
        }
        cameraTarget.position = targetPosition;
        cameraTarget.rotation = targetRotation;
        cameraTarget.SetParent(targetTransform);
    }
}