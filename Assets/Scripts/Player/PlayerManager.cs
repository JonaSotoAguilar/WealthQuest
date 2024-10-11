using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using System.Collections;


[System.Serializable]
public class PlayerManager : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;         // Datos del jugador
    [SerializeField] private PlayerInput playerInput;       // Entrada del jugador
    [SerializeField] private PlayerMovement playerMovement; // Movimiento del jugador
    [SerializeField] private PlayerCanvas playerCanvas;     // Canvas del jugador
    [SerializeField] private PlayerDice playerDice;         // Dado del jugador

    public PlayerDice PlayerDice { get => playerDice; }

    // Inicialización los Input del jugador
    public void InitializePlayer(PlayerData assignedPlayer, PlayerInput input, PlayerMovement movement, PlayerCanvas canvas, PlayerDice dice)
    {
        playerData = assignedPlayer;
        playerInput = input;
        playerMovement = movement;
        playerCanvas = canvas;
        playerDice = dice;
        playerDice.ShowDice(false);

        playerMovement.CornerOffset = PlayerCorner.GetCorner(playerData.Index);
    }

    // Jugar turno
    public void Throw(CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            playerInput.SwitchCurrentActionMap("UI");
            StartCoroutine(ThrowDice());
        }
    }

    // Lanzar el dado y esperar a que termine
    public IEnumerator ThrowDice()
    {
        //GameManager.Instance.Cameras.ChangeDiceView();

        // Llamar a la corrutina del dado y esperar a que termine
        yield return playerDice.StopDice();

        // El dado ya terminó, ahora mover al jugador
        yield return MovePlayer();
    }

    // Mover al jugador actual
    private IEnumerator MovePlayer()
    {
        //GameManager.Instance.Cameras.ChangePlayerView();

        // Llamar a la corrutina de movimiento y esperar a que termine
        yield return playerMovement.MovePlayer(playerDice.DiceRoll, playerData);

        // Una vez que termine el movimiento, jugar la casilla
        yield return PlaySquare();
    }

    // Jugar casilla
    private IEnumerator PlaySquare()
    {
        Square square = GameManager.Instance.Squares.Squares[playerData.CurrentPosition].GetComponent<Square>();

        // Llamar a la corrutina de la casilla
        yield return square.ActiveSquare(playerData, playerCanvas);

        // Volver al mapa de acción del jugador
        GameManager.Instance.HUD.UpdatePlayer(playerData);
        yield return GameManager.Instance.UpdateTurn();
    }
}
