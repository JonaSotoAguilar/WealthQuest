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
    private static GameManagerNetwork gameManager;
    public event Action<bool> OnQuestionAnswered;

    void Start()
    {
        if (gameManager != null) return;

        gameManager = GameManagerNetwork.Instance;
    }

    public void SetupQuestion(QuestionData questionData, IPlayer player)
    {
        questionText.text = questionData.question;

        for (int i = 0; i < questionData.answers.Length; i++)
        {
            optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = questionData.answers[i];
            int index = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => Answer(index, questionData, player));
        }

        playerEventSystem.SetSelectedGameObject(optionButtons[0].gameObject);

        ShowPanel(true);
    }

    void Answer(int index, QuestionData questionData, IPlayer player)
    {
        bool isCorrect = index == questionData.indexCorrectAnswer;

        if (isCorrect)
        {
            player.AddPoints(questionData.scoreForCorrectAnswer);
            gameManager.CmdDeleteQuestion(questionData);
        }

        OnQuestionAnswered?.Invoke(isCorrect);
    }

    public void ClosePanel()
    {
        // FIXME: Agregar animacioN respuesta escogida
        ShowPanel(false);
    }

    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
