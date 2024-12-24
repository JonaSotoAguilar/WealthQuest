using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerNetUI : NetworkBehaviour
{
    [SerializeField] private UIPlayer ui;

    // Question
    [SyncVar(hook = nameof(SetupQuestion))] private QuestionData currentQuestion;
    private List<QuestionData> questions = new List<QuestionData>();
    private int levelQuestion = 0;
    private string topicQuestion;
    [SyncVar] private int attempts = 2;
    private bool useBGames = false;

    // Cards
    private readonly SyncList<Card> selectedCards = new SyncList<Card>();
    [SyncVar(hook = nameof(ChangeAmountInvest))] private int amountInvest = 0;

    public override void OnStartClient()
    {
        base.OnStartServer();

        selectedCards.OnAdd += OnCardAdded;
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
    }

    private void GetQuestionsTopic()
    {
        if (levelQuestion == 0 || topicQuestion == null)
        {
            levelQuestion = GetComponent<PlayerNetData>().Level;
            topicQuestion = GameLocalManager.Data.GetRandomTopicQuestions(levelQuestion);
        }
        questions = GameLocalManager.Data.GetQuestionsByTopic(topicQuestion, levelQuestion);
    }



    private void SetupQuestion(QuestionData oldQuestion, QuestionData newQuestion)
    {
        ui.SetupQuestion(currentQuestion, attempts, isOwned);
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
        //FIXME: bGames te da un intento extra

        levelQuestion = 0;
        topicQuestion = null;
        attempts = 2;
        currentQuestion = null;
        questions.Clear();
    }

    [ClientRpc]
    private void RpcCloseQuestion(bool isCorrect)
    {
        ui.ShowQuestion(false);
        if (isOwned && isCorrect) DiceRoll();
    }

    [Command]
    private void CmdSubmitAnswer(bool isCorrect)
    {
        RpcCloseQuestion(true);
        if (isCorrect)
        {
            GetComponent<PlayerNetData>().AddPoints(levelQuestion);
            GameNetManager.Data.DeleteQuestion(currentQuestion);
            ResetQuestionValues();
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
    private bool CanPlayBGames()
    {
        if (useBGames || ProfileUser.BGamesProfile == null || ProfileUser.BGamesProfile.points <= 0) return false;
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
            bool success = await HttpService.SpendPoints();
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

    private void DiceRoll()
    {
        GetComponent<PlayerLocalManager>().DiceRoll(true);
    }

    [Command]
    private void CmdFinishTurn()
    {
        GetComponent<PlayerNetManager>().FinishTurn();
    }

    private void FinishTurn()
    {
        GetComponent<PlayerLocalManager>().FinishTurn();
    }

    #endregion
}
