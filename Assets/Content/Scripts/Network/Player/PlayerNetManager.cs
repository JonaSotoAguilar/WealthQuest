using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class PlayerNetManager : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerNetData data;
    [SerializeField] private PlayerNetUI ui;
    [SerializeField] private Dice dice;
    [SerializeField] private PlayerNetMovement movement;

    [SerializeField] private Animator animator;

    // Flags
    [SyncVar(hook = nameof(DiceRoll))] private bool rollDice = false;

    //FIXME: Borrar UID
    [SyncVar] private int playerId;

    #region Getters

    public PlayerNetData Data { get => data; }
    public PlayerNetMovement Movement { get => movement; }

    #endregion  

    #region Initialization

    public override void OnStartServer()
    {
        base.OnStartServer();

        playerId = connectionToClient.connectionId;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // FIXME: Borrar UID
        data.Initialize(playerId.ToString(), "Player " + playerId, 0);
        GameNetManager.PlayerJoined(this);

        GameUINetManager.PlayerJoined(data.UID);
    }

    #endregion

    #region Game Actions

    [Server]
    public void StartTurn()
    {
        //1. Centrar jugador
        movement.CenterPlayer(data.Position);

        //2. Crear pregunta
        ui.CreateQuestion();
    }

    private void Update()
    {
        // 3. Lanzar dado
        if (isOwned && rollDice && Input.GetKeyDown(KeyCode.Space))
        {
            CmdEnableDice(false);
        }
    }

    //3. Activar dado
    [Command]
    public void CmdEnableDice(bool enable)
    {
        Debug.Log("CmdEnableDice: " + enable);
        dice.ShowDice(true);
        rollDice = enable;
    }

    private void DiceRoll(bool oldRoll, bool newRoll)
    {
        Debug.Log("DiceRoll: " + newRoll);
        if (newRoll)
        {
            if (isOwned) StartCoroutine(dice.RotateDiceRoutine());
            dice.ShowDice(true);
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
        yield return new WaitForSeconds(2.4f);
        dice.ShowDice(false);
        //if (isOwned) CmdMove();
    }

    //4. Mover jugador
    [Command]
    private void CmdMove()
    {
        StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        yield return movement.Move(dice.DiceRoll, data.Position);
        //ActiveSquare();
    }

    #endregion


}
