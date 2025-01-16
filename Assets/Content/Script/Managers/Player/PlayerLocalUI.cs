using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;

public class PlayerLocalUI : MonoBehaviour
{
    [SerializeField] private UIPlayer ui;

    // Question
    private int levelQuestion = 0;
    private string topicQuestion;
    private int attempts = 2;
    private List<QuestionData> questions = new List<QuestionData>();
    private QuestionData currentQuestion;
    private Coroutine questionTimerCoroutine;

    bool useBGames = false;

    public UIPlayer UI { get => ui; }

    // Cards
    private List<Card> selectedCards = new List<Card>();

    #region Question

    public void OnDestroy()
    {
        ui.OnQuestionAnswered -= OnAnswerQuestion;
        ui.OnAttemptFinished -= OnAttemptFinished;
        ui.OnCardSelected -= OnCardSelected;
        StopAllCoroutines();
    }

    public void CreateQuestion()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"{gameObject.name} está inactivo. No se puede iniciar la pregunta.");
            return;
        }

        if (questions.Count == 0) GetQuestionsTopic();

        int index = Random.Range(0, questions.Count);
        currentQuestion = questions[index];

        ui.SetupQuestion(currentQuestion, attempts, true);
        ui.OnQuestionAnswered += OnAnswerQuestion;

        questionTimerCoroutine = StartCoroutine(QuestionTimer());
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

    private void ResetQuestionValues()
    {
        useBGames = false;
        levelQuestion = 0;
        topicQuestion = null;
        attempts = 2;
        currentQuestion = null;
        questions.Clear();
    }

    private void OnAnswerQuestion(bool isCorrect)
    {
        ui.OnQuestionAnswered -= OnAnswerQuestion;

        // Detener el temporizador
        if (questionTimerCoroutine != null)
        {
            StopCoroutine(questionTimerCoroutine);
            questionTimerCoroutine = null;
        }

        ui.ShowQuestion(false);

        if (isCorrect)
        {
            GetComponent<PlayerLocalData>().AddPoints(levelQuestion);
            GameLocalManager.Data.DeleteQuestion(currentQuestion);
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

    private IEnumerator QuestionTimer()
    {
        float timeRemaining = 15f;
        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            // Puedes actualizar la UI del temporizador aquí, si tienes una
            ui.UpdateTimerDisplay(Mathf.CeilToInt(timeRemaining));

            yield return null;
        }

        // Si se agota el tiempo, se considera respuesta incorrecta
        OnAnswerQuestion(false);
    }

    #endregion

    #region Attempts

    private bool CanPlayBGames()
    {
        if (useBGames || ProfileUser.bGamesProfile == null || ProfileUser.bGamesProfile.points <= 0) return false;
        return true;
    }

    private void AddAttempt()
    {
        useBGames = true;
        ui.ShowAttempts(true);
        ui.OnAttemptFinished += OnAttemptFinished;
    }

    private async void OnAttemptFinished(bool isYes)
    {
        ui.ShowAttempts(false);
        ui.OnAttemptFinished -= OnAttemptFinished;
        if (isYes)
        {
            bool success = await HttpService.SpendPoints(1);
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
        else
        {
            ResetQuestionValues();
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
            ui.SetupCard(card, data.Points);
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
