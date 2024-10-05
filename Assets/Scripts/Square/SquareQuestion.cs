using UnityEngine;
using System.Collections.Generic;

public class SquareQuestion : Square
{
    private bool squareSleeping;
    private CanvasPlayer canvasPlayer;

    public override bool SquareSleeping() => squareSleeping;

    public override void ActiveSquare(PlayerData player) => throw new System.NotImplementedException();

    public override void ActiveSquare(PlayerData player, CanvasPlayer canvas)
    {
        squareSleeping = false;
        canvasPlayer = canvas;
        QuestionData selectedQuestion = GetRandomQuestion();
        if (selectedQuestion != null)
        {
            QuestionController panel = canvasPlayer.QuestionPanel;
            panel.SetupQuestion(selectedQuestion, player);
            panel.OnQuestionAnswered += HandleQuestionAnswered;
        }
        else
        {
            Debug.LogError("No quedan preguntas disponibles.");
        }
    }

    private void HandleQuestionAnswered()
    {
        canvasPlayer.QuestionPanel.OnQuestionAnswered -= HandleQuestionAnswered;
        squareSleeping = true;
    }

    private QuestionData GetRandomQuestion()
    {
        if (GameData.Instance.QuestionList != null && GameData.Instance.QuestionList.Count > 0)
        {
            int randomIndex = Random.Range(0, GameData.Instance.QuestionList.Count);
            QuestionData selectedQuestion = GameData.Instance.QuestionList[randomIndex];
            GameData.Instance.QuestionList.RemoveAt(randomIndex);

            return selectedQuestion;
        }
        else
        {
            return null;
        }
    }
}
