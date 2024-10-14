using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using System.Collections;
using UnityEditor.Callbacks;


[System.Serializable]
public class PlayerController : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField] private PlayerData playerData;         // Datos del jugador
    [SerializeField] private PlayerInput playerInput;       // Entrada del jugador
    [SerializeField] private PlayerMovement playerMovement;                  // Movimiento del jugador
    [SerializeField] private PlayerCanvas playerCanvas;                      // Canvas del jugador
    [SerializeField] private PlayerDice playerDice;                          // Dado del jugador
    [SerializeField] private Animator playerAnimator;                        // Controlador de animación

    public PlayerDice PlayerDice { get => playerDice; }
    public Animator PlayerAnimator { get => playerAnimator; }

    // Inicialización los Input del jugador
    public void InitializePlayer(PlayerData assignedPlayer, PlayerInput input)
    {
        playerData = assignedPlayer;
        playerInput = input;

        playerMovement = GetComponent<PlayerMovement>();
        playerCanvas = GetComponentInChildren<PlayerCanvas>();
        playerDice = GetComponentInChildren<PlayerDice>();
        playerAnimator = GetComponentInChildren<Animator>();

        playerMovement.PlayerAnimator = playerAnimator;
        playerDice.ShowDice(false);
        playerMovement.CornerOffset = PlayerCorner.GetCorner(playerData.Index);
    }

    public void InitPosition()
    {
        playerMovement.InitPosition();
    }

    public IEnumerator Jump()
    {
        playerAnimator.SetTrigger("Jump");

        // Espera hasta que la animación de salto complete
        yield return new WaitForSeconds(2.3f);  // Asegúrate de que jumpDuration corresponde a la duración de la animación de salto
    }


    // Jugar turno
    public void Throw(CallbackContext context)
    {
        // Imprimer si esta en el suelo
        if (context.phase == InputActionPhase.Performed)
        {
            playerInput.SwitchCurrentActionMap("UI");
            StartCoroutine(ThrowDice());
        }
    }

    // Lanzar el dado y esperar a que termine
    public IEnumerator ThrowDice()
    {
        // Ejecutar el salto antes de lanzar el dado
        StartCoroutine(Jump());

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
