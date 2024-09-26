using UnityEngine;

public class SquareQuestion : Square
{
    public string pregunta = "";
    public string[] opciones = { "", "", "" };
    public int indiceRespuestaCorrecta = 0; // El índice de la respuesta correcta
    public int puntajePorRespuestaCorrecta = 10; // Puntaje que otorga si responde bien

    public override void ActivarCasilla(Player player)
    {
        // Marca que una pregunta está activa, bloqueando el lanzamiento de los dados
        GameManager.instance.isQuestionActive = true;

        // Mostrar la pregunta y configurar el panel de la pregunta
        QuestionPanel panel = HUDManager.instance.GetComponentInChildren<QuestionPanel>(true);
        panel.SetupQuestion(pregunta, opciones, indiceRespuestaCorrecta, player);

        // El panel de preguntas desactiva la bandera `isQuestionActive` cuando el jugador responde
        panel.OnQuestionAnswered += () =>
        {
            // Cuando la pregunta haya sido respondida, se habilita nuevamente el turno
            GameManager.instance.isQuestionActive = false;
        };
    }
}
