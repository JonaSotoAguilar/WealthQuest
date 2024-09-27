using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Atributos
    [SerializeField]
    private string playerName;
    [SerializeField]
    private int playerIndex = 0;
    [SerializeField]
    private int money = 0;
    [SerializeField]
    private int score = 0;
    private GameState playerState = GameState.EnCurso;
    private Vector3[] corners = new Vector3[]
    {
        new Vector3(-0.5f, 0f, 0.5f),
        new Vector3(0.5f, 0f, 0.5f),
        new Vector3(-0.5f, 0f, -0.5f),
        new Vector3(0.5f, 0f, -0.5f)
    };
    private PlayerMovement playerMovement; // Componente de movimiento del jugador

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    // Inicializar datos del jugador
    public void InitializePlayer(string name, int initialMoney, int initialScore, int index)
    {
        playerName = name;
        money = initialMoney;
        score = initialScore;
        playerIndex = index;
        playerMovement.SetCorner(corners[index]);
    }

    // Mover al jugador
    public void MovePlayer(int steps)
    {
        playerMovement.MovePlayer(steps);
    }

    // TODO:

    // Comprobar si el jugador está en movimiento
    public bool IsPlayerStopped()
    {
        return playerMovement.IsPlayerStopped();
    }

    // Obtener la posición actual del jugador
    public int CurrentPlayerPosition()
    {
        return playerMovement.GetCurrentPosition();
    }

    // TODO: Getters

    // Obtener el índice del jugador
    public string GetPlayerName()
    {
        return playerName;
    }

    // Obtener el dinero del jugador
    public int GetMoney()
    {
        return money;
    }

    // Obtener la puntuación del jugador
    public int GetScore()
    {
        return score;
    }

    // Obtener el índice del jugador
    public int GetPlayerIndex()
    {
        return playerIndex;
    }

    // Obtener el estado del jugador
    public GameState GetPlayerState()
    {
        return playerState;
    }

    // TODO: Setters

    // Modificar puntuación
    public void ModifyScore(int points)
    {
        score += points;
        CanvasManager.instance.UpdateHUD(this);
    }

    // Modificar dinero
    public void ModifyMoney(int amount)
    {
        money += amount;
    }

    // Modificar estado del jugador
    public void SetPlayerState(GameState state)
    {
        playerState = state;
    }
}
