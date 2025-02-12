using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;

public class PlayerNetUI : NetworkBehaviour
{
    [SerializeField] private UIPlayer ui;

    // Question
    [SyncVar(hook = nameof(SetupQuestion))] private Question currentQuestion = null;
    private List<Question> questions = new List<Question>();
    private int levelQuestion;
    [SyncVar(hook = nameof(OnAttemptUpdated))] private int attempts = 2;
    private bool useBGames = false;
    [SyncVar(hook = nameof(OnTimerUpdated))] private float timeRemaining = 30f;
    private int readyPlayer = 0;

    // Cards
    private readonly SyncList<Card> selectedCards = new SyncList<Card>();
    [SyncVar(hook = nameof(ChangeAmountInvest))] private int amountInvest = 0;

    private void OnDestroy()
    {
        ui.OnQuestionAnswered -= OnAnswerQuestion;
        ui.OnAttemptFinished -= OnAttemptFinished;
        ui.OnCardSelected -= OnCardSelected;
        selectedCards.OnAdd -= OnCardAdded;
        StopAllCoroutines();
    }

    public override void OnStartClient()
    {
        base.OnStartServer();

        selectedCards.OnAdd += OnCardAdded;

        if (!isOwned)
        {
            ui.DesactiveCanvaGroup();
            ui.NotOwnerInvest();
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        selectedCards.OnAdd -= OnCardAdded;
    }

    #region Question

    [Server]
    public void CreateQuestion()
    {
        if (questions.Count == 0) GetQuestionsTopic();

        int index = Random.Range(0, questions.Count);
        currentQuestion = questions[index];
        levelQuestion = currentQuestion.level;

        StartQuestionTimer();
    }

    [Server]
    private void GetQuestionsTopic()
    {
        int level = GetComponent<PlayerNetData>().Level;
        questions = GameNetManager.Data.GetQuestionsByLevel(level);
    }

    private void SetupQuestion(Question oldQuestion, Question newQuestion)
    {
        ui.ShowQuestion(false);
        if (newQuestion == null) return;
        ui.SetupQuestion(newQuestion, attempts, isOwned);
        if (isOwned) ui.OnQuestionAnswered += OnAnswerQuestion;
    }

    [Server]
    private void ResetQuestionValues()
    {
        attempts = 2;
        currentQuestion = null;
        questions.Clear();
    }

    private void OnAnswerQuestion(int index, bool isCorrect)
    {
        ui.OnQuestionAnswered -= OnAnswerQuestion;
        CmdSubmitAnswer(index, isCorrect);
    }

    [Command]
    private void CmdSubmitAnswer(int index, bool isCorrect)
    {
        FinishAnswer(index, isCorrect);
    }

    [Server]
    private void FinishAnswer(int index, bool isCorrect)
    {
        RpcStopSoundTimer();
        StopCoroutine(nameof(QuestionTimer));

        if (index < 0)
        {
            SubmitAnswer(false);
        }
        else
        {
            RpcAnsweredQuestion(index, isCorrect);
        }
    }

    [ClientRpc]
    private void RpcAnsweredQuestion(int index, bool isCorrect)
    {
        _ = TaskRpcAnsweredQuestion(index, isCorrect);
    }

    private async Task TaskRpcAnsweredQuestion(int index, bool isCorrect)
    {
        await ui.AnsweredQuestion(index, isCorrect);
        CmdFinishAnsweredQuestion(isCorrect);
    }

    [Command(requiresAuthority = false)]
    private void CmdFinishAnsweredQuestion(bool isCorrect)
    {
        readyPlayer++;

        if (GameNetManager.Players.Count == readyPlayer)
        {
            readyPlayer = 0;
            SubmitAnswer(isCorrect);
        }
    }

    [Server]
    private void SubmitAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            GetComponent<PlayerNetData>().AddPoints(levelQuestion);
            GameNetManager.Data.DeleteQuestion(currentQuestion);
            ResetQuestionValues();
            DiceRoll();
        }
        else
        {
            attempts--;
            if (attempts <= 0)
            {
                if (CanPlayBGames())
                {
                    AddAttempt();
                }
                else
                {
                    ResetQuestionValues();
                    FinishTurn();
                }
            }
            else
            {
                questions.Remove(currentQuestion);
                CreateQuestion();
            }
        }
    }

    [Server]
    private void StartQuestionTimer()
    {
        if (timeRemaining > 0) StopCoroutine(nameof(QuestionTimer));
        timeRemaining = 30f;
        StartCoroutine(nameof(QuestionTimer));
    }

    [Server]
    private IEnumerator QuestionTimer()
    {
        RpcSoundTimer();
        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        FinishAnswer(-1, false);
    }

    private void OnTimerUpdated(float oldTime, float newTime)
    {
        ui.UpdateTimerDisplay(Mathf.CeilToInt(newTime));
    }

    #endregion

    #region Attempts

    private void OnAttemptUpdated(int oldAttempts, int newAttempts)
    {
        ui.UpdateAttempts(newAttempts);
    }

    [Server]
    private bool CanPlayBGames()
    {
        if (useBGames || ProfileUser.bGamesProfile == null || ProfileUser.bGamesProfile.points <= 0) return false;
        return true;
    }

    [Server]
    private void AddAttempt()
    {
        useBGames = true;
        RpcSetupBGames(true);
    }

    [ClientRpc]
    private void RpcSetupBGames(bool show)
    {
        ui.ShowAttempts(show);
        if (isOwned && show) ui.OnAttemptFinished += OnAttemptFinished;
    }

    private async void OnAttemptFinished(bool isYes)
    {
        ui.OnAttemptFinished -= OnAttemptFinished;
        if (isYes)
        {
            bool success = await HttpService.SpendPoints(1);
            CmdOnAttemptFinished(success);
        }
        else
        {
            CmdFinishQuestion();
        }
    }

    [Command]
    private void CmdOnAttemptFinished(bool success)
    {
        RpcSetupBGames(false);
        if (success)
        {
            attempts++;
            questions.Remove(currentQuestion);
            CreateQuestion();
        }
        else
        {
            ResetQuestionValues();
            FinishTurn();
        }
    }

    [Command]
    private void CmdFinishQuestion()
    {
        ResetQuestionValues();
        FinishTurn();
    }

    #endregion

    #region Cards

    [Server]
    public void SetupCards(Square square)
    {
        amountInvest = ui.MinInvestment;
        List<Card> cards = square.GetCards();
        foreach (var card in cards)
            selectedCards.Add(card);
    }

    private void OnCardAdded(int index)
    {
        PlayerNetData data = GetComponent<PlayerNetData>();
        ui.SetupCard(selectedCards[index], data.Points);

        if (!ui.ActiveCards()) ShowCards();
    }

    private void ShowCards()
    {
        PlayerNetData data = GetComponent<PlayerNetData>();
        ui.ShowCards(data.Money, isOwned);
        if (isOwned) ui.OnCardSelected += OnCardSelected;
    }

    private void OnCardSelected(Card card)
    {
        ui.OnCardSelected -= OnCardSelected;
        int index = -1;
        if (card != null) index = selectedCards.IndexOf(card);
        CmdSubmitCard(index);
    }

    [Command]
    private void CmdSubmitCard(int indexCard)
    {
        CardSelected(indexCard);
    }

    [Server]
    private void CardSelected(int index)
    {
        if (index >= 0)
        {
            Card selectedCard = selectedCards[index];
            int capital = 0;
            if (selectedCard is InvestmentCard) capital = amountInvest;
            selectedCard.ApplyEffect(capital, false);
        }

        RpcCardSelected(index);
    }

    [ClientRpc]
    private void RpcCardSelected(int index)
    {
        _ = TaskCardSelected(index);
    }

    private async Task TaskCardSelected(int index)
    {
        await ui.CardSelected(index);
        CmdFinishCard();
    }

    [Command(requiresAuthority = false)]
    private void CmdFinishCard()
    {
        readyPlayer++;

        if (GameNetManager.Players.Count == readyPlayer)
        {
            readyPlayer = 0;
            SubmitCard();
        }
    }

    [Server]
    private void SubmitCard()
    {
        selectedCards.Clear();

        FinishTurn();
    }

    [ClientRpc]
    private void RpcCloseCards()
    {
        ui.CloseCards();
    }

    #endregion

    #region Investment Card

    public void IncreaseAmount()
    {
        int amountChange = ui.AmountChange;
        int moneyPlayer = GetComponent<PlayerNetData>().Money;
        if (amountInvest + amountChange <= moneyPlayer)
        {
            CmdChangeAmountInvest(amountInvest + amountChange);
        }
    }

    public void LowerAmount()
    {
        int amountChange = ui.AmountChange;
        int minInvestment = ui.MinInvestment;
        if (amountInvest - amountChange >= minInvestment)
        {
            CmdChangeAmountInvest(amountInvest - amountChange);
        }
    }

    [Command]
    private void CmdChangeAmountInvest(int newAmount)
    {
        amountInvest = newAmount;
    }

    private void ChangeAmountInvest(int oldAmount, int newAmount)
    {
        ui.ChangeAmountInvest(newAmount);
    }

    #endregion

    #region Turn

    [Server]
    private void DiceRoll()
    {
        GetComponent<PlayerNetManager>().EnableDice(true);
    }

    private void FinishTurn()
    {
        GetComponent<PlayerNetManager>().FinishTurn();
    }

    #endregion

    #region Audio


    [ClientRpc]
    private void RpcSoundTimer()
    {
        AudioManager.PlaySoundTimer();
    }

    [ClientRpc]
    private void RpcStopSoundTimer()
    {
        AudioManager.StopSoundSFX();
    }

    #endregion

}
