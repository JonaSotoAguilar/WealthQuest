using UnityEngine;

public class SquareController : MonoBehaviour
{
    private Transform[] squaresBoard;
    public Transform[] SquaresBoard { get => squaresBoard; }

    private int squareCount;
    public int SquareCount { get => squareCount; }

    private void Awake()
    {
        InitializeSquares();
    }

    private void InitializeSquares()
    {
        // Asumimos que el script está en el objeto "Squares", por lo que no necesitamos buscarlo.
        Transform containerSquares = transform; // Nos referimos a sí mismo
        squaresBoard = new Transform[containerSquares.childCount];
        for (int i = 0; i < squaresBoard.Length; i++)
        {
            squaresBoard[i] = containerSquares.GetChild(i);
        }
        squareCount = squaresBoard.Length;
    }
}
