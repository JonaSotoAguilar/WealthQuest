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
    public PlayerData Player { get => player; }

    private PlayerInput playerInput; // Entrada del jugador
    public PlayerInput PlayerInput { get => playerInput; }

    private CanvasPlayer canvasPlayer; // Canvas del jugador

    // Inicialización los Input del jugador
    public void Initialize(PlayerData assignedPlayer, PlayerInput input, CanvasPlayer canvas)
    {
        player = assignedPlayer;
        playerInput = input;
        canvasPlayer = canvas;
    }

    // Jugar turno
    public void PlayTurn(CallbackContext context)
    {
        // Comprobar si es posible jugar el turno
        if (GameManager.Instance.CanPlayTurn(player.PlayerIndex))
        {
            GameManager.Instance.InitTurn = false; // Ya inició el turno
            StartCoroutine(ThrowDice(player)); // Lanzar el dado
        }
    }

    // Lanzar el dado
    public IEnumerator ThrowDice(PlayerData player)
    {
        // Esperar hasta que el dado se detenga y obtener el resultados
        GameManager.Instance.ChangeDiceView(); // Cambiar a la vista del jugador
        GameManager.Instance.Dice.LaunchDice();
        while (!GameManager.Instance.Dice.DiceSleeping)
        {
            yield return null;
        }

        StartCoroutine(MovePlayer(player)); // Mover al jugador actual
    }

    // Mover al jugador actual
    private IEnumerator MovePlayer(PlayerData player)
    {
        // Mover al jugador actual
        GameManager.Instance.ChangePlayerView();
        player.PlayerMovement.MovePlayer(GameManager.Instance.Dice.DiceRoll);
        while (!player.PlayerMovement.PlayerSleeping) // Esperar hasta que el jugador se detenga
        {
            yield return null;
        }

        StartCoroutine(PlaySquare(player));
    }

    // Jugar casilla
    private IEnumerator PlaySquare(PlayerData player)
    {
        // Pasamos el jugador y su canvas
        Square square = GameManager.Instance.Squares.SquaresBoard[player.PlayerMovement.CurrentPosition].GetComponent<Square>();
        playerInput.SwitchCurrentActionMap("UI"); // Cambiar al mapa de acción UI
        GameManager.Instance.HUD.ShowPanel(false); // Mostrar el panel
        square.ActiveSquare(player, canvasPlayer); // Pasamos el dispositivo del jugador actual

        while (!square.SquareSleeping()) // Esperar hasta que la casilla se termine de ejecutar
        {
            yield return null;
        }

        playerInput.SwitchCurrentActionMap("Player");
        GameManager.Instance.HUD.ShowPanel(true); // Mostrar el panel
        GameManager.Instance.HUD.UpdateHUD(player); // Actualizar el HUD
        GameManager.Instance.UpdateTurn(); // Actualizar el turno
    }
}
