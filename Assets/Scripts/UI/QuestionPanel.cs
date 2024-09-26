using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class QuestionPanel : MonoBehaviour
{
    public TextMeshProUGUI questionText;
    public Button[] optionButtons;
    private int correctAnswerIndex;
    private Player currentPlayer;
    public event Action OnQuestionAnswered; // Evento para notificar cuando se ha respondido la pregunta

    void Start()
    {
        // Inicialmente ocultar el panel
        gameObject.SetActive(false);
    }

    public void SetupQuestion(string question, string[] options, int correctIndex, Player player)
    {
        questionText.text = question;
        correctAnswerIndex = correctIndex;
        currentPlayer = player;

        for (int i = 0; i < options.Length; i++)
        {
            optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = options[i];
            int index = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => Answer(index));
        }

        ShowPanel(true);
    }

    void Answer(int index)
    {
        ShowPanel(false);
        if (index == correctAnswerIndex)
        {
            currentPlayer.ModifyScore(10); // Asume un puntaje fijo por respuesta correcta
            HUDManager.instance.ActualizarHUD(currentPlayer);
        }

        // Notificar que la pregunta ha sido respondida
        OnQuestionAnswered?.Invoke();
    }

    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
