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

    [Header("Input System")]
    [SerializeField] private InputActionAsset inputActions;
    private PlayerInput input;
    private InputAction throwAction;
    private bool isGamepad = false;

    private bool rollDice = false;

    #region Getters

    public PlayerLocalData Data { get => data; }
    public PlayerLocalUI UI { get => ui; }
    public PlayerMovement Movement { get => movement; }
    public Animator Animator { get => animator; set => animator = value; }

    #endregion

    #region Initialization

    public void Initialize(PlayerData data, PlayerInput input = null)
    {
        this.data.PlayerData = data;
        this.input = input;

        animator = GetComponentInChildren<Animator>();
        movement.Animator = animator;

        if (input == null) SetActions();
        else SetScheme();
    }

    private void SetActions()
    {
        throwAction = inputActions?.FindAction("Throw");
        throwAction.performed += ctx => OnThrowAction();
        throwAction.Enable();
    }

    private void SetScheme()
    {
        if (input.currentControlScheme == "Keyboard" || input.currentControlScheme == "Keyboard&Mouse" || input.currentControlScheme == "Mouse")
        {
            isGamepad = false;
        }
        else
        {
            isGamepad = true;
        }
    }

    private void OnDestroy()
    {
        if (throwAction != null)
        {
            throwAction.performed -= ctx => OnThrowAction();
        }
        StopAllCoroutines();
    }

    #endregion

    #region Game Actions

    public void StartTurn()
    {
        // Crear pregunta
        if (input != null) GameUIManager.ChangeScheme(isGamepad);
        GameUIManager.ActiveMenu();
        GameUIManager.ActiveUIActions(true);
        ui.CreateQuestion();
    }

    public void Throw(CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;
        OnThrowAction();
    }
    private void OnThrowAction()
    {
        if (!rollDice) return;
        DiceRoll(false);
    }

    public void DiceRoll(bool active)
    {
        if (active)
        {
            rollDice = true;
            if (input != null) input.SwitchCurrentActionMap("Player");
            dice.ShowDice(true);
            GameUIManager.ActiveUIActions(false);
            GameUIManager.ActiveThrowActions(true);
            StartCoroutine(dice.RotateDiceRoutine());
        }
        else
        {
            rollDice = false;
            if (input != null) input.SwitchCurrentActionMap("UI");
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

    // Mover jugador
    private IEnumerator Move(int steps)
    {
        yield return movement.Move(steps, data.Position);
        data.NewPosition(movement.NewPosition);
        ActiveSquare();
    }

    // Activar casilla
    public void ActiveSquare()
    {
        Square square = SquareManager.Squares[data.Position];
        GameUIManager.ActiveThrowActions(false);
        GameUIManager.ActiveUIActions(true);
        ui.SetupCards(square);
    }

    // Finalizar turno
    public void FinishTurn()
    {
        GameUIManager.ActiveUIActions(false);
        GameUIManager.ActiveThrowActions(false);
        GameLocalManager.FinishTurn();
    }

    #endregion
}
