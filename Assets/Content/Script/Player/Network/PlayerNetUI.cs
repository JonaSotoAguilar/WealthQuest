using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerNetUI : NetworkBehaviour
{
    [SerializeField] private UIPlayer ui;

    // Question
    [SyncVar(hook = nameof(SetupQuestion))] private QuestionData currentQuestion;

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
        QuestionData questionData = GameNetManager.Data.GetRandomQuestion();
        currentQuestion = questionData;
    }

    private void SetupQuestion(QuestionData oldQuestion, QuestionData newQuestion)
    {
        ui.SetupQuestion(currentQuestion, isOwned);
        if (isOwned) ui.OnQuestionAnswered += OnAnswerQuestion;
    }

    private void OnAnswerQuestion(bool isCorrect)
    {
        ui.OnQuestionAnswered -= OnAnswerQuestion;
        CmdSubmitAnswer(isCorrect);
    }

    [Command]
    private void CmdSubmitAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            GetComponent<PlayerNetData>().AddPoints(currentQuestion.level);
            GameNetManager.Data.DeleteQuestion(currentQuestion);
        }

        CloseQuestion(isCorrect);
    }

    [ClientRpc]
    private void CloseQuestion(bool isCorrect)
    {
        ui.ShowQuestion(false);
        if (!isOwned) return;

        if (isCorrect)
        {
            DiceRoll();
        }
        else
        {
            CmdFinishTurn();
        }
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
        ui.SetupCard(selectedCards[index], data.Points, data.Money);

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

    #endregion
}
