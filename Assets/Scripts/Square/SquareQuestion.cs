using UnityEngine;
using System.Linq;

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
    public override bool SquareSleeping() => squareSleeping;

    private CanvasPlayer canvasPlayer;

    public override void ActiveSquare(PlayerData player) => throw new System.NotImplementedException();


    public override void ActiveSquare(PlayerData player, CanvasPlayer canvasPlayer)
    {
        squareSleeping = false;
        this.canvasPlayer = canvasPlayer;

        // Mostrar la pregunta y configurar el panel de la pregunta
        QuestionController panel = canvasPlayer.QuestionPanel;

        panel.SetupQuestion(question, answers, indexCorrectAnswer, ScoreForCorrectAnswer, player); // Pasamos el dispositivo aquí

        // Suscribirse al evento de respuesta de la pregunta
        panel.OnQuestionAnswered += HandleQuestionAnswered;
    }

    private void HandleQuestionAnswered()
    {
        // Desuscribirse para evitar que el manejador sea llamado múltiples veces
        canvasPlayer.QuestionPanel.OnQuestionAnswered -= HandleQuestionAnswered;

        squareSleeping = true; // Ahora que la pregunta ha sido respondida, permitir que el juego continúe
    }
}

