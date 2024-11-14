using System.Collections;
using FishNet.Component.Animating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerControllerNetwork : PlayerNetwork
{
    [Header("Player Components")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerCanvas ui;
    [SerializeField] private PlayerDice dice;
    private PlayerAnimator animator;

    // Variables de control de pregunta en Server
    private bool canThrow = false;
    private bool questionAnswered = false;
    private bool wasAnswerCorrect = false;
    private bool squareAnswered = false;

    void Update()
    {
        if (IsOwner && canThrow && Input.GetKeyDown(KeyCode.Space))
        {
            canThrow = false;
            Throw();
        }
    }

    public override void OnStartClient()
    {
        if (!IsOwner) ui.ActiveCanvas(false);
        else if (GameManagerNetwork.Instance.LocalPlayer == null) GameManagerNetwork.Instance.LocalPlayer = this;
    }

    [Server]
    public void InitializePlayer()
    {
        GetPlayerComponents();
    }

    [Server]
    private void GetPlayerComponents()
    {
        animator = GetComponentInChildren<PlayerAnimator>();
        animator.ActiveNetworkAnimator();
        movement.Animator = animator;
        GameManagerNetwork.Instance.Players.Add(this);
    }

    [Server]
    public void InitializePosition(int position) => movement.InitPosition(position);

    #region Methods Getters

    public override PlayerMovement Movement { get => movement; }

    #endregion

    #region Methods Actions Client
    public void Throw()
    {
        CmdThrowDice();
    }

    private void OnAnswerQuestion(bool isCorrect)
    {
        if (!IsOwner) return;
        ui.QuestionPanel.OnQuestionAnswered -= OnAnswerQuestion;
        CmdSubmitAnswer(isCorrect);
    }

    private void OnAnswerSquare()
    {
        if (!IsOwner) return;
        ui.CardsPanel.OnCardSelected -= OnAnswerSquare;
        CmdSubmitCard();
    }

    #endregion

    #region Methods Server Question

    [Server]
    public override IEnumerator CmdCreateQuestion()
    {
        // 1. Obtener la pregunta
        QuestionData question = gameData.GetRandomQuestion();

        // 2. Mostrar la pregunta en todos los clientes
        RpcShowQuestion(question);

        // 3. Esperar a que el Owner responda la pregunta
        questionAnswered = false;
        yield return new WaitUntil(() => questionAnswered);

        // 4. Habilitar o saltar turno del jugador
        if (wasAnswerCorrect)
        {
            wasAnswerCorrect = false;
            EnableDice();
        }
        else FinishTurn();
    }

    [ObserversRpc]
    public void RpcShowQuestion(QuestionData question)
    {
        ui.QuestionPanel.SetupQuestion(question, this);
        if (IsOwner) ui.QuestionPanel.OnQuestionAnswered += OnAnswerQuestion;
    }

    [ServerRpc]
    private void CmdSubmitAnswer(bool isCorrect)
    {
        CloseQuestionPanel();
        wasAnswerCorrect = isCorrect;
        questionAnswered = true;
    }

    [ObserversRpc] //FIXME: Agregar la respuesta seleccionada (index)
    private void CloseQuestionPanel() => ui.QuestionPanel.ClosePanel();

    #endregion

    #region Methods Server Throw Dice

    [Server]
    private void EnableDice()
    {
        StartCoroutine(dice.RotateDiceRoutine());
        RpcEnableDice();
        TargetEnableCanThrow(Owner);
    }

    [ObserversRpc]
    private void RpcEnableDice()
    {
        dice.ShowDice(true);
    }

    [TargetRpc]
    private void TargetEnableCanThrow(NetworkConnection target)
    {
        canThrow = true;
    }

    [ServerRpc]
    private void CmdThrowDice()
    {
        animator.Jump();
        StartCoroutine(ThrowDice());
    }

    [Server]
    private IEnumerator ThrowDice()
    {
        yield return dice.StopDice();
        RpcDisableDice();
        StartCoroutine(MovePlayer());
    }

    [ObserversRpc]
    private void RpcDisableDice() => dice.ShowDice(false);

    #endregion

    #region Methods Server Square

    [Server]
    private IEnumerator MovePlayer()
    {
        yield return movement.MovePlayer(dice.DiceRoll, Position);
        position.Value = movement.NewPosition;
        StartCoroutine(PlaySquare());
    }

    [Server]
    private IEnumerator PlaySquare()
    {
        // 1. Muestra las cartas en todos los clientes
        RpcShowCards(Position);

        // 2. Esperar a que el Owner juegue la casilla
        squareAnswered = false;
        yield return new WaitUntil(() => squareAnswered);

        // 3. Terminar turno del jugador
        FinishTurn();
    }

    [ObserversRpc]
    private void RpcShowCards(int position)
    {
        Square square = GameManagerNetwork.Instance.Squares[position];
        square.SetupSquare(this, ui);
        if (IsOwner) ui.CardsPanel.OnCardSelected += OnAnswerSquare;
    }

    [ServerRpc]
    private void CmdSubmitCard()
    {
        CloseCardsPanel();
        squareAnswered = true;
    }

    [ObserversRpc] //FIXME: Agregar la carta seleccionada (index)
    private void CloseCardsPanel() => ui.CardsPanel.ClosePanel();

    [Server]
    private void FinishTurn()
    {
        movement.CornerPosition(Position);
        GameManagerNetwork.Instance.NextTurn();
    }

    #endregion

}
