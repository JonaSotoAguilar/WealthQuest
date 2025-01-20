using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestQuestion : MonoBehaviour
{
    public event Action OnAnswered; // Evento para notificar cuando se responde

    [Header("Question data")]
    [SerializeField] private TextMeshProUGUI questionIndexText;
    [SerializeField] private TextMeshProUGUI questionText;

    [Header("Answer data")]
    [SerializeField] private GameObject answerPrefab;
    [SerializeField] private GameObject answerParent;
    private List<Toggle> answerToggles = new List<Toggle>();

    public int indexCorrectAnswer = -1;
    public int answerIndex = -1;

    private bool answered = false;

    public void SetQuestion(QuestionData question, int index)
    {
        // Reiniciar el índice de la respuesta seleccionada y estado
        indexCorrectAnswer = question.IndexCorrectAnswer;
        answerIndex = -1;
        answered = false;

        // Limpiar los toggles anteriores
        foreach (Transform child in answerParent.transform)
        {
            Destroy(child.gameObject);
        }
        answerToggles.Clear();

        // Configurar los textos de la pregunta
        questionIndexText.text = $"Pregunta {index + 1}";
        questionText.text = question.Question;

        // Crear toggles para las respuestas
        for (int i = 0; i < question.Answers.Length; i++)
        {
            // Instanciar el prefab
            GameObject toggleObject = Instantiate(answerPrefab, answerParent.transform);
            Toggle toggle = toggleObject.GetComponent<Toggle>();
            Text toggleLabel = toggleObject.GetComponentInChildren<Text>();

            // Configurar el texto de la respuesta
            toggleLabel.text = question.Answers[i];

            // Capturar el índice actual en una variable local para usar en el callback
            int currentIndex = i;

            // Agregar la lógica de exclusividad al toggle
            toggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    // Actualizar el índice de la respuesta seleccionada
                    answerIndex = currentIndex;

                    // Desactivar los otros toggles
                    foreach (var otherToggle in answerToggles)
                    {
                        if (otherToggle != toggle)
                        {
                            otherToggle.isOn = false;
                        }
                    }

                    // Notificar solo la primera vez que se selecciona una respuesta
                    if (!answered)
                    {
                        answered = true;
                        OnAnswered?.Invoke(); // Llamar al evento
                    }
                }
            });

            // Añadir el toggle a la lista
            answerToggles.Add(toggle);
        }
    }

    public bool CheckAnswer()
    {
        return answerIndex == indexCorrectAnswer;
    }
}
