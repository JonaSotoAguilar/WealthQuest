using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class QuestionController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI questionText;
    [SerializeField]
    private Button[] optionButtons;
    public event Action OnQuestionAnswered;

    // Variable para almacenar el último botón seleccionado
    private GameObject lastSelectedButton;

    public void SetupQuestion(string question, string[] options, int correctIndex, int score, PlayerData player)
    {
        questionText.text = question;

        for (int i = 0; i < options.Length; i++)
        {
            optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = options[i];
            int index = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => Answer(index, correctIndex, score, player));
        }
        // Te posicionas en el primer botón
        optionButtons[0].Select();
        lastSelectedButton = optionButtons[0].gameObject; // Guardar el primer botón seleccionado

        ShowPanel(true);
    }

    void Answer(int index, int correctIndex, int score, PlayerData player)
    {
        if (index == correctIndex)
        {
            player.Score = score; // Puntaje por respuesta correcta
        }

        ShowPanel(false);

        OnQuestionAnswered?.Invoke(); // Notificar que la pregunta ha sido respondida

        // Vuelve a seleccionar el último botón seleccionado después de procesar la respuesta
        if (lastSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(lastSelectedButton);
        }
    }

    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
