using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem.UI;

public class PlayerLocalUI : MonoBehaviour
{
    [SerializeField] private UIPlayer ui;

    // Question
    private int levelQuestion;
    private int attempts = 2;
    private List<Question> questions = new List<Question>();
    private Question currentQuestion;
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

        ui.SetupQuestion(currentQuestion, attempts);
        ui.OnQuestionAnswered += OnAnswerQuestion;

        questionTimerCoroutine = StartCoroutine(QuestionTimer());
    }

    private void GetQuestionsTopic()
    {
        int level = GetComponent<PlayerLocalData>().Level;
        questions = GameLocalManager.Data.GetQuestionsByLevel(level);
    }

    private void ResetQuestionValues()
    {
        useBGames = false;
        attempts = 2;
        currentQuestion = null;
        questions.Clear();
    }

    private void OnAnswerQuestion(int index, bool isCorrect)
    {
        ui.OnQuestionAnswered -= OnAnswerQuestion;
        StopTimer();

        if (index < 0)
        {
            SubmitAnswer(false);
        }
        else
        {
            _ = TaskRpcAnsweredQuestion(index, isCorrect);
        }
    }

    private async Task TaskRpcAnsweredQuestion(int index, bool isCorrect)
    {
        await ui.AnsweredQuestion(index, isCorrect);
        SubmitAnswer(isCorrect);
    }

    private void SubmitAnswer(bool isCorrect)
    {
        ui.CloseQuestion();

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
            if (attempts <= 0 && CanPlayBGames())
            {
                Debug.Log("No hay más intentos. ¿Deseas usar un BGame?");
                AddAttempt();
            }
            else if (attempts <= 0)
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

    #endregion

    #region 

    private IEnumerator QuestionTimer()
    {
        float timeRemaining = 30f;
        AudioManager.PlaySoundTimer();
        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            // Puedes actualizar la UI del temporizador aquí, si tienes una
            ui.UpdateTimerDisplay(Mathf.CeilToInt(timeRemaining));

            yield return null;
        }

        // Si se agota el tiempo, se considera respuesta incorrecta
        OnAnswerQuestion(-1, false);
    }

    private void StopTimer()
    {
        AudioManager.StopSoundSFX();

        // Detener el temporizador
        if (questionTimerCoroutine != null)
        {
            StopCoroutine(questionTimerCoroutine);
            questionTimerCoroutine = null;
        }
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
        ui.ShowMoreAttempts();
        ui.OnAttemptFinished += OnAttemptFinished;
    }

    private async void OnAttemptFinished(bool isYes)
    {
        ui.CloseMoreAttempts();
        ui.OnAttemptFinished -= OnAttemptFinished;
        if (isYes)
        {
            bool success = await HttpService.SpendPoints(1);
            if (success)
            {
                attempts++;
                if (questions.Count > 1) questions.Remove(currentQuestion);
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
        _ = TaskCardSelected(index);
    }

    private async Task TaskCardSelected(int index)
    {
        if (index >= 0)
        {
            Card selectedCard = selectedCards[index];
            int capital = 0;
            if (selectedCard is InvestmentCard) capital = ui.AmountInvest;
            selectedCard.ApplyEffect(capital);
        }

        await ui.CardSelected(index);
        SubmitCard();
    }

    private void SubmitCard()
    {
        selectedCards.Clear();
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
