using UnityEngine;
using System.Collections.Generic;

public class SquareQuestion : Square
{
    private bool squareSleeping;
    private CanvasPlayer canvasPlayer;

    public override bool SquareSleeping() => squareSleeping;

    public override void ActiveSquare(PlayerData player, CanvasPlayer canvas)
    {
        squareSleeping = false;
        canvasPlayer = canvas;
        QuestionData selectedQuestion = GameData.Instance.GetRandomQuestion();
        if (selectedQuestion != null)
        {
            QuestionPanel panel = canvasPlayer.QuestionPanel;
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
}
