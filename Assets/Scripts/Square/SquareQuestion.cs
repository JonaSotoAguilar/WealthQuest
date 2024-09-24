using UnityEngine;

public class SquareQuestion : Square
{
    public string pregunta = "";
    public string[] opciones = { "", "", "" };
    public int indiceRespuestaCorrecta = 0; // El índice de la respuesta correcta
    public int puntajePorRespuestaCorrecta = 10; // Puntaje que otorga si responde bien

    public override void ActivarCasilla(PlayerStats jugador)
    {
        QuestionPanel panel = HUDManager.instance.GetComponentInChildren<QuestionPanel>(true);
        panel.SetupQuestion(pregunta, opciones, indiceRespuestaCorrecta, jugador);
    }

}
