using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;

[System.Serializable]
public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private PlayerData playerData; // Datos del jugador
    [SerializeField] private PlayerInput playerInput; // Entrada del jugador
    [SerializeField] private PlayerMovement playerMovement; // Movimiento del jugador
    [SerializeField] private CanvasPlayer canvasPlayer; // Canvas del jugador

    // Jugar turno
    public void Throw(CallbackContext context)
    {
        if (GameManager.Instance.CanPlayTurn(playerData.Index))
        {
            GameManager.Instance.InitTurn = false;
            StartCoroutine(ThrowDice());
        }
    }

    // Inicialización los Input del jugador
    public void InitializePlayer(PlayerData assignedPlayer, PlayerInput input, PlayerMovement movement, CanvasPlayer canvas)
    {
        playerData = assignedPlayer;
        playerInput = input;
        playerMovement = movement;
        canvasPlayer = canvas;

        playerMovement.CornerOffset = PlayerCorner.GetCorner(playerData.Index);
    }

    // Lanzar el dado
    public IEnumerator ThrowDice()
    {
        GameManager.Instance.ChangeDiceView();
        GameManager.Instance.Dice.LaunchDice();
        while (!GameManager.Instance.Dice.DiceSleeping)
        {
            yield return null;
        }

        StartCoroutine(MovePlayer());
    }

    // Mover al jugador actual
    private IEnumerator MovePlayer()
    {
        GameManager.Instance.ChangePlayerView();
        playerMovement.MovePlayer(GameManager.Instance.Dice.DiceRoll, playerData);
        while (!playerMovement.PlayerSleeping)
        {
            yield return null;
        }

        StartCoroutine(PlaySquare());
    }

    // Jugar casilla
    private IEnumerator PlaySquare()
    {
        Square square = GameManager.Instance.Squares.Squares[playerData.CurrentPosition].GetComponent<Square>();
        playerInput.SwitchCurrentActionMap("UI");
        GameManager.Instance.HUD.ShowPanel(false);
        square.ActiveSquare(playerData, canvasPlayer);

        while (!square.SquareSleeping())
        {
            yield return null;
        }
        playerInput.SwitchCurrentActionMap("Player");
        GameManager.Instance.HUD.UpdatePlayer(playerData);
        GameManager.Instance.HUD.ShowPanel(true);
        GameManager.Instance.UpdateTurn();
    }
}
