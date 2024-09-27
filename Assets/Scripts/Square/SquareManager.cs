using UnityEngine;

public class SquareManager : MonoBehaviour
{
    public static SquareManager Instance { get; private set; }
    public Transform[] Squares { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            InitializeSquares();
        }
    }

    private void InitializeSquares()
    {
        GameObject containerSquares = GameObject.Find("Squares");
        if (containerSquares == null)
        {
            Debug.LogError("El objeto 'Squares' no se ha encontrado en la escena.");
            return;
        }
        Squares = new Transform[containerSquares.transform.childCount];
        for (int i = 0; i < Squares.Length; i++)
        {
            Squares[i] = containerSquares.transform.GetChild(i);
        }
    }
}

