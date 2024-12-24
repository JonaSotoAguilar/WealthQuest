using System;
using System.Collections;
using Mirror;
using Mirror.Examples.Basic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerNetManager : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerNetData data;
    [SerializeField] private PlayerNetUI ui;
    [SerializeField] private Dice dice;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private Animator animator;

    // Flags
    [SyncVar(hook = nameof(DiceRoll))] private bool rollDice = false;

    #region Getters

    public PlayerNetData Data { get => data; }
    public PlayerMovement Movement { get => movement; }
    public Animator Animator { get => animator; set => animator = value; }

    #endregion

    #region Initialization

    public void Start()
    {
        Debug.Log("PlayerNetManager Start: " + data.UID);

        GameNetManager.PlayerJoined(this);
        GameUIManager.InitializeHUD(data.UID, false);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("PlayerNetManager OnStartServer: " + data.UID);
    }

    #endregion

    #region Game Actions

    [Server]
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
        if (!isOwned || !rollDice || (context.phase != InputActionPhase.Performed)) return;

        CmdEnableDice(false);
    }

    [Command]
    public void CmdEnableDice(bool enable)
    {
        rollDice = enable;
    }

    private void DiceRoll(bool oldRoll, bool newRoll)
    {
        if (newRoll)
        {
            dice.ShowDice(true);
            if (isOwned) StartCoroutine(dice.RotateDiceRoutine());
        }
        else
        {
            animator.SetTrigger("Jump");
            StartCoroutine(StopDice());
        }
    }

    private IEnumerator StopDice()
    {
        if (isOwned) StartCoroutine(dice.StopDice());
        yield return new WaitForSeconds(2.5f);
        dice.ShowDice(false);
        if (isOwned) CmdMove(dice.DiceRoll);
    }

    //4. Move Player
    [Command]
    private void CmdMove(int steps)
    {
        StartCoroutine(Move(steps));
    }

    [Server]
    private IEnumerator Move(int steps)
    {
        yield return movement.Move(steps, data.Position);
        data.NewPosition(movement.NewPosition);
        ActiveSquare();
    }

    //5. Active Square
    [Server]
    public void ActiveSquare()
    {
        Debug.Log("Active Square");
        Square square = SquareManager.Squares[data.Position];
        ui.SetupCards(square);
    }

    //6. Finish Turn
    [Server]
    public void FinishTurn()
    {
        movement.CornerPlayer(data.Position);
        GameNetManager.FinishTurn();
    }

    #endregion


}
