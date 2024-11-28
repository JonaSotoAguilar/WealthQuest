using System.Collections.Generic;
using UnityEngine;

public class PlayerLocalUI : MonoBehaviour
{
    [SerializeField] private UIPlayer ui;

    // Question
    private QuestionData currentQuestion;

    // Cards
    private List<Card> selectedCards = new List<Card>();

    #region Question

    public void CreateQuestion()
    {
        QuestionData questionData = GameLocalManager.Data.GetRandomQuestion();
        currentQuestion = questionData;
        ui.SetupQuestion(currentQuestion, true);
        ui.OnQuestionAnswered += OnAnswerQuestion;
    }

    private void OnAnswerQuestion(bool isCorrect)
    {
        ui.OnQuestionAnswered -= OnAnswerQuestion;

        if (isCorrect)
        {
            GetComponent<PlayerLocalData>().AddPoints(currentQuestion.scoreForCorrectAnswer);
            GameLocalManager.Data.DeleteQuestion(currentQuestion);
        }

        CloseQuestion(isCorrect);
    }

    private void CloseQuestion(bool isCorrect)
    {
        ui.ShowQuestion(false);

        if (isCorrect)
        {
            GetComponent<PlayerLocalManager>().DiceRoll(true);
        }
        else
        {
            FinishTurn();
        }
    }

    #endregion

    #region Cards

    public void SetupCards(Square square)
    {
        PlayerLocalData data = GetComponent<PlayerLocalData>();
        List<Card> cards = square.GetCards();
        foreach (var card in cards)
        {
            selectedCards.Add(card);
            ui.SetupCard(card, data.Points, data.Money);
        }

        ui.ShowCards(data.Money);
        ui.OnCardSelected += OnCardSelected;
    }

    private void OnCardSelected(Card card)
    {
        ui.OnCardSelected -= OnCardSelected;
        int index = -1;
        if (card != null) index = selectedCards.IndexOf(card);
        SubmitCard(index);
    }

    private void SubmitCard(int indexCard)
    {
        if (indexCard >= 0)
        {
            Card selectedCard = selectedCards[indexCard];
            int capital = 0;
            if (selectedCard is InvestmentCard) capital = ui.AmountInvest;
            selectedCard.ApplyEffect(capital);
        }
        selectedCards.Clear();
        ui.CloseCards();
        FinishTurn();
    }

    #endregion

    #region Turn

    private void FinishTurn()
    {
        GetComponent<PlayerLocalManager>().FinishTurn();
    }

    #endregion
}
