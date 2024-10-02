using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;

[System.Serializable]
public class PlayerController : MonoBehaviour
{
    private PlayerData player; // Datos del jugador
    private PlayerInput playerInput; // Entrada del jugador
    private PlayerMovement playerMovement; // Movimiento del jugador
    private CanvasPlayer canvasPlayer; // Canvas del jugador

    [Header("Player Components")]
    private DiceController dice;
    private HUDController hud;
    private SquareLoader squares;

    // Inicialización los Input del jugador
    public void InitializePlayer(PlayerData assignedPlayer, PlayerInput input, PlayerMovement movement, CanvasPlayer canvas)
    {
        player = assignedPlayer;
        playerInput = input;
        playerMovement = movement;
        canvasPlayer = canvas;

        playerMovement.CornerOffset = PlayerCorner.GetCorner(player.Index);
    }

    public void InitializeComponents(DiceController diceController, HUDController hudController, SquareLoader squareLoader)
    {
        dice = diceController;
        hud = hudController;
        squares = squareLoader;
    }

    // Jugar turno
    public void PlayTurn(CallbackContext context)
    {
        if (GameManager.Instance.CanPlayTurn(player.Index))
        {
            GameManager.Instance.InitTurn = false;
            StartCoroutine(ThrowDice(player));
        }
    }

    // Lanzar el dado
    public IEnumerator ThrowDice(PlayerData player)
    {
        GameManager.Instance.ChangeDiceView();
        dice.LaunchDice();
        while (!dice.DiceSleeping)
        {
            yield return null;
        }

        StartCoroutine(MovePlayer(player));
    }

    // Mover al jugador actual
    private IEnumerator MovePlayer(PlayerData player)
    {
        GameManager.Instance.ChangePlayerView();
        playerMovement.MovePlayer(dice.DiceRoll);
        while (!playerMovement.PlayerSleeping)
        {
            yield return null;
        }

        StartCoroutine(PlaySquare(player));
    }

    // Jugar casilla
    private IEnumerator PlaySquare(PlayerData player)
    {
        Square square = squares.Squares[player.CurrentPosition].GetComponent<Square>();
        playerInput.SwitchCurrentActionMap("UI");
        hud.ShowPanel(false);
        square.ActiveSquare(player, canvasPlayer);

        while (!square.SquareSleeping())
        {
            yield return null;
        }
        playerInput.SwitchCurrentActionMap("Player");
        hud.UpdatePlayer(player);
        hud.ShowPanel(true);
        GameManager.Instance.UpdateTurn();
    }
}
