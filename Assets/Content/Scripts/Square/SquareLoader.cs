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
        Transform containerSquares = transform;
        squares = new Transform[containerSquares.childCount];
        for (int i = 0; i < squares.Length; i++)
        {
            squares[i] = containerSquares.GetChild(i);
        }
        squareCount = squares.Length;
    }
}
