using UnityEngine;

public class PlayerData : MonoBehaviour
{
    // Atributos
    [SerializeField]
    private string playerName;
    public string PlayerName { get => playerName; set => playerName = value; }

    [SerializeField]
    private int playerIndex = 0;
    public int PlayerIndex { get => playerIndex; set => playerIndex = value; }

    [SerializeField]
    private int money = 0;
    public int Money { get => money; set => money = value; }

    [SerializeField]
    private int score = 0;
    public int Score { get => score; set => score = value; }

    private GameState playerState = GameState.EnCurso;
    public GameState PlayerState { get => playerState; set => playerState = value; }

    // Movimiento del jugador
    private PlayerMovement playerMovement;
    public PlayerMovement PlayerMovement { get => playerMovement; }

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerMovement.CornerOffset = PlayerCorner.GetCorner(playerIndex);
    }

    // Inicializar datos del jugador
    public void InitializePlayer(string name, int initialMoney, int initialScore, int index)
    {
        PlayerName = name;
        Money = initialMoney;
        Score = initialScore;
        PlayerIndex = index;
    }
}
