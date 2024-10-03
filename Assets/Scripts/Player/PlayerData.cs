using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [SerializeField] private int index;
    [SerializeField] private string playerName;
    private int score = 0;
    private int currentPosition = 0;
    private GameState state = GameState.EnCurso;

    public string PlayerName { get => playerName; set => playerName = value; }
    public int Index { get => index; set => index = value; }
    public int Score { get => score; set => score = value; }
    public int CurrentPosition { get => currentPosition; set => currentPosition = value; }
    public GameState State { get => state; set => state = value; }

    // Inicializar datos del jugador
    public void InitializePlayer(int playerIndex, string name, int playerScore, int position, GameState playerState)
    {
        index = playerIndex;
        playerName = name;
        score = playerScore;
        currentPosition = position;
        state = playerState;
    }
}

