using UnityEngine;
using System.Collections;

public class SquareQuestion : Square
{
    private PlayerCanvas canvasPlayer;

    // Implementación de ActiveSquare como una corrutina
    public override IEnumerator ActiveSquare(PlayerData player, PlayerCanvas canvas)
    {
        canvasPlayer = canvas;
        QuestionData selectedQuestion = GameData.Instance.GetRandomQuestion();

        if (selectedQuestion != null)
        {
            // Configurar el panel de preguntas
            QuestionPanel panel = canvasPlayer.QuestionPanel;
            panel.SetupQuestion(selectedQuestion, player);

            // Usar un bool para esperar hasta que la pregunta se haya respondido
            bool questionAnswered = false;

            // Subscribirse al evento de respuesta
            System.Action onQuestionAnswered = () => questionAnswered = true;
            panel.OnQuestionAnswered += onQuestionAnswered;

            // Esperar hasta que la pregunta sea respondida
            yield return new WaitUntil(() => questionAnswered);

            // Desuscribirse del evento después de que la pregunta haya sido respondida
            panel.OnQuestionAnswered -= onQuestionAnswered;
        }
        else
        {
            Debug.LogError("No quedan preguntas disponibles.");
        }
    }
}