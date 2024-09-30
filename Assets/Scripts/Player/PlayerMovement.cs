using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    // Atributos
    private int currentPosition = 0; // Posición actual
    public int CurrentPosition { get => currentPosition; }

    [SerializeField]
    private float speedMovement = 2f; // Velocidad de movimiento

    private bool playerSleeping; // Controla si el jugador está en movimiento
    public bool PlayerSleeping { get => playerSleeping; }

    private Vector3 cornerOffset; // Offset de la esquina
    public Vector3 CornerOffset { get => cornerOffset; set => cornerOffset = value; }

    // Mueve al jugador en la cantidad de pasos especificada
    public void MovePlayer(int steps)
    {
        // Asegúrate de que SquareManager está inicializado y tiene casillas disponibles
        if (GameManager.Instance.Squares.SquareCount > 0)
        {
            int remainingSquares = GameManager.Instance.Squares.SquareCount - currentPosition - 1;
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

    // Ejecutar movimiento
    private IEnumerator Move(int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            currentPosition++;
            Vector3 initialPosition = transform.position;
            Transform squareTransform = GameManager.Instance.Squares.SquaresBoard[currentPosition];
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
}
