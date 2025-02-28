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

    public void LoadQuestion(Question questionData)
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

    public Question CreateQuestionData()
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

        return new Question(question.text, answersArray, correctAnswer.value, topic.text, subTopic.text, levels.value);
    }

    public bool QuestionComplete()
    {
        if (question == null || topic == null || subTopic == null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(question.text) || string.IsNullOrEmpty(topic.text) || string.IsNullOrEmpty(subTopic.text))
        {
            return false;
        }

        if (answers == null)
        {
            return false;
        }

        for (int i = 0; i < answers.Length; i++)
        {
            if (answers[i] == null || string.IsNullOrEmpty(answers[i].text) || answers[i].text.Trim().Length == 0 || answers[i].text.Trim() == " ")
            {
                return false;
            }
        }

        return true;
    }


}