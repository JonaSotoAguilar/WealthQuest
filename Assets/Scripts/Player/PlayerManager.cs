using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;

[System.Serializable]
public class PlayerManager : MonoBehaviour
{
    [SerializeField] private PlayerData playerData; // Datos del jugador
    [SerializeField] private PlayerInput playerInput; // Entrada del jugador
    [SerializeField] private PlayerMovement playerMovement; // Movimiento del jugador
    [SerializeField] private CanvasPlayer canvasPlayer; // Canvas del jugador

    // Inicialización los Input del jugador
    public void InitializePlayer(PlayerData assignedPlayer, PlayerInput input, PlayerMovement movement, CanvasPlayer canvas)
    {
        playerData = assignedPlayer;
        playerInput = input;
        playerMovement = movement;
        canvasPlayer = canvas;

        playerMovement.CornerOffset = PlayerCorner.GetCorner(playerData.Index);
    }

    // // Jugar turno
    // public void Throw(CallbackContext context)
    // {
    //     Debug.Log("Throw");
    //     playerInput.actions.FindAction("Throw").Disable();
    //     playerInput.SwitchCurrentActionMap("UI");
    //     StartCoroutine(ThrowDice());
    //     if (GameManager.Instance.CanPlayTurn(playerData.Index))
    //     {
    //         GameManager.Instance.InitTurn = false;
    //         playerInput.SwitchCurrentActionMap("UI");
    //         StartCoroutine(ThrowDice());
    //     }
    // }

    // Jugar turno
    public void Throw(CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            Debug.Log("Throw");
            playerInput.SwitchCurrentActionMap("UI");
            StartCoroutine(ThrowDice());
        }
    }

    // Lanzar el dado y esperar a que termine
    public IEnumerator ThrowDice()
    {
        GameManager.Instance.ChangeDiceView();

        // Llamar a la corrutina del dado y esperar a que termine
        yield return GameManager.Instance.Dice.LaunchDice();

        // El dado ya terminó, ahora mover al jugador
        yield return MovePlayer();
    }

    // Mover al jugador actual
    private IEnumerator MovePlayer()
    {
        GameManager.Instance.ChangePlayerView();

        // Llamar a la corrutina de movimiento y esperar a que termine
        yield return playerMovement.MovePlayer(GameManager.Instance.Dice.DiceRoll, playerData);

        // Una vez que termine el movimiento, jugar la casilla
        yield return PlaySquare();
    }

    // Jugar casilla
    private IEnumerator PlaySquare()
    {
        Square square = GameManager.Instance.Squares.Squares[playerData.CurrentPosition].GetComponent<Square>();
        GameManager.Instance.HUD.ShowPanel(false);

        // Llamar a la corrutina de la casilla
        yield return square.ActiveSquare(playerData, canvasPlayer);

        // Volver al mapa de acción del jugador
        GameManager.Instance.HUD.UpdatePlayer(playerData);
        GameManager.Instance.UpdateTurn();
    }
}