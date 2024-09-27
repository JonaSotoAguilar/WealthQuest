using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class Player : MonoBehaviour
{
    public string playerName;
    public int money;
    public int score;
    public int playerIndex;
    public GameState playerState = GameState.EnCurso;
    public PlayerMovement playerMovement; // Atributo para almacenar la referencia de PlayerMovement
    public PlayerInput playerInput; // Atributo para almacenar la referencia de PlayerInpu
    private Vector3[] corners = new Vector3[]
    {
        new Vector3(-0.5f, 0f, 0.5f),
        new Vector3(0.5f, 0f, 0.5f),
        new Vector3(-0.5f, 0f, -0.5f),
        new Vector3(0.5f, 0f, -0.5f)
    };

    // Initialization
    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>() ?? gameObject.AddComponent<PlayerMovement>();
        playerInput = GetComponent<PlayerInput>();
        playerInput.actions["Action"].performed += ctx => PlayTurn();
    }

    // Jugar turno
    public void PlayTurn()
    {
        // Si el estado del jugador es EnCurso, es su Turno y puede lanzar el dado
        if (playerState == GameState.EnCurso && GameControls.Instance.getCurrentPlayer() == playerIndex && GameControls.Instance.canPlayTurn())
        {
            GameControls.Instance.StartCoroutine(GameControls.Instance.ThrowDice());
        }
    }

    // Inicializar jugador
    public void InitializePlayer(string name, int initialMoney, int initialScore, int index)
    {
        playerName = name;
        money = initialMoney;
        score = initialScore;
        playerIndex = index;
        playerMovement.SetCorner(corners[index]);
    }


    // Modificar puntuación
    public void ModifyScore(int points)
    {
        score += points;
        HUDController.instance.UpdateHUD(this);
    }

    // Modificar dinero
    public void ModifyMoney(int amount)
    {
        money += amount;
    }
}
