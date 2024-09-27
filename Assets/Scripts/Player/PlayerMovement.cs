using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    // Position
    private Vector3 cornerOffset;
    private int currentPosition = 0;
    // Movement
    public float speedMovement = 2f;
    // Flag
    private bool playerSleeping;

    public void MovePlayer(int steps)
    {
        // Asegúrate de que SquareManager está inicializado y tiene casillas disponibles
        if (SquareManager.Instance != null && SquareManager.Instance.Squares.Length > 0)
        {
            int remainingSquares = SquareManager.Instance.Squares.Length - currentPosition - 1;
            steps = Mathf.Min(steps, remainingSquares);

            // Comenzar movimiento
            playerSleeping = false;
            StartCoroutine(Move(steps));
        }
        else
        {
            Debug.LogError("SquareManager no está disponible o no tiene casillas inicializadas.");
        }
    }

    private IEnumerator Move(int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            currentPosition++;
            Vector3 initialPosition = transform.position;
            Transform squareTransform = SquareManager.Instance.Squares[currentPosition];
            Vector3 positionCenterBox = squareTransform.position;
            RaycastHit hit;
            Vector3 rayStart = positionCenterBox + Vector3.up * 10;
            if (Physics.Raycast(rayStart, Vector3.down, out hit, Mathf.Infinity))
            {
                Vector3 destinyPosition = hit.point + cornerOffset;
                Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                float time = 0f;
                while (time < 1f)
                {
                    time += Time.deltaTime * speedMovement;
                    transform.position = Vector3.Lerp(initialPosition, destinyPosition, time);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, time);
                    yield return null;
                }
            }
            else
            {
                Debug.LogError("No se encontró la superficie bajo la casilla.");
            }
        }
        playerSleeping = true;
    }

    public void SetCorner(Vector3 corner)
    {
        cornerOffset = corner;
    }

    public bool IsPlayerStopped()
    {
        return playerSleeping;
    }

    public int GetCurrentPosition()
    {
        return currentPosition;
    }
}
