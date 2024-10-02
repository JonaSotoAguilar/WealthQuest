using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [SerializeField] private string playerName;
    [SerializeField] private int index;
    private int score = 0;
    private int currentPosition = 0;
    private GameState playerState = GameState.EnCurso;

    public string PlayerName { get => playerName; set => playerName = value; }
    public int Index { get => index; set => index = value; }
    public int Score { get => score; set => score = value; }
    public int CurrentPosition { get => currentPosition; set => currentPosition = value; }
    public GameState PlayerState { get => playerState; set => playerState = value; }

    // Inicializar datos del jugador
    public void InitializePlayer(string name, int playerScore, int playerIndex, int position)
    {
        playerName = name;
        score = playerScore;
        index = playerIndex;
        currentPosition = position;
    }
}

