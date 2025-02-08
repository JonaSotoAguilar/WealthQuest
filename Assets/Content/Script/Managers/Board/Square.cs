using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Square : MonoBehaviour
{
    [SerializeField] private GameData data;
    [SerializeField] private SquareType type;
    private List<GameObject> players = new List<GameObject>();

    #region Methods Square

    public List<Card> GetCards()
    {
        switch (type)
        {
            case SquareType.Event:
                return data.GetRandomEventCards(2).Cast<Card>().ToList();
            case SquareType.Expense:
                return data.GetRandomExpenseCards(2).Cast<Card>().ToList();
            case SquareType.Income:
                return data.GetRandomIncomeCards(2).Cast<Card>().ToList();
            case SquareType.Investment:
                return data.GetRandomInvestmentCards(2).Cast<Card>().ToList();
            default:
                return new List<Card>();
        }
    }

    #endregion

    #region Methods Players

    public void AddPlayer(GameObject player)
    {
        players.Add(player);
        UpdateCornerPositions();
    }

    public void RemovePlayer(GameObject player)
    {
        players.Remove(player);
        CenterPosition(player);
        UpdateCornerPositions();
    }

    private void UpdateCornerPositions()
    {
        if (players.Count == 0) return;

        // Obtener la dirección "arriba" de acuerdo a la rotación de la casilla
        Vector3 upDirection = GetUpDirection();

        for (int i = 0; i < players.Count; i++)
        {
            // Distribuir horizontalmente en un rango de [-1, +1] según el número de jugadores
            float horizontalOffset = players.Count > 1 ? Mathf.Lerp(-1f, 1f, (float)i / (players.Count - 1)) : 0f;

            // Calcular la posición del jugador basado en la orientación de la casilla
            Vector3 newPosition = transform.position + upDirection + GetHorizontalDirection() * horizontalOffset;

            players[i].transform.position = newPosition;
            ScalePlayer(players[i], 0.5f); // Reducir tamaño en la esquina
            RotateToCenter(players[i]);   // Rotar hacia el centro de la casilla
        }
    }

    // Obtiene la dirección "arriba" de la casilla según su rotación
    private Vector3 GetUpDirection()
    {
        float yRotation = transform.eulerAngles.y;

        if (yRotation >= 45 && yRotation < 135) return Vector3.forward;  // 90° → Z+
        if (yRotation >= 135 && yRotation < 225) return Vector3.right;    // 180° → X+
        if (yRotation >= 225 && yRotation < 315) return Vector3.back;     // 270° → Z-
        return Vector3.left;                                              // 0° → X-
    }

    // Obtiene la dirección "horizontal" de la casilla según su rotación
    private Vector3 GetHorizontalDirection()
    {
        float yRotation = transform.eulerAngles.y;

        if (yRotation >= 45 && yRotation < 135) return Vector3.right;  // 90° → X
        if (yRotation >= 135 && yRotation < 225) return Vector3.back;  // 180° → Z
        if (yRotation >= 225 && yRotation < 315) return Vector3.left;  // 270° → X
        return Vector3.forward;                                        // 0° → Z
    }

    public void CenterPosition(GameObject player)
    {
        player.transform.position = transform.position;
        ScalePlayer(player, 1.0f); // Restaurar tamaño al centrarse
        RotateToNextSquare(player); // Rotar hacia la siguiente casilla
    }

    private void ScalePlayer(GameObject player, float scale)
    {
        player.transform.localScale = new Vector3(scale, scale, scale);
    }

    private void RotateToCenter(GameObject player)
    {
        Vector3 directionToCenter = transform.position - player.transform.position;
        RotatePlayer(player, directionToCenter);
    }

    private void RotateToNextSquare(GameObject player)
    {
        Square nextSquare = GetNextSquare();
        if (nextSquare != null)
        {
            Vector3 directionToNext = nextSquare.transform.position - player.transform.position;
            RotatePlayer(player, directionToNext);
        }
    }

    private void RotatePlayer(GameObject player, Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            player.transform.rotation = targetRotation;
        }
    }

    private Square GetNextSquare()
    {
        Square[] allSquares = SquareManager.Squares;
        int currentIndex = System.Array.IndexOf(allSquares, this);

        if (currentIndex != -1)
        {
            int nextIndex = (currentIndex + 1) % allSquares.Length;
            return allSquares[nextIndex];
        }

        return null;
    }

    #endregion
}
