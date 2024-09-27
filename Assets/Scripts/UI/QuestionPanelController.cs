using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class QuestionPanelController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI questionText;
    [SerializeField]
    private Button[] optionButtons;
    public event Action OnQuestionAnswered;

    public void SetupQuestion(string question, string[] options, int correctIndex, int score, PlayerController player)
    {
        questionText.text = question;

        for (int i = 0; i < options.Length; i++)
        {
            optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = options[i];
            int index = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => Answer(index, correctIndex, score, player));
        }

        ShowPanel(true);
    }

    void Answer(int index, int correctIndex, int score, PlayerController player)
    {
        if (index == correctIndex)
        {
            player.ModifyScore(score); // Puntaje por respuesta correcta
        }

        ShowPanel(false);

        OnQuestionAnswered?.Invoke(); // Notificar que la pregunta ha sido respondida
    }

    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }
}