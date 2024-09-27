using UnityEngine;

public class SquareQuestion : Square
{
    public string question = "";
    public string[] answers = { "", "", "" };
    public int indexCorrectAnswer = 0;
    public int ScoreForCorrectAnswer = 10;
    private bool squareSleeping;

    public override void ActiveSquare(Player player)
    {
        squareSleeping = false;

        // Mostrar la pregunta y configurar el panel de la pregunta
        QuestionPanel panel = HUDController.instance.GetComponentInChildren<QuestionPanel>(true);
        panel.SetupQuestion(question, answers, indexCorrectAnswer, player); // Pasamos el dispositivo aquí

        // Suscribirse al evento de respuesta de la pregunta
        panel.OnQuestionAnswered += HandleQuestionAnswered;
    }

    private void HandleQuestionAnswered()
    {
        // Desuscribirse para evitar que el manejador sea llamado múltiples veces
        HUDController.instance.GetComponentInChildren<QuestionPanel>(true).OnQuestionAnswered -= HandleQuestionAnswered;

        squareSleeping = true; // Ahora que la pregunta ha sido respondida, permitir que el juego continúe
    }

    public override bool IsSquareStopped()
    {
        return squareSleeping;
    }
}

