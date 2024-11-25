using System.Collections;
using Mirror;
using UnityEngine;

public class PlayerNetUI : NetworkBehaviour
{
    [SerializeField] private UIPlayer ui;

    // Question
    [SyncVar(hook = nameof(SetupQuestion))] private QuestionData currentQuestion;
    [SyncVar(hook = nameof(CloseQuestion))] private bool wasAnswerCorrect = false;

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
            PlayerNetData data = GetComponent<PlayerNetManager>().Data;
            data.AddPoints(currentQuestion.scoreForCorrectAnswer);
            GameNetManager.Data.DeleteQuestion(currentQuestion);
        }

        wasAnswerCorrect = isCorrect;
    }

    private void CloseQuestion(bool oldAnswered, bool newAnswered)
    {
        ui.ShowQuestion(false);
        if (!isOwned) return;

        PlayerNetManager player = GetComponent<PlayerNetManager>();
        if (newAnswered)
        {
            Debug.Log("CloseQuestion");
            player.CmdEnableDice(true);
        }
        else
        {
            //FinishTurn();
        }
    }

    #endregion
}
