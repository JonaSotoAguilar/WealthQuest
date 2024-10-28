using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCanvas playerCanvas;
    [SerializeField] private PlayerDice playerDice;
    [SerializeField] private Animator playerAnimator;

    [Header("Player HUD")]
    [SerializeField] private PlayerHUD playerHUD;
    private CultureInfo chileanCulture = new CultureInfo("es-CL");

    public PlayerData PlayerData { get => playerData; }
    public PlayerHUD PlayerHUD { get => playerHUD; set => playerHUD = value; }
    public PlayerCanvas PlayerCanvas { get => playerCanvas; }
    public PlayerMovement PlayerMovement { get => playerMovement;}

    public void InitializePlayer(PlayerData assignedPlayer, PlayerInput input)
    {
        playerData = assignedPlayer;
        playerInput = input;

        playerMovement = GetComponent<PlayerMovement>();
        playerCanvas = GetComponentInChildren<PlayerCanvas>();
        playerDice = GetComponentInChildren<PlayerDice>();
        playerAnimator = GetComponentInChildren<Animator>();

        playerDice.ShowDice(false);
        playerMovement.PlayerAnimator = playerAnimator;
        playerMovement.InitPosition(playerData.CurrentPosition);

        GameManager.Instance.Players.Add(this);
    }

    public IEnumerator InitQuestion()
    {
        if (GameManager.Instance.GameData.QuestionList.Count == 0)
            yield return GameManager.Instance.GameData.ResetQuestionList();
        QuestionData selectedQuestion = GameManager.Instance.GameData.GetRandomQuestion();

        // Configurar el panel de preguntas
        QuestionPanel panel = playerCanvas.QuestionPanel;
        panel.SetupQuestion(selectedQuestion, this);
        bool questionAnswered = false;
        bool wasAnswerCorrect = false;

        // Subscribirse al evento de respuesta
        Action<bool> onQuestionAnswered = (isCorrect) =>
        {
            questionAnswered = true;
            wasAnswerCorrect = isCorrect;
        };
        panel.OnQuestionAnswered += onQuestionAnswered;
        yield return new WaitUntil(() => questionAnswered);
        panel.OnQuestionAnswered -= onQuestionAnswered;

        if (wasAnswerCorrect) EnableDice();
        else FinishTurn();
    }

    private void EnableDice()
    {
        playerDice.ShowDice(true);
        playerInput.SwitchCurrentActionMap("Player");
    }

    private IEnumerator Jump()
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
    private IEnumerator ThrowDice()
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
        Square square = GameManager.Instance.SquareList[playerData.CurrentPosition].GetComponent<Square>();
        yield return square.ActiveSquare(this);
        FinishTurn();
    }

    private void FinishTurn()
    {
        playerMovement.CornerPosition(playerData.CurrentPosition);
        StartCoroutine(GameManager.Instance.UpdateTurn());
    }

    // TODO: Uupdate HUD and PlayerData

    public void ChangeMoney(int amount)
    {
        playerData.Money += amount;
        playerHUD.Money.text = playerData.Money.ToString("C0", chileanCulture);
    }

    public void ChangeKFP(int score)
    {
        playerData.ScoreKFP += score;
        playerHUD.Kpf.text = playerData.ScoreKFP.ToString();
    }

    public void ChangeDebt(int amount)
    {
        playerData.Debt += amount;
        playerHUD.Debt.text = playerData.Debt.ToString("C0", chileanCulture);
    }

    public void ChangeInvest(int amount)
    {
        playerData.Invest += amount;
        playerHUD.Invest.text = playerData.Invest.ToString("C0", chileanCulture);
    }

    public void ChangeIncome(int amount)
    {
        playerData.IncomeTurn += amount;
        playerHUD.Income.text = playerData.IncomeTurn.ToString("C0", chileanCulture);
    }

    public void ChangeExpense(int amount)
    {
        playerData.ExpenseTurn += amount;
        playerHUD.Expense.text = playerData.ExpenseTurn.ToString("C0", chileanCulture);
    }

    public void ChangeSalary(int newSalary)
    {
        playerData.IncomeTurn += newSalary - playerData.Salary;
        playerData.Salary = newSalary;
        playerHUD.Income.text = playerData.IncomeTurn.ToString("C0", chileanCulture);
    }

    public void CreateInvestment(PlayerInvestment investment)
    {
        if (playerData.Money < investment.Capital)
            return;
        playerData.Investments.Add(investment);
        ChangeMoney(-investment.Capital);
        ChangeInvest(investment.Capital);
        ChangeIncome(investment.Dividend);
    }

    public void CreateExpense(PlayerExpense expense, bool isRecurrent)
    {
        if (isRecurrent)
        {
            playerData.Expenses.Add(expense);
            ChangeDebt(expense.Amount * expense.Turns);
            ChangeExpense(expense.Amount);
        }
        else
        {
            if (playerData.Money >= expense.Amount)
                ChangeMoney(-expense.Amount);
            else
            {
                int interestMount = (int)(expense.Amount * 0.1f);
                expense.Amount += interestMount;
                expense.Turns++;
                playerData.Expenses.Add(expense);
                ChangeDebt(interestMount * expense.Turns);
                ChangeExpense(interestMount);
            }
        }
    }

    // TODO: Método para procesar datos financieros

    public void ProcessIncome()
    {
        ChangeMoney(playerData.IncomeTurn);
    }

    public void ProcessInvestments()
    {
        if (playerData.Investments.Count == 0)
            return;
        List<PlayerInvestment> toRemove = new List<PlayerInvestment>();
        foreach (var investment in playerData.Investments)
        {
            if (investment.Turns == 0)
            {
                ChangeIncome(-investment.Dividend);
                ChangeMoney(investment.Capital);
                ChangeInvest(-investment.Capital);
                toRemove.Add(investment);
            }
            else
            {
                int beforeDividend = investment.Dividend;
                int beforeCapital = investment.Capital;
                investment.UpdateInvestment();
                investment.Turns--;
                ChangeInvest(investment.Capital - beforeCapital);
                ChangeIncome(investment.Dividend - beforeDividend);
            }
        }

        foreach (var item in toRemove)
        {
            playerData.Investments.Remove(item);
        }
    }

    public void ProcessRecurrentExpenses()
    {
        if (playerData.Expenses.Count == 0)
            return;

        List<PlayerExpense> toRemove = new List<PlayerExpense>();

        foreach (var expense in playerData.Expenses)
        {
            if (playerData.Money >= expense.Amount)
            {
                ChangeMoney(-expense.Amount);
                ChangeDebt(-expense.Amount);
                expense.Turns--;
                if (expense.Turns == 0)
                {
                    ChangeExpense(-expense.Amount);
                    toRemove.Add(expense);
                }
            }
            else
            {
                int interestMount = (int)(expense.Amount * 0.05f);
                expense.Amount += interestMount;
                expense.Turns++;
                ChangeDebt(interestMount * expense.Turns);
                ChangeExpense(interestMount);
            }
        }

        foreach (var item in toRemove)
        {
            playerData.Expenses.Remove(item);
        }
    }
}
