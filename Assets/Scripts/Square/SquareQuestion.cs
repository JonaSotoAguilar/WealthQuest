using UnityEngine;

public class SquareQuestion : Square
{
    [SerializeField]
    private string question = "";
    [SerializeField]
    private string[] answers = { "", "", "" };
    [SerializeField]
    private int indexCorrectAnswer = 0;
    [SerializeField]
    private int ScoreForCorrectAnswer = 10;
    private bool squareSleeping;

    public override void ActiveSquare(PlayerController player)
    {
        squareSleeping = false;

        // Mostrar la pregunta y configurar el panel de la pregunta
        QuestionPanelController panel = CanvasManager.instance.GetComponentInChildren<QuestionPanelController>(true);
        panel.SetupQuestion(question, answers, indexCorrectAnswer, ScoreForCorrectAnswer, player); // Pasamos el dispositivo aquí

        // Suscribirse al evento de respuesta de la pregunta
        panel.OnQuestionAnswered += HandleQuestionAnswered;
    }

    private void HandleQuestionAnswered()
    {
        // Desuscribirse para evitar que el manejador sea llamado múltiples veces
        CanvasManager.instance.GetComponentInChildren<QuestionPanelController>(true).OnQuestionAnswered -= HandleQuestionAnswered;

        squareSleeping = true; // Ahora que la pregunta ha sido respondida, permitir que el juego continúe
    }

    public override bool IsSquareStopped()
    {
        return squareSleeping;
    }
}

