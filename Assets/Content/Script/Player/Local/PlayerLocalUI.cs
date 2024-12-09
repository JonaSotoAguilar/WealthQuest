using System.Collections.Generic;
using UnityEngine;

public class PlayerLocalUI : MonoBehaviour
{
    [SerializeField] private UIPlayer ui;

    // Question
    private int levelQuestion = 0;
    private string topicQuestion;
    private int attempts = 3;
    private List<QuestionData> questions = new List<QuestionData>();
    private QuestionData currentQuestion;

    // Cards
    private List<Card> selectedCards = new List<Card>();

    #region Question

    public void CreateQuestion()
    {
        if (questions.Count == 0) GetQuestionsTopic();

        int index = Random.Range(0, questions.Count);
        currentQuestion = questions[index];

        ui.SetupQuestion(currentQuestion, true);
        ui.OnQuestionAnswered += OnAnswerQuestion;
    }

    private void GetQuestionsTopic()
    {
        if (levelQuestion == 0 || topicQuestion == null)
        {
            levelQuestion = GetComponent<PlayerLocalData>().Level;
            topicQuestion = GameLocalManager.Data.GetRandomTopicQuestions(levelQuestion);
        }
        questions = GameLocalManager.Data.GetQuestionsByTopic(topicQuestion, levelQuestion);
    }

    private void OnAnswerQuestion(bool isCorrect)
    {
        ui.OnQuestionAnswered -= OnAnswerQuestion;
        ui.ShowQuestion(false);

        // FIXME: Agregar Ui animation
        if (isCorrect)
        {
            ResetQuestionValues();
            GetComponent<PlayerLocalData>().AddPoints(currentQuestion.level);
            GameLocalManager.Data.DeleteQuestion(currentQuestion);
            DiceRoll();
        }
        else
        {
            attempts--;
            if (attempts <= 0)
            {
                ResetQuestionValues();
                FinishTurn();
            }
            else
            {
                questions.Remove(currentQuestion);
                CreateQuestion();
            }
        }
    }

    private void ResetQuestionValues()
    {
        //FIXME: bGames te da un intento extra

        levelQuestion = 0;
        topicQuestion = null;
        attempts = 3;
        currentQuestion = null;
        questions.Clear();
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

    private void DiceRoll()
    {
        GetComponent<PlayerLocalManager>().DiceRoll(true);
    }

    private void FinishTurn()
    {
        GetComponent<PlayerLocalManager>().FinishTurn();
    }

    #endregion

}
