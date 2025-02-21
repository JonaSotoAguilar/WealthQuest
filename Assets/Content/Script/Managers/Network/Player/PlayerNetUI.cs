using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;

public class PlayerNetUI : NetworkBehaviour
{
    [SerializeField] private UIPlayer ui;

    // Question
    [SyncVar(hook = nameof(SetupQuestion))] private Question currentQuestion;
    private List<Question> questions = new List<Question>();
    private int levelQuestion;
    [SyncVar] private int attempts = 2;
    [SyncVar(hook = nameof(OnTimerUpdated))] private float timeRemaining = 30f;
    private int readyPlayer = 0;
    private Coroutine questionTimerCoroutine;

    // Cards
    private readonly SyncList<Card> selectedCards = new SyncList<Card>();
    [SyncVar(hook = nameof(ChangeAmountInvest))] private int amountInvest;

    private void OnDestroy()
    {
        ui.OnQuestionAnswered -= OnAnswerQuestion;
        ui.OnCardSelected -= OnCardSelected;
        selectedCards.OnAdd -= OnCardAdded;
        StopAllCoroutines();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        selectedCards.OnAdd += OnCardAdded;
        ui.RemoveLocalListeners();

        if (isOwned)
        {
            ui.amountText.onEndEdit.AddListener(ValidateAmountInput);
        }
        else
        {
            ui.IsNotOwner();
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
    private void GetQuestionsTopic()
    {
        int level = GetComponent<PlayerNetData>().Level;
        questions = GameNetManager.Data.GetQuestionsByLevel(level);
    }

    [Server]
    public void CreateQuestion()
    {
        currentQuestion = null;
        StartCoroutine(NewQuestion());
    }

    private IEnumerator NewQuestion()
    {
        yield return new WaitForSeconds(0.2f);

        if (questions.Count == 0) GetQuestionsTopic();
        int index = Random.Range(0, questions.Count);
        currentQuestion = questions[index];
        levelQuestion = currentQuestion.level;
        questionTimerCoroutine = StartCoroutine(QuestionTimer());
    }

    private void SetupQuestion(Question oldQuestion, Question newQuestion)
    {
        ui.CloseQuestion();
        if (newQuestion == null) return;

        ui.SetupQuestion(newQuestion, attempts);
        ui.OnQuestionAnswered -= OnAnswerQuestion;
        if (isOwned) ui.OnQuestionAnswered += OnAnswerQuestion;
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
        StopTimer();

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
            attempts = attempts - 1;
            if (attempts <= 0)
            {
                ResetQuestionValues();
                FinishTurn();
            }
            else
            {
                if (questions.Count > 1) questions.Remove(currentQuestion);
                CreateQuestion();
            }
        }
    }

    [Server]
    private void ResetQuestionValues()
    {
        attempts = 2;
        currentQuestion = null;
        questions.Clear();
    }

    #endregion

    #region Timer

    [Server]
    private IEnumerator QuestionTimer()
    {
        timeRemaining = 30f;
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

    [Server]
    private void StopTimer()
    {
        RpcStopSoundTimer();

        // Detener el temporizador
        if (questionTimerCoroutine != null)
        {
            StopCoroutine(questionTimerCoroutine);
            questionTimerCoroutine = null;
        }
    }

    #endregion

    #region Attempts

    // [Server]
    // private bool CanPlayBGames()
    // {
    //     if (useBGames || ProfileUser.bGamesProfile == null || ProfileUser.bGamesProfile.points <= 0) return false;
    //     return true;
    // }

    // [Server]
    // private void AddAttempt()
    // {
    //     useBGames = true;
    //     RpcSetupBGames();
    // }

    // [ClientRpc]
    // private void RpcSetupBGames()
    // {
    //     if (!isOwned) return;
    //     ui.ShowMoreAttempts();
    //     ui.OnAttemptFinished += OnAttemptFinished;
    // }

    // private async void OnAttemptFinished(bool isYes)
    // {
    //     ui.CloseMoreAttempts();
    //     ui.OnAttemptFinished -= OnAttemptFinished;
    //     if (isYes)
    //     {
    //         bool success = await HttpService.SpendPoints(1);
    //         CmdOnAttemptFinished(success);
    //     }
    //     else
    //     {
    //         CmdFinishQuestion();
    //     }
    // }

    // [Command]
    // private void CmdOnAttemptFinished(bool success)
    // {
    //     if (success)
    //     {
    //         attempts = 1;
    //         if (questions.Count > 1) questions.Remove(currentQuestion);
    //         CreateQuestion();
    //     }
    //     else
    //     {
    //         ResetQuestionValues();
    //         FinishTurn();
    //     }
    // }

    // [Command]
    // private void CmdFinishQuestion()
    // {
    //     ResetQuestionValues();
    //     FinishTurn();
    // }

    #endregion

    #region Cards

    [Server]
    public void SetupCards(Square square)
    {
        amountInvest = 0;
        List<Card> cards = square.GetCards();
        foreach (var card in cards)
            selectedCards.Add(card);
    }

    private void OnCardAdded(int index)
    {
        PlayerNetData data = GetComponent<PlayerNetData>();
        ui.SetupCard(selectedCards[index], data.Points);

        if (selectedCards.Count == 2) ShowCards();
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
        else
        {
            CmdChangeAmountInvest(moneyPlayer);
        }
    }

    public void LowerAmount()
    {
        int amountChange = ui.AmountChange;
        if (amountInvest - amountChange > 0)
        {
            CmdChangeAmountInvest(amountInvest - amountChange);
        }
        else
        {
            CmdChangeAmountInvest(0);
        }
    }

    private void ValidateAmountInput(string input)
    {
        ParseAmount();
    }

    private void ParseAmount()
    {
        int moneyPlayer = GetComponent<PlayerNetData>().Money;
        string amount = ui.amountText.text.Replace("$", "").Replace(".", "").Trim();

        if (int.TryParse(amount, out int parsedAmount))
        {
            int amountValue = Mathf.Clamp(parsedAmount, 0, moneyPlayer);
            ui.ChangeAmountInvest(amountValue);
            CmdChangeAmountInvest(amountValue);
        }
        else
        {
            ui.ChangeAmountInvest(0);
            CmdChangeAmountInvest(0);
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
