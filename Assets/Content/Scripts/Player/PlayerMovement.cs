using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speedMovement;
    [SerializeField] private Vector3 cornerOffset;
    [SerializeField] private Vector2 direction;
    private Animator playerAnimator;
    private int layerMask;

    public Animator PlayerAnimator { get => playerAnimator; set => playerAnimator = value; }
    public Vector3 CornerOffset { get => cornerOffset; set => cornerOffset = value; }

    private void Awake()
    {
        // Inicializa la máscara de Layer para ignorar el Layer "Trigger"
        layerMask = ~(1 << LayerMask.NameToLayer("Trigger"));
    }

    public IEnumerator MovePlayer(int steps, PlayerData player)
    {
        if (GameManager.Instance.Squares.SquareCount > 0)
        {
            int remainingSquares = GameManager.Instance.Squares.SquareCount - player.CurrentPosition - 1;
            steps = Mathf.Min(steps, remainingSquares);
            yield return StartCoroutine(Move(steps, player));
            // Resetear la dirección a cero al final del movimiento completo
            direction = Vector2.zero;
            playerAnimator.SetFloat("X", direction.x);
            playerAnimator.SetFloat("Y", direction.y);
        }
        else
        {
            Debug.LogError("No se encontraron casillas para mover al jugador.");
        }
    }

    private IEnumerator Move(int steps, PlayerData player)
    {
        for (int i = 0; i < steps; i++)
        {
            player.CurrentPosition++;
            Vector3 initialPosition = transform.position;
            Transform squareTransform = GameManager.Instance.Squares.Squares[player.CurrentPosition];
            Vector3 positionCenterBox = squareTransform.position;
            RaycastHit hit;
            Vector3 rayStart = positionCenterBox + Vector3.up * 10;

            if (Physics.Raycast(rayStart, Vector3.down, out hit, Mathf.Infinity, layerMask))
            {
                Vector3 destinyPosition = hit.point + cornerOffset;
                Vector3 movementDirection = (destinyPosition - initialPosition).normalized;

                // Calcular la rotación deseada para mirar hacia la dirección del destino
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);

                // Establece la animación para moverse hacia adelante
                playerAnimator.SetFloat("X", 0); // Asumiendo que el Blend Tree ya no usa 'X' y 'Y' para la dirección
                playerAnimator.SetFloat("Y", 1); // Siempre activa correr hacia adelante

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
        Transform squareTransform = GameManager.Instance.Squares.Squares[0];
        Vector3 squareCenterPosition = squareTransform.position;
        RaycastHit hit;
        Vector3 rayStart = squareCenterPosition + Vector3.up * 10;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            transform.position = hit.point + cornerOffset;
            transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            direction = Vector2.zero;
            playerAnimator.SetFloat("X", direction.x);
            playerAnimator.SetFloat("Y", direction.y);
        }
        else
        {
            Debug.LogError("No surface found beneath the square.");
        }
    }
}
