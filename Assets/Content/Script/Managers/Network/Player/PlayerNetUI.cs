using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerNetUI : NetworkBehaviour
{
    [SerializeField] private UIPlayer ui;

    // Question
    [SyncVar(hook = nameof(SetupQuestion))] private QuestionData currentQuestion = null;
    private List<QuestionData> questions = new List<QuestionData>();
    private int levelQuestion = 0;
    private string topicQuestion;
    [SyncVar(hook = nameof(OnAttemptUpdated))] private int attempts = 2;
    private bool useBGames = false;
    [SyncVar(hook = nameof(OnTimerUpdated))] private float timeRemaining = 15f;

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

        StartQuestionTimer();
    }

    [Server]
    private void GetQuestionsTopic()
    {
        if (levelQuestion == 0 || topicQuestion == null)
        {
            levelQuestion = GetComponent<PlayerNetData>().Level;
            topicQuestion = GameNetManager.Data.GetRandomTopicQuestions(levelQuestion);
        }
        questions = GameNetManager.Data.GetQuestionsByTopic(topicQuestion, levelQuestion);
    }

    private void SetupQuestion(QuestionData oldQuestion, QuestionData newQuestion)
    {
        ui.ShowQuestion(false);
        if (newQuestion == null) return;
        ui.SetupQuestion(newQuestion, attempts, isOwned);
        if (isOwned) ui.OnQuestionAnswered += OnAnswerQuestion;
    }

    private void OnAnswerQuestion(bool isCorrect)
    {
        ui.OnQuestionAnswered -= OnAnswerQuestion;
        CmdSubmitAnswer(isCorrect);
    }

    [Server]
    private void ResetQuestionValues()
    {
        levelQuestion = 0;
        topicQuestion = null;
        attempts = 2;
        currentQuestion = null;
        questions.Clear();
    }

    [Command]
    private void CmdSubmitAnswer(bool isCorrect)
    {
        SubmitAnswer(isCorrect);
    }

    [Server]
    private void SubmitAnswer(bool isCorrect)
    {
        StopCoroutine(nameof(QuestionTimer));

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
        timeRemaining = 15f;
        StartCoroutine(nameof(QuestionTimer));
    }

    [Server]
    private IEnumerator QuestionTimer()
    {
        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        SubmitAnswer(false);
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
        ui.ShowCards(data.Money);
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
        if (indexCard >= 0)
        {
            Card selectedCard = selectedCards[indexCard];
            int capital = 0;
            if (selectedCard is InvestmentCard) capital = amountInvest;
            selectedCard.ApplyEffect(capital, false);
        }
        selectedCards.Clear();
        RpcCloseCards();
    }

    [ClientRpc]
    private void RpcCloseCards()
    {
        ui.CloseCards();
        if (isOwned) CmdFinishTurn();
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

    [Command]
    private void CmdFinishTurn()
    {
        GetComponent<PlayerNetManager>().FinishTurn();
    }

    private void FinishTurn()
    {
        GetComponent<PlayerNetManager>().FinishTurn();
    }

    #endregion

}
