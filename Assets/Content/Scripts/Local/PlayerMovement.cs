using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speedMovement;

    // Movement Components
    private PlayerAnimator animator;
    private int groundLayerMask;
    private int newPosition;
    private Vector2 direction;

    // Game Manager Instance
    [SerializeField] private IGameManager game;

    public PlayerAnimator Animator { set => animator = value; }

    void Awake()
    {
        groundLayerMask = LayerMask.GetMask("Ground");
    }

    #region Methods Getters & Setters

    public int NewPosition { get => newPosition; }

    #endregion

    #region Methods Movement

    public IEnumerator MovePlayer(int steps, int currPosition)
    {
        if (game.Squares.Length < 0)
        {
            Debug.LogError("No se encontraron casillas para mover al jugador.");
            yield break;
        }

        yield return StartCoroutine(Move(steps, currPosition));
        direction = Vector2.zero;
        animator.SetMoving(direction.x, direction.y);
    }


    private IEnumerator Move(int steps, int currPosition)
    {
        for (int i = 0; i < steps; i++)
        {
            // Avanzar la posición del jugador en el tablero
            currPosition++;
            if (currPosition >= game.Squares.Length) currPosition = 0;

            // Configurar la casilla de destino y su posición central
            Transform squareTransform = game.Squares[currPosition].transform;
            Vector3 positionCenterBox = squareTransform.position;
            RaycastHit hit;
            Vector3 rayStart = positionCenterBox + Vector3.up * 10;

            if (Physics.Raycast(rayStart, Vector3.down, out hit, Mathf.Infinity, groundLayerMask))
            {
                Vector3 destinyPosition = hit.point;
                Vector3 movementDirection = (destinyPosition - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);

                // Configurar animación de movimiento
                animator.SetMoving(0, 1);

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
        newPosition = currPosition;
    }

    #endregion

    #region Methods Position

    // public void InitPosition(int position)
    // {
    //     game = GameOnline.Instance;
    //     game.Squares[position].AddPlayer(this);
    // }

    public void CenterPosition(int position)
    {
        Transform squareTransform = game.Squares[position].transform;

        // Posicionarse en el centro de la casilla
        Vector3 targetPosition = squareTransform.position;
        transform.position = targetPosition;
        transform.localScale = Vector3.one;

        // Calcular la orientación hacia la siguiente casilla
        int nextPosition = (position + 1) % game.Squares.Length;
        Vector3 nextSquarePosition = game.Squares[nextPosition].transform.position;
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

    // public void CornerPosition(int position)
    // {
    //     Transform squareTransform = game.Squares[position].transform;
    //     Square currentSquare = game.Squares[position];
    //     //currentSquare.AddPlayer(this);

    //     //int playerIndex = currentSquare.GetPlayerIndex(this);
    //     int totalPlayers = currentSquare.PlayersCount;
    //     Vector3 characterForwardDirection = transform.forward;

    //     Vector3 targetPosition = squareTransform.position;
    //     transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

    //     //Ajustar en base a characterForwardDirection 
    //     if (Mathf.Abs(characterForwardDirection.x) > Mathf.Abs(characterForwardDirection.z))
    //     {
    //         targetPosition.z += (characterForwardDirection.z > 0) ? 1 : -1;
    //         //float positionX = CalculatePosition(totalPlayers, playerIndex);
    //         //targetPosition.x += positionX;
    //     }
    //     else
    //     {
    //         targetPosition.x += (characterForwardDirection.x > 0) ? 1 : -1;
    //         float positionZ = CalculatePosition(totalPlayers, playerIndex);
    //         targetPosition.z += positionZ;
    //     }
    //     transform.position = targetPosition;
    // }

    // private float CalculatePosition(int totalPlayers, int playerIndex)
    // {
    //     if (totalPlayers == 1) return 0f;
    //     else if (totalPlayers == 2) return (playerIndex == 0) ? -0.5f : 0.5f;
    //     else return -0.75f + 1.5f * playerIndex / (totalPlayers - 1);
    // }

    #endregion

}
