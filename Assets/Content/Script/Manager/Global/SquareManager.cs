using UnityEngine;

public class SquareManager : MonoBehaviour {
    private static SquareManager instance;

    private Square[] squares;

    public static Square[] Squares { get => instance.squares; }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        InitializeSquares();
    }

    private void InitializeSquares()
    {
        Transform containerSquares = GameObject.Find("Squares").transform;
        squares = new Square[containerSquares.childCount];
        for (int i = 0; i < squares.Length; i++)
            squares[i] = containerSquares.GetChild(i).GetComponent<Square>();
    }

}