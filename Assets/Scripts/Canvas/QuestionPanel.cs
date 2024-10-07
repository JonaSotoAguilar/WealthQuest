using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class QuestionPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button[] optionButtons;
    public event Action OnQuestionAnswered;
    private GameObject lastSelectedButton;

    public void SetupQuestion(QuestionData questionData, PlayerData player)
    {
        questionText.text = questionData.question;

        for (int i = 0; i < questionData.answers.Length; i++)
        {
            optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = questionData.answers[i];
            int index = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => Answer(index, questionData, player));
        }
        optionButtons[0].Select();
        lastSelectedButton = optionButtons[0].gameObject;

        // Seleccionar el primer botón
        EventSystem.current.SetSelectedGameObject(optionButtons[0].gameObject);

        ShowPanel(true);
    }

    void Answer(int index, QuestionData questionData, PlayerData player)
    {
        if (index == questionData.indexCorrectAnswer)
        {
            player.ScoreKFP = questionData.scoreForCorrectAnswer;
        }

        ShowPanel(false);
        OnQuestionAnswered?.Invoke();
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
