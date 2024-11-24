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
    private static GameOnline game;
    public event Action<bool> OnQuestionAnswered;

    void Start()
    {
        if (game != null) return;

        game = GameOnline.Instance;
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

        //FIXME: No en Online
        if (playerEventSystem != null) playerEventSystem.SetSelectedGameObject(optionButtons[0].gameObject);

        ShowPanel(true);
    }

    void Answer(int index, QuestionData questionData, IPlayer player)
    {
        bool isCorrect = index == questionData.indexCorrectAnswer;

        if (isCorrect)
        {
            Debug.Log("Respuesta correcta");
            player.AddPoints(questionData.scoreForCorrectAnswer);
            game.CmdDeleteQuestion(questionData); //FIXME: Revisar
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
