using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCanvas playerCanvas;
    [SerializeField] private PlayerDice playerDice;
    [SerializeField] private Animator playerAnimator;

    public PlayerDice PlayerDice { get => playerDice; }

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

    public void EnableDice() 
    {
        playerDice.ShowDice(true);
        playerInput.SwitchCurrentActionMap("Player");
    }

    public IEnumerator Jump()
    {
        playerAnimator.SetTrigger("Jump");
        yield return new WaitForSeconds(2.3f);
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
        StartCoroutine(Jump());
        yield return playerDice.StopDice();
        yield return MovePlayer();
    }

    // Mover al jugador actual
    private IEnumerator MovePlayer()
    {
        yield return playerMovement.MovePlayer(playerDice.DiceRoll, playerData);
        yield return PlaySquare();
    }

    // Jugar casilla
    private IEnumerator PlaySquare()
    {
        Square square = GameManager.Instance.Squares.Squares[playerData.CurrentPosition].GetComponent<Square>();
        yield return square.ActiveSquare(playerData, playerCanvas);
        yield return GameManager.Instance.UpdateTurn();
    }
}
