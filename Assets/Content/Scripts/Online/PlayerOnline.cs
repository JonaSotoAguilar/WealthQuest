using System.Collections;
using System.Collections.Generic;
using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform))]
public class PlayerOnline : NetworkBehaviour, IPlayer
{
    [Header("Player Components")]
    [SerializeField] protected GameData data;
    [SerializeField] private UIPlayer ui;
    [SerializeField] private PlayerDice dice;
    [SerializeField] private PlayerAnimator animator;
    [SerializeField] private HUD hud;

    [Header("Player Settings")]
    [SerializeField] private float interest = 0.05f;
    [SerializeField] private float speedMovement;

    // Player Data
    private readonly SyncVar<int> index = new SyncVar<int>(0);
    private readonly SyncVar<string> nickName = new SyncVar<string>("Jugador");
    private readonly SyncVar<int> points = new SyncVar<int>(0);
    protected readonly SyncVar<int> position = new SyncVar<int>(0);
    private readonly SyncVar<int> characterID = new SyncVar<int>(0);

    // Player Finances
    private readonly SyncVar<int> money = new SyncVar<int>(10000);
    private readonly SyncVar<int> salary = new SyncVar<int>(0);
    private readonly SyncVar<int> invest = new SyncVar<int>(0);
    private readonly SyncVar<int> debt = new SyncVar<int>(0);
    private readonly SyncList<Investment> investments = new SyncList<Investment>();
    private readonly SyncList<Expense> expenses = new SyncList<Expense>();

    // Player Finances Turn
    private readonly SyncVar<int> income = new SyncVar<int>(0);
    private readonly SyncVar<int> expense = new SyncVar<int>(0);


    private readonly SyncVar<int> questionIndex = new SyncVar<int>(0);
    private readonly SyncVar<bool> questionAnswered = new SyncVar<bool>(false);
    private readonly SyncVar<bool> wasAnswerCorrect = new SyncVar<bool>(false);

    private bool canThrow = false;

    private readonly SyncVar<bool> squareAnswered = new SyncVar<bool>(false);
    private readonly SyncList<int> cardsIndex = new SyncList<int>();

    private Vector2 direction = Vector2.zero;
    private int groundLayerMask;
    private GameOnline game = GameOnline.Instance;

    #region Getters & Setters

    public HUD HUD { get => hud; set => hud = value; }
    public int Index { get => index.Value; }
    public string Nickname { get => nickName.Value; }
    public int Position { get => position.Value; set => CmdChangePosition(value); }
    public int Points { get => points.Value; }
    public int Money { get => money.Value; }
    public int Salary { get => salary.Value; set => CmdChangeSalary(value); }
    public int Invest { get => invest.Value; }
    public int Debt { get => debt.Value; }
    public int Income { get => income.Value; }
    public int Expense { get => expense.Value; }

    //public PlayerMovement Movement { get => movement; }
    public Transform Transform { get => transform; }
    public NetworkConnection Connection { get => LocalConnection; }

    #endregion

    #region Initialization

    void Awake()
    {
        groundLayerMask = LayerMask.GetMask("Ground");
    }

    public override void OnStartClient()
    {
        SuscribeEvents();
        InitializeComponents();
        if (IsOwner)
        {
            CmdInitializeHUD();
            CmdInitializePosition();
        }
    }

    private void SuscribeEvents()
    {
        nickName.OnChange += OnChangeNickName;
        position.OnChange += OnChangePosition;
        points.OnChange += OnChangePoints;
        money.OnChange += OnChangeMoney;
        invest.OnChange += OnChangeInvest;
        debt.OnChange += OnChangeDebt;
        income.OnChange += OnChangeIncome;
        expense.OnChange += OnChangeExpense;

        questionIndex.OnChange += OnChangeQuestionIndex;
        cardsIndex.OnChange += OnChangeCardsList;
    }

    public void InitializeData(int i, string name, int model)
    {
        index.Value = i;
        nickName.Value = name;
        characterID.Value = model;

        //FIXME: Copiar data de player
    }

    public void InitializeComponents()
    {
        animator = GetComponentInChildren<PlayerAnimator>();
        animator.ActiveNetworkAnimator();
    }

    [ServerRpc]
    public void CmdInitializeHUD()
    {
        UIOnline.Instance.InitializePlayerHUD(this);
        RpcInitializeHUD();
    }

    [ObserversRpc(BufferLast = true)]
    private void RpcInitializeHUD()
    {
        hud = UIOnline.Instance.GetHUD(index.Value);
        hud.InitHUD(this);
    }

    #endregion

    #region OnChange Events

    private void OnChangeNickName(string oldNickName, string newNickName, bool asServer)
    {
        if (asServer) return;
    }

    private void OnChangePosition(int oldPosition, int newPosition, bool asServer)
    {
        //FIXME: Revisar
    }

    private void OnChangePoints(int oldPoints, int newPoints, bool asServer)
    {
        if (asServer) return;
        hud.SetPoints(newPoints);
    }

    private void OnChangeMoney(int oldMoney, int newMoney, bool asServer)
    {
        if (asServer) return;
        hud.SetMoney(newMoney);
    }

    private void OnChangeInvest(int oldInvest, int newInvest, bool asServer)
    {
        if (asServer) return;
        hud.SetInvest(newInvest);
    }

    private void OnChangeDebt(int oldDebt, int newDebt, bool asServer)
    {
        if (asServer) return;
        hud.SetDebt(newDebt);
    }

    private void OnChangeIncome(int oldIncome, int newIncome, bool asServer)
    {
        if (asServer) return;
        hud.SetIncome(newIncome);
    }

    private void OnChangeExpense(int oldExpense, int newExpense, bool asServer)
    {
        if (asServer) return;
        hud.SetExpense(newExpense);
    }

    #endregion

    #region Client Change Values

    public void AddPoints(int points) => CmdAddPoints(points);

    public void AddMoney(int money) => CmdAddMoney(money);

    public void AddInvest(int invest) => CmdAddInvest(invest);

    public void AddDebt(int debt) => CmdAddDebt(debt);

    public void AddInvestment(Investment newInvestment) => CmdAddInvestment(newInvestment);

    public void AddExpense(Expense newExpense, bool isRecurrent)
    {
        if (isRecurrent) CmdAddExpense(newExpense, false);
        else
        {
            if (money.Value >= newExpense.Amount)
                CmdAddMoney(-newExpense.Amount);
            else
                CmdAddExpense(newExpense, true);
        }
    }

    #endregion

    #region Server Change Values 

    //FIXME: Agregar guardar en gameData
    [ServerRpc]
    private void CmdChangeIndex(int newIndex)
    {
        index.Value = newIndex;
    }

    [ServerRpc]
    private void CmdChangeNickName(string newNickName)
    {
        nickName.Value = newNickName;
    }

    [ServerRpc]
    private void CmdChangePosition(int newPosition)
    {
        position.Value = newPosition;
    }

    [ServerRpc]
    private void CmdChangeSalary(int newSalary)
    {
        income.Value += newSalary - salary.Value;
        salary.Value = newSalary;
    }

    [ServerRpc]
    private void CmdAddPoints(int addPoints)
    {
        points.Value += addPoints;
    }

    [ServerRpc]
    private void CmdAddMoney(int amount)
    {
        money.Value += amount;
    }

    [ServerRpc]
    private void CmdAddInvest(int addInvest)
    {
        invest.Value += addInvest;
    }

    [ServerRpc]
    private void CmdAddDebt(int addDebt)
    {
        debt.Value += addDebt;
    }

    [ServerRpc]
    private void CmdAddInvestment(Investment newInvestment)
    {
        if (newInvestment.Capital > money.Value) return;

        money.Value -= newInvestment.Capital;
        invest.Value += newInvestment.Capital;
        income.Value += newInvestment.Dividend;
        investments.Add(newInvestment);
    }

    [Server]
    private void UpdateInvestment(int index, int oldDividend, int oldCapital)
    {
        if (index < 0 || index >= investments.Count) return;

        // Obtiene dividendo anual
        Investment currInvestment = investments[index];
        if (currInvestment.Dividend != 0) money.Value += currInvestment.Dividend;

        // Actualiza Capital y Proximo Dividendo
        currInvestment.UpdateInvestment();
        invest.Value += currInvestment.Capital - oldCapital;
        int nextDividend = currInvestment.Dividend - oldDividend;
        if (nextDividend != 0) income.Value += nextDividend;
        currInvestment.Turns--;
        if (currInvestment.Turns != 0)
        {
            investments.Dirty(index);
            return;
        }

        // Terminó la inversión
        money.Value += currInvestment.Capital;
        invest.Value -= currInvestment.Capital;
        investments.RemoveAt(index);
    }

    [ServerRpc]
    private void CmdAddExpense(Expense newExpense, bool withInterest)
    {
        if (withInterest)
        {
            newExpense.Amount += (int)(newExpense.Amount * interest);
            newExpense.Turns++;
        }
        debt.Value += newExpense.Amount * newExpense.Turns;
        expense.Value += newExpense.Amount;
        expenses.Add(newExpense);
    }

    [Server]
    private void UpdateExpense(int index, int interest)
    {
        if (index < 0 || index >= expenses.Count) return;

        // No tiene para pagar: Suma interes
        Expense currExpense = expenses[index];
        if (interest > 0)
        {
            currExpense.Amount += interest;
            currExpense.Turns++;
            debt.Value += interest * currExpense.Turns;
            expense.Value += interest;
            expenses.Dirty(index);
            return;
        }

        // Tiene para pagar: Paga la cuota
        money.Value -= currExpense.Amount;
        debt.Value -= currExpense.Amount;
        currExpense.Turns--;
        if (currExpense.Turns != 0)
        {
            expenses.Dirty(index);
            return;
        }

        // Terminó de pagar: Elimina la deuda
        expense.Value -= currExpense.Amount;
        expenses.RemoveAt(index);
    }

    #endregion

    #region Client Process Finances

    [Server]
    public void ProccessFinances()
    {
        ProcessSalary();
        ProcessInvestments();
        ProcessRecurrentExpenses();
    }

    [Server]
    private void ProcessSalary() => money.Value += salary.Value;

    [Server]
    private void ProcessInvestments()
    {
        if (investments.Count == 0) return;

        for (int i = investments.Count - 1; i >= 0; i--)
        {
            var invest = investments[i];
            UpdateInvestment(i, invest.Dividend, invest.Capital);
        }
    }

    [Server]
    private void ProcessRecurrentExpenses()
    {
        if (expenses.Count == 0) return;

        for (int i = expenses.Count - 1; i >= 0; i--)
        {
            var expense = expenses[i];
            if (money.Value >= expense.Amount)
                UpdateExpense(i, 0);
            else
                UpdateExpense(i, (int)(expense.Amount * interest));
        }
    }

    #endregion

    #region Client Actions

    void Update()
    {
        if (!IsOwner) return;

        // Accion: Lanzar dado
        if (canThrow && Input.GetKeyDown(KeyCode.Space))
        {
            canThrow = false;
            Throw();
        }
    }

    [Server]
    private void FinishTurn()
    {
       // AddCornerPosition();
        GameOnline.Instance.NextTurn();
    }

    #endregion

    //2. Configurar pregunta
    #region Server Question Setup

    [Server]
    public void CreateQuestion()
    {
        QuestionData question = data.GetRandomQuestion();
        questionIndex.Value = data.questions.IndexOf(question);
    }

    private void OnChangeQuestionIndex(int oldIndex, int newIndex, bool asServer)
    {
        StartCoroutine(SetupQuestion(newIndex));
    }

    private IEnumerator SetupQuestion(int index)
    {
        Debug.Log("Question Index: " + index);
        QuestionData question = data.questions[index];
        //ui.SetupQuestion(question, this, IsOwner);
        if (IsOwner)
        {
            Debug.Log("Owner: " + IsOwner);
            ui.OnQuestionAnswered += OnAnswerQuestion;
        }

        yield return new WaitUntil(() => questionAnswered.Value);
        ui.ShowQuestion(false);

        if (wasAnswerCorrect.Value)
        {
            CmdSubmitAnswer(false, false);
            EnableDice();
        }
        else
        {
            CmdSubmitAnswer(false, false);
            FinishTurn();
        }
    }

    private void OnAnswerQuestion(bool isCorrect)
    {
        Debug.Log("Respuesta seleccionada: " + isCorrect);
        ui.OnQuestionAnswered -= OnAnswerQuestion;
        CmdSubmitAnswer(true, isCorrect);
    }

    [ServerRpc]
    private void CmdSubmitAnswer(bool answered, bool isCorrect)
    {
        wasAnswerCorrect.Value = isCorrect;
        questionAnswered.Value = answered;
        if (answered) ui.ShowQuestion(false);
    }

    #endregion

    //3. Mostar dado
    #region Server Dice Setup

    private void EnableDice()
    {
        CmdActivateDice(true);

        if (!IsOwner) return;
        StartCoroutine(dice.RotateDiceRoutine());
        canThrow = true;
    }

    [ServerRpc]
    public void CmdActivateDice(bool active)
    {
        dice.ShowDice(active);
        RpcActivateDice(active);
    }

    [ObserversRpc]
    private void RpcActivateDice(bool active)
    {
        dice.ShowDice(active);
    }

    private void Throw()
    {
        animator.Jump(); //FIXME: Agregar animación de salto [OberserverRpc]
        StartCoroutine(ThrowDice());
    }

    private IEnumerator ThrowDice()
    {
        yield return dice.StopDice();
        CmdDisableDice();
        yield return new WaitForSeconds(1f);
        StartCoroutine(MovePlayer());
    }

    [ServerRpc]
    private void CmdDisableDice() => RpcDisableDice();

    [ObserversRpc]
    private void RpcDisableDice() => dice.ShowDice(false);

    #endregion

    //4. Jugar casilla
    #region Server Square Setup

    private IEnumerator MovePlayer()
    {
        yield return Move(dice.DiceRoll, Position);
        CmdCreateSquare();
    }

    [ServerRpc]
    private void CmdCreateSquare()
    {
        //1. Obtiene casilla
        Square square = game.Squares[Position];
        //2. Obtiene tarjetas
        List<CardBase> cards = square.GetCards();
        //3. Envia tarjetas

        this.cardsIndex.Clear();
        //cardsIndex.AddRange(cardsID);
    }

    private void OnChangeCardsList(SyncListOperation op, int index, int oldItem,
                                   int newItem, bool asServer)
    {
        if (asServer) return;

        switch (op)
        {
            case SyncListOperation.Add:
                break;
            case SyncListOperation.RemoveAt:
                break;
            case SyncListOperation.Insert:
                break;
            case SyncListOperation.Set:
                break;
            case SyncListOperation.Clear:
                break;
            case SyncListOperation.Complete:
                StartCoroutine(SetupSquare());
                break;
        }
    }

    private IEnumerator SetupSquare()
    {
        Square square = GameOnline.Instance.Squares[Position];
        square.SetupSquare(this);
        //if (IsOwner) UIOnline.Instance.OnCardSelected += OnAnswerSquare;

        yield return new WaitUntil(() => squareAnswered.Value);
        squareAnswered.Value = false;
        //UIOnline.Instance.CloseCards();

        FinishTurn();
    }

    private void OnAnswerSquare()
    {
        if (!IsOwner) return;
        //UIOnline.Instance.OnCardSelected -= OnAnswerSquare;
        CmdSubmitCard();
    }

    [ServerRpc]
    private void CmdSubmitCard()
    {
        squareAnswered.Value = true;
    }

    #endregion

    #region Client Movement

    // Initialize Move Player
    [ServerRpc]
    public void CmdInitializePosition()
    {
        //if (data.turnPlayer == index.Value) CenterPosition();
        //else AddCornerPosition();
    }

    // Move
    private IEnumerator Move(int steps, int currPosition)
    {
        direction = Vector2.zero;
        for (int i = 0; i < steps; i++)
        {
            // Avanzar la posición del jugador en el tablero
            currPosition++;
            if (currPosition >= game.Squares.Length) currPosition = 0;

            // Configurar la casilla de destino y su posición central
            Transform squareTransform = game.Squares[currPosition].transform;
            Vector3 positionCenterBox = squareTransform.position;
            RaycastHit hit;
            Vector3 rayStart = positionCenterBox + Vector3.up * 10;

            if (Physics.Raycast(rayStart, Vector3.down, out hit, Mathf.Infinity, groundLayerMask))
            {
                Vector3 destinyPosition = hit.point;
                Vector3 movementDirection = (destinyPosition - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);

                // Configurar animación de movimiento
                animator.SetMoving(0, 1);

                // Interpolación de movimiento hacia la siguiente casilla
                float time = 0f;
                Vector3 initialPosition = transform.position;
                while (time < 1f)
                {
                    time += Time.deltaTime * speedMovement;
                    transform.position = Vector3.Lerp(initialPosition, destinyPosition, time);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, time);
                    yield return null;
                }
            }
            else
            {
                Debug.LogError("No se encontró la superficie bajo la casilla.");
            }
        }

        animator.SetMoving(direction.x, direction.y); //FIXME: Activar en todos
        UpdatePosition(currPosition);
    }

    [ServerRpc] //FIXME: Revisar
    public void UpdatePosition(int newPosition)
    {
        position.Value = newPosition;
        data.playersData[index.Value].Position = newPosition;
    }

    // Center Position
    [Server]
    public void CenterPosition()
    {
        Transform squareTransform = game.Squares[Position].transform;
        Square currentSquare = game.Squares[Position];
        //currentSquare.RemovePlayer(this);

        // Posicionarse en el centro de la casilla
        Vector3 targetPosition = squareTransform.position;
        transform.position = targetPosition;
        transform.localScale = Vector3.one;

        // Calcular la orientación hacia la siguiente casilla
        int nextPosition = (Position + 1) % game.Squares.Length;
        Vector3 nextSquarePosition = game.Squares[nextPosition].transform.position;
        Vector3 directionToNext = (nextSquarePosition - targetPosition).normalized;

        // Ajustar la rotación para mirar hacia la siguiente casilla
        if (Mathf.Abs(directionToNext.x) > Mathf.Abs(directionToNext.z))
        {
            transform.rotation = directionToNext.x > 0 ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, -90, 0);
        }
        else
        {
            transform.rotation = directionToNext.z > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
        }
    }

    // Corner Position
    [Server]
    public void CornerPosition()
    {
        Transform squareTransform = game.Squares[Position].transform;
        Square currentSquare = game.Squares[Position];

        //int playerIndex = currentSquare.GetPlayerIndex(this);
        int totalPlayers = currentSquare.PlayersCount;
        Vector3 characterForwardDirection = transform.forward;

        Vector3 targetPosition = squareTransform.position;
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        //Ajustar en base a characterForwardDirection 
        if (Mathf.Abs(characterForwardDirection.x) > Mathf.Abs(characterForwardDirection.z))
        {
            targetPosition.z += (characterForwardDirection.z > 0) ? 1 : -1;
           // float positionX = CalculatePosition(totalPlayers, playerIndex);
            //targetPosition.x += positionX;
        }
        else
        {
            targetPosition.x += (characterForwardDirection.x > 0) ? 1 : -1;
            //float positionZ = CalculatePosition(totalPlayers, playerIndex);
            //targetPosition.z += positionZ;
        }
        transform.position = targetPosition;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RpcCornerPosition()
    {
        CornerPosition();
    }

    [Server]
    private float CalculatePosition(int totalPlayers, int playerIndex)
    {
        if (totalPlayers == 1) return 0f;
        else if (totalPlayers == 2) return (playerIndex == 0) ? -0.5f : 0.5f;
        else return -0.75f + 1.5f * playerIndex / (totalPlayers - 1);
    }

    //[Server]
    //private void AddCornerPosition() => game.Squares[Position].AddPlayer(this);

    #endregion

}
