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
}
