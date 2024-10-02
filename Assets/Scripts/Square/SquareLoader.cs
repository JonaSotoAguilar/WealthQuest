using UnityEngine;

public class SquareLoader: MonoBehaviour
{
    private Transform[] squares;
    private int squareCount;

    public Transform[] Squares { get => squares; }
    public int SquareCount { get => squareCount; }

    private void Awake()
    {
        InitializeSquares();
    }

    private void InitializeSquares()
    {
        // Asumimos que el script está en el objeto "Squares", por lo que no necesitamos buscarlo.
        Transform containerSquares = transform; // Nos referimos a sí mismo
        squares = new Transform[containerSquares.childCount];
        for (int i = 0; i < squares.Length; i++)
        {
            squares[i] = containerSquares.GetChild(i);
        }
        squareCount = squares.Length;
    }
}
