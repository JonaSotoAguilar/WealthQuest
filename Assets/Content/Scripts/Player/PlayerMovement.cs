using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speedMovement;
    [SerializeField] private Vector2 direction;
    private Animator playerAnimator;
    private int groundLayerMask;

    public Animator PlayerAnimator { get => playerAnimator; set => playerAnimator = value; }

    private void Awake()
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
            // Avanzar la posición del jugador en el tablero
            player.CurrentPosition++;
            if (player.CurrentPosition >= GameManager.Instance.SquareList.Length)
            {
                player.CurrentPosition = 0;
            }

            // Configurar la casilla de destino y su posición central
            Transform squareTransform = GameManager.Instance.SquareList[player.CurrentPosition];
            Vector3 positionCenterBox = squareTransform.position;
            RaycastHit hit;
            Vector3 rayStart = positionCenterBox + Vector3.up * 10;

            if (Physics.Raycast(rayStart, Vector3.down, out hit, Mathf.Infinity, groundLayerMask))
            {
                Vector3 destinyPosition = hit.point; // Ir directamente al centro de la casilla
                Vector3 movementDirection = (destinyPosition - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);

                // Configurar animación de movimiento
                playerAnimator.SetFloat("X", 0);
                playerAnimator.SetFloat("Y", 1);

                // Interpolación de movimiento hacia la siguiente casilla
                float time = 0f;
                Vector3 initialPosition = transform.position;
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

        // Al finalizar el movimiento, reinicia la animación a estado idle
        direction = Vector2.zero;
        playerAnimator.SetFloat("X", direction.x);
        playerAnimator.SetFloat("Y", direction.y);
    }


    public void InitPosition(int position)
    {
        // FIXME
        //GameManager.Instance.SquareList[position].GetComponent<Square>().AddPlayer(this);
    }

    public void CenterPosition(int position)
    {
        Transform squareTransform = GameManager.Instance.SquareList[position];

        // Posicionarse en el centro de la casilla
        Vector3 targetPosition = squareTransform.position;
        transform.position = targetPosition;
        transform.localScale = Vector3.one;

        // Calcular la orientación hacia la siguiente casilla
        int nextPosition = (position + 1) % GameManager.Instance.SquareList.Length;
        Vector3 nextSquarePosition = GameManager.Instance.SquareList[nextPosition].position;
        Vector3 directionToNext = (nextSquarePosition - targetPosition).normalized;

        // Ajustar la rotación para mirar hacia la siguiente casilla
        if (Mathf.Abs(directionToNext.x) > Mathf.Abs(directionToNext.z))
        {
            transform.rotation = directionToNext.x > 0 ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, -90, 0);
        }
        else
        {
            transform.rotation = directionToNext.z > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
        }
    }

    public void CornerPosition(int position)
    {
        Transform squareTransform = GameManager.Instance.SquareList[position];
        Square currentSquare = GameManager.Instance.SquareList[position].GetComponent<Square>();
        //FIXME
        //currentSquare.AddPlayer(this);

        //int playerIndex = currentSquare.GetPlayerIndex(this);
        int totalPlayers = currentSquare.PlayersCount;
        Vector3 characterForwardDirection = transform.forward;

        Vector3 targetPosition = squareTransform.position;
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        //Ajustar en base a characterForwardDirection 
        if (Mathf.Abs(characterForwardDirection.x) > Mathf.Abs(characterForwardDirection.z))
        {
            targetPosition.z += (characterForwardDirection.z > 0) ? 1 : -1;
            //float positionX = CalculatePosition(totalPlayers, playerIndex);
            //targetPosition.x += positionX;
        }
        else
        {
            targetPosition.x += (characterForwardDirection.x > 0) ? 1 : -1;
            //float positionZ = CalculatePosition(totalPlayers, playerIndex);
            //targetPosition.z += positionZ;
        }
        transform.position = targetPosition;
    }

    private float CalculatePosition(int totalPlayers, int playerIndex)
    {
        if (totalPlayers == 1) return 0f;
        else if (totalPlayers == 2) return (playerIndex == 0) ? -0.5f : 0.5f;
        else return -0.75f + 1.5f * playerIndex / (totalPlayers - 1);
    }
}
