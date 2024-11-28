using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerLocalManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerLocalData data;
    [SerializeField] private PlayerLocalUI ui;
    [SerializeField] private Dice dice;
    [SerializeField] private PlayerMovement movement;
    private Animator animator;
    private PlayerInput input;

    #region Getters

    public PlayerLocalData Data { get => data; }
    public PlayerMovement Movement { get => movement; }
    public Animator Animator { get => animator; set => animator = value; }

    #endregion

    #region Initialization

    public void Initialize(PlayerData data, PlayerInput input)
    {
        Debug.Log("PlayerLocalManager Initialize");
        this.data.PlayerData = data;
        this.input = input;

        GameLocalManager.PlayerJoined(this);
        GameUIManager.InitializeHUD(data.UID);

        animator = GetComponentInChildren<Animator>();
        movement.Animator = animator;
        Debug.Log("PlayerLocalManager Finish Initialize");
    }

    #endregion

    #region Game Actions

    public void StartTurn()
    {
        //1. Center player
        movement.CenterPlayer(data.Position);

        //2. Initialize Question
        ui.CreateQuestion();
    }

    //3. Throw Dice
    public void Throw(CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;
        input.SwitchCurrentActionMap("UI");
        DiceRoll(false);
    }

    public void DiceRoll(bool active)
    {
        if (active)
        {
            input.SwitchCurrentActionMap("Player");
            dice.ShowDice(true);
            StartCoroutine(dice.RotateDiceRoutine());
        }
        else
        {
            animator.SetTrigger("Jump");
            StartCoroutine(StopDice());
        }
    }

    private IEnumerator StopDice()
    {
        StartCoroutine(dice.StopDice());
        yield return new WaitForSeconds(2.5f);
        dice.ShowDice(false);
        StartCoroutine(Move(dice.DiceRoll));
    }

    //4. Move Player
    private IEnumerator Move(int steps)
    {
        yield return movement.Move(steps, data.Position);
        data.NewPosition(movement.NewPosition);
        ActiveSquare();
    }

    //5. Active Square
    public void ActiveSquare()
    {
        Square square = SquareManager.Squares[data.Position];
        ui.SetupCards(square);
    }

    //6. Finish Turn
    public void FinishTurn()
    {
        movement.CornerPlayer(data.Position);
        GameLocalManager.FinishTurn();
    }

    #endregion
}
