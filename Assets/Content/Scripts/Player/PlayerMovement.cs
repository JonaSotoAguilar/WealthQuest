using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speedMovement;
    [SerializeField] private Vector3 cornerOffset;
    [SerializeField] private Vector2 direction;
    private Animator playerAnimator;
    private int groundLayerMask;

    public Vector3 CornerOffset { get => cornerOffset; set => cornerOffset = value; }
    public Animator PlayerAnimator { get => playerAnimator; set => playerAnimator = value; }

    void Awake()
    {
        groundLayerMask = LayerMask.GetMask("Ground");
    }

    public IEnumerator MovePlayer(int steps, PlayerData player)
    {
        if (GameManager.Instance.SquareList.Length > 0)
        {
            yield return StartCoroutine(Move(steps, player));

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
            if (player.CurrentPosition >= GameManager.Instance.SquareList.Length)
            {
                player.CurrentPosition = 0;
            }

            Vector3 initialPosition = transform.position;
            Transform squareTransform = GameManager.Instance.SquareList[player.CurrentPosition];
            Vector3 positionCenterBox = squareTransform.position;
            RaycastHit hit;
            Vector3 rayStart = positionCenterBox + Vector3.up * 10;

            if (Physics.Raycast(rayStart, Vector3.down, out hit, Mathf.Infinity, groundLayerMask))
            {
                Vector3 destinyPosition = hit.point + cornerOffset;
                Vector3 movementDirection = (destinyPosition - initialPosition).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);

                playerAnimator.SetFloat("X", 0);
                playerAnimator.SetFloat("Y", 1);

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


    public void InitPosition(int position)
    {
        Transform squareTransform = GameManager.Instance.SquareList[position];
        Vector3 positionCenterBox = squareTransform.position;
        RaycastHit hit;
        Vector3 rayStart = positionCenterBox + Vector3.up * 10;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, Mathf.Infinity, groundLayerMask))
        {
            Vector3 destinyPosition = hit.point + cornerOffset;
            transform.position = destinyPosition;

            // Calcula la siguiente casilla con un comportamiento circular
            int nextPosition = (position + 1) % GameManager.Instance.SquareList.Length;

            Vector3 nextSquarePosition = GameManager.Instance.SquareList[nextPosition].position;
            Vector3 directionToNext = (nextSquarePosition - destinyPosition).normalized;

            // Ajusta la rotación en base a la dirección hacia la siguiente casilla
            if (Mathf.Abs(directionToNext.x) > Mathf.Abs(directionToNext.z))
                transform.rotation = directionToNext.x > 0 ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, -90, 0);
            else
                transform.rotation = directionToNext.z > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
        }
        else
        {
            Debug.LogError("No se encontró la superficie bajo la casilla.");
        }
    }

    public IEnumerator CenterPosition(int position)
    {
        if (position >= 0 && position < GameManager.Instance.SquareList.Length)
        {
            Transform squareTransform = GameManager.Instance.SquareList[position];
            Vector3 targetPosition = squareTransform.position;

            // Inicializar el tiempo de interpolación
            float time = 0f;
            Vector3 initialPosition = transform.position;

            while (time < 1f)
            {
                time += Time.deltaTime * speedMovement;
                transform.position = Vector3.Lerp(initialPosition, targetPosition, time);
                yield return null;
            }
        }
        else
        {
            Debug.LogError("La posición proporcionada está fuera del rango de casillas.");
        }
    }

    public IEnumerator CornerPosition(int position)
    {
        if (position >= 0 && position < GameManager.Instance.SquareList.Length)
        {
            Transform squareTransform = GameManager.Instance.SquareList[position];
            Vector3 targetPosition = squareTransform.position + cornerOffset;  // Mueve a la esquina de la casilla

            // Inicializar el tiempo de interpolación
            float time = 0f;
            Vector3 initialPosition = transform.position;

            while (time < 1f)
            {
                time += Time.deltaTime * speedMovement;
                transform.position = Vector3.Lerp(initialPosition, targetPosition, time);
                yield return null;
            }
        }
        else
        {
            Debug.LogError("La posición proporcionada está fuera del rango de casillas.");
        }
    }



}
