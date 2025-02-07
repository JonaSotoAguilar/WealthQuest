using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speedMovement = 1.5f;
    [SerializeField] private Animator animator;

    private int groundLayerMask;
    private int newPosition;

    public Animator Animator { set => animator = value; }
    public int NewPosition { get => newPosition; }

    private void Awake()
    {
        groundLayerMask = LayerMask.GetMask("Ground");
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    // Move
    public IEnumerator Move(int steps, int currPosition)
    {
        Square[] squares = SquareManager.Squares;

        for (int i = 0; i < steps; i++)
        {
            // Avanzar la posición del jugador en el tablero
            currPosition++;
            if (currPosition >= squares.Length) currPosition = 0;

            // Configurar la casilla de destino y su posición central
            Transform squareTransform = squares[currPosition].transform;
            Vector3 positionCenterBox = squareTransform.position;
            RaycastHit hit;
            Vector3 rayStart = positionCenterBox + Vector3.up * 10;

            if (Physics.Raycast(rayStart, Vector3.down, out hit, Mathf.Infinity, groundLayerMask))
            {
                Vector3 destinyPosition = hit.point;
                Vector3 movementDirection = (destinyPosition - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);

                // Configurar animación de movimiento
                animator.SetBool("isMoving", true);

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

        animator.SetBool("isMoving", false);
        newPosition = currPosition;
    }

    public void CenterPlayer(int position)
    {
        Square[] squares = SquareManager.Squares;

        Transform squareTransform = squares[position].transform;
        Square currentSquare = squares[position];
        currentSquare.RemovePlayer(this, position);

        // Posicionarse en el centro de la casilla
        Vector3 targetPosition = squareTransform.position;
        transform.position = targetPosition;
        transform.localScale = Vector3.one;

        // Calcular la orientación hacia la siguiente casilla
        int nextPosition = (position + 1) % squares.Length;
        Vector3 nextSquarePosition = squares[nextPosition].transform.position;
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

    public void CornerPlayer(int position)
    {
        Square[] squares = SquareManager.Squares;

        Transform squareTransform = squares[position].transform;
        Square currentSquare = squares[position];

        int playerIndex = currentSquare.GetPlayerIndex(this);
        int totalPlayers = currentSquare.PlayersCount;
        Vector3 characterForwardDirection = transform.forward;

        Vector3 targetPosition = squareTransform.position;
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        //Ajustar en base a characterForwardDirection 
        if (Mathf.Abs(characterForwardDirection.x) > Mathf.Abs(characterForwardDirection.z))
        {
            targetPosition.z += (characterForwardDirection.z > 0) ? 1 : -1;
            float positionX = CalculatePosition(totalPlayers, playerIndex);
            targetPosition.x += positionX;
        }
        else
        {
            targetPosition.x += (characterForwardDirection.x > 0) ? 1 : -1;
            float positionZ = CalculatePosition(totalPlayers, playerIndex);
            targetPosition.z += positionZ;
        }
        transform.position = targetPosition;
    }

    private float CalculatePosition(int totalPlayers, int playerIndex)
    {
        if (totalPlayers == 1) return 0f;
        else if (totalPlayers == 2) return (playerIndex == 0) ? -0.5f : 0.5f;
        else return -0.75f + 1.5f * playerIndex / (totalPlayers - 1);
    }

    public void AddCornerPosition(int position)
    {
        Square[] squares = SquareManager.Squares;
        squares[position].AddPlayer(this, position);
    }
}
