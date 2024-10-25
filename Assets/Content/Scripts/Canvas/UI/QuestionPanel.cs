using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class QuestionPanel : MonoBehaviour
{
    [SerializeField] private EventSystem playerEventSystem;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button[] optionButtons;
    public event Action OnQuestionAnswered;

    public void SetupQuestion(QuestionData questionData, PlayerController player)
    {
        questionText.text = questionData.question;

        for (int i = 0; i < questionData.answers.Length; i++)
        {
            optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = questionData.answers[i];
            int index = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => Answer(index, questionData, player));
        }

        // Seleccionar el primer botón multiplayer event system
        playerEventSystem.SetSelectedGameObject(optionButtons[0].gameObject);

        ShowPanel(true);
    }

    void Answer(int index, QuestionData questionData, PlayerController player)
    {
        if (index == questionData.indexCorrectAnswer)
        {
            player.ChangeKFP(questionData.scoreForCorrectAnswer);
            GameManager.Instance.GameData.QuestionList.Remove(questionData);
        }

        ShowPanel(false);
        OnQuestionAnswered?.Invoke();
    }

    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
