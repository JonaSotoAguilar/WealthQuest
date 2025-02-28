using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNetManager : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerNetData data;
    [SerializeField] private PlayerNetUI ui;
    [SerializeField] private Dice dice;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private Animator animator;

    // Actions
    [SerializeField] private InputActionAsset inputActions;
    private InputAction throwAction;

    // Flags
    [SyncVar(hook = nameof(DiceRoll))] private bool rollDice = false;

    #region Getters

    public PlayerNetData Data { get => data; }
    public PlayerNetUI UI { get => ui; }
    public PlayerMovement Movement { get => movement; }
    public Animator Animator { get => animator; set => animator = value; }

    #endregion

    #region Initialization

    public void Start()
    {
        GameNetManager.PlayerJoined(this);
        GameUIManager.InitializeHUD(data.UID);

        if (isOwned)
        {
            throwAction = inputActions.FindAction("Throw");
            throwAction.performed += ctx => Throw();
            throwAction.Enable();
        }
    }

    private void OnDestroy()
    {
        if (throwAction != null)
            throwAction.performed -= ctx => Throw();

        StopAllCoroutines();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    #endregion

    #region Game Actions

    [Server]
    public void StartTurn()
    {
        //1. Initialize Question
        RpcActiveMenu(netIdentity.connectionToClient, true);
        RpcActiveUIActions(netIdentity.connectionToClient, true);
        ui.CreateQuestion();
    }

    //3. Throw Dice
    public void Throw()
    {
        if (!rollDice) return;

        CmdEnableDice(false);
    }

    [Command]
    public void CmdEnableDice(bool enable)
    {
        rollDice = enable;
    }

    [Server]
    public void EnableDice(bool enable)
    {
        rollDice = enable;
    }

    private void DiceRoll(bool oldRoll, bool newRoll)
    {
        if (newRoll)
        {
            dice.ShowDice(true);

            if (isOwned)
            {
                GameUIManager.ActiveUIActions(false);
                GameUIManager.ActiveThrowActions(true);
                StartCoroutine(dice.RotateDiceRoutine());
            }
            else
            {
                StartCoroutine(dice.SoundDice());
            }
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
        else StartCoroutine(dice.StopSpin());
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
        RpcActiveThrowActions(netIdentity.connectionToClient, false);
        RpcActiveUIActions(netIdentity.connectionToClient, true);
        Square square = SquareManager.Squares[data.Position];
        ui.SetupCards(square);
    }

    //6. Finish Turn
    [Server]
    public void FinishTurn()
    {
        RpcActiveUIActions(netIdentity.connectionToClient, false);
        RpcActiveThrowActions(netIdentity.connectionToClient, false);
        GameNetManager.FinishTurn();
    }

    #endregion

    #region UI Actions

    [TargetRpc]
    private void RpcActiveMenu(NetworkConnectionToClient target, bool active)
    {
        GameUIManager.ActiveMenu();
    }

    [TargetRpc]
    private void RpcActiveUIActions(NetworkConnectionToClient target, bool active)
    {
        GameUIManager.ActiveUIActions(active);
    }

    [TargetRpc]
    private void RpcActiveThrowActions(NetworkConnectionToClient target, bool active)
    {
        GameUIManager.ActiveThrowActions(active);
    }

    #endregion

}
