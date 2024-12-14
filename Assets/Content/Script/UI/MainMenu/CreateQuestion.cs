using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateQuestion : MonoBehaviour
{
    [SerializeField] private TMP_InputField question;
    [SerializeField] private TMP_InputField[] answers;
    [SerializeField] private TMP_Dropdown correctAnswer;

    [SerializeField] private TMP_InputField topic;
    [SerializeField] private TMP_InputField subTopic;
    [SerializeField] private TMP_Dropdown levels;

    [SerializeField] public Button deleteButton;

    private void Awake()
    {
        GameObject[] buttons = new GameObject[] { deleteButton.gameObject };
        MenuAnimation.Instance.SubscribeButtonsToEvents(buttons);
    }

    public void LoadQuestion(QuestionData questionData)
    {
        question.text = questionData.question;
        for (int i = 0; i < answers.Length; i++)
        {
            answers[i].text = questionData.answers[i];
        }
        correctAnswer.value = questionData.indexCorrectAnswer;

        topic.text = questionData.topic;
        subTopic.text = questionData.subTopic;
        levels.value = questionData.level;
    }

    public QuestionData CreateQuestionData()
    {
        if (string.IsNullOrEmpty(question.text) || string.IsNullOrEmpty(topic.text) || string.IsNullOrEmpty(subTopic.text))
        {
            return null;
        }

        string[] answersArray = new string[answers.Length];
        for (int i = 0; i < answers.Length; i++)
        {
            answersArray[i] = answers[i].text;
            // Si alguna respuesta está vacía, ya existe en la lista, o son espacios en blanco, se retorna null
            if (string.IsNullOrEmpty(answersArray[i]) || answersArray[i].Trim().Length == 0 || answersArray[i].Trim() == " ")
            {
                return null;
            }
        }

        return new QuestionData(question.text, answersArray, correctAnswer.value, topic.text, subTopic.text, levels.value);
    }

}