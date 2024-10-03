using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speedMovement = 2f;
    [SerializeField] private PlayerData player;
    [SerializeField] private SquareLoader squares;
    private Vector3 cornerOffset;
    private bool playerSleeping;

    public Vector3 CornerOffset { get => cornerOffset; set => cornerOffset = value; }
    public bool PlayerSleeping { get => playerSleeping; }
    public PlayerData Player { get => player; set => player = value; }
    // Mueve al jugador en la cantidad de pasos especificada
    public void MovePlayer(int steps)
    {
        if (squares.SquareCount > 0)
        {
            int remainingSquares = squares.SquareCount - player.CurrentPosition - 1;
            steps = Mathf.Min(steps, remainingSquares);

            // Comenzar movimiento
            playerSleeping = false;
            StartCoroutine(Move(steps));
        }
        else
        {
            Debug.LogError("No se encontraron casillas para mover al jugador.");
        }
    }

    // Ejecutar movimiento
    private IEnumerator Move(int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            player.CurrentPosition++;
            Vector3 initialPosition = transform.position;
            Transform squareTransform = squares.Squares[player.CurrentPosition];
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

    // TODO: Implementar método para posicionar en casilla inicial
    public void InitPosition()
    {
        player.CurrentPosition = 0;
        transform.position = squares.Squares[player.CurrentPosition].position + cornerOffset;
    }
}
