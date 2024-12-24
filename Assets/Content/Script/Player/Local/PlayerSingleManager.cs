using System.Collections;
using Mirror.Examples.MultipleMatch;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerSingleManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerLocalData data;
    [SerializeField] private PlayerLocalUI ui;
    [SerializeField] private Dice dice;
    [SerializeField] private PlayerMovement movement;
    private Animator animator;
    private bool rollDice = false;

    #region Getters

    public PlayerLocalData Data { get => data; }
    public PlayerMovement Movement { get => movement; }
    public Animator Animator { get => animator; set => animator = value; }

    #endregion

    #region Initialization

    public void Initialize(PlayerData data, PlayerInput input)
    {
        this.data.PlayerData = data;

        //GameLocalManager.PlayerJoined(this);
        GameUIManager.InitializeHUD(data.UID);

        animator = GetComponentInChildren<Animator>();
        movement.Animator = animator;
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
        if (!rollDice || context.phase != InputActionPhase.Performed) return;
        DiceRoll(false);
    }

    public void DiceRoll(bool active)
    {
        if (active)
        {
            rollDice = true;
            dice.ShowDice(true);
            StartCoroutine(dice.RotateDiceRoutine());
        }
        else
        {
            rollDice = false;
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
