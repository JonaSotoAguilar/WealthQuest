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
        if (GameManager.Instance.Squares.SquareCount > 0)
        {
            int remainingSquares = GameManager.Instance.Squares.SquareCount - player.CurrentPosition - 1;
            steps = Mathf.Min(steps, remainingSquares);
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
            Vector3 initialPosition = transform.position;
            Transform squareTransform = GameManager.Instance.Squares.Squares[player.CurrentPosition];
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

    public void InitPosition()
    {
        Transform squareTransform = GameManager.Instance.Squares.Squares[0];
        Vector3 positionCenterBox = squareTransform.position;
        RaycastHit hit;
        Vector3 rayStart = positionCenterBox + Vector3.up * 10;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, Mathf.Infinity, groundLayerMask))
        {
            Vector3 destinyPosition = hit.point + cornerOffset; 
            transform.position = destinyPosition; 
            transform.forward = squareTransform.forward;
        }
        else
        {
            Debug.LogError("No se encontró la superficie bajo la casilla.");
        }
    }


}
