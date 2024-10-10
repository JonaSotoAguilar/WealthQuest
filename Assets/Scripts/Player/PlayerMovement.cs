using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speedMovement;
    [SerializeField] private Vector3 cornerOffset;

    public Vector3 CornerOffset { get => cornerOffset; set => cornerOffset = value; }

    // Mueve al jugador en la cantidad de pasos especificada
    public IEnumerator MovePlayer(int steps, PlayerData player)
    {
        if (GameManager.Instance.Squares.SquareCount > 0)
        {
            int remainingSquares = GameManager.Instance.Squares.SquareCount - player.CurrentPosition - 1;
            steps = Mathf.Min(steps, remainingSquares);

            // Comenzar el movimiento del jugador y esperar a que termine
            yield return StartCoroutine(Move(steps, player));
        }
        else
        {
            Debug.LogError("No se encontraron casillas para mover al jugador.");
        }
    }

    // Ejecutar movimiento
    private IEnumerator Move(int steps, PlayerData player)
    {
        for (int i = 0; i < steps; i++)
        {
            player.CurrentPosition++;
            Vector3 initialPosition = transform.position;
            Transform squareTransform = GameManager.Instance.Squares.Squares[player.CurrentPosition];
            Vector3 positionCenterBox = squareTransform.position;

            // Realizamos un raycast hacia abajo desde la casilla para encontrar la superficie
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
    }

    public void InitPosition()
    {
        // Get the square transform from the game manager, corresponding to the initial position.
        Transform squareTransform = GameManager.Instance.Squares.Squares[0];

        // Calculate the position based on the center of the square and the corner offset.
        Vector3 squareCenterPosition = squareTransform.position;
        RaycastHit hit;
        Vector3 rayStart = squareCenterPosition + Vector3.up * 10;

        // Cast a ray downwards to detect the surface below the square.
        if (Physics.Raycast(rayStart, Vector3.down, out hit, Mathf.Infinity))
        {
            // Set the player's position to the calculated destination with the corner offset.
            transform.position = hit.point + cornerOffset;

            // Align the player's rotation to the surface normal.
            transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }
        else
        {
            Debug.LogError("No surface found beneath the square.");
        }
    }

}
