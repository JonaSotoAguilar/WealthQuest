using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateContent : MonoBehaviour
{
    [Header("Game Data")]
    [SerializeField] private Content content;

    [Header("Create Content Panel")]
    [SerializeField] private Transform container;
    [SerializeField] private GameObject questionPanelPrefab;
    [SerializeField] private List<CreateQuestion> questions = new List<CreateQuestion>();

    [Header("Confirm Panel")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TextMeshProUGUI nameError;
    [SerializeField] private Button confirmButton;

    private bool update = false;
    private int version = 1;

    private void Start()
    {
        confirmButton.interactable = false;
        nameInput.onValueChanged.AddListener(delegate { ValidateNameInput(); });
    }

    private void OnDisable()
    {
        update = false;
        nameInput.text = "";
        nameInput.interactable = true;
        ClearQuestions();
        AddQuestion();
    }

    private void ValidateNameInput()
    {
        if (update) return;

        if (string.IsNullOrWhiteSpace(nameInput.text))
        {
            confirmButton.interactable = false;
            return;
        }

        if (content.ExistsContent(nameInput.text))
        {
            nameError.text = "Ya existe un contenido con ese nombre";
            nameError.gameObject.SetActive(true);
            confirmButton.interactable = false;
        }
        else
        {
            nameError.gameObject.SetActive(false);
            confirmButton.interactable = true;
        }
    }

    private void ClearQuestions()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        questions.Clear();
    }

    public void OpenCreatePopup()
    {
        MenuManager.Instance.OpenConfirmCreatePopup(true);
        nameError.gameObject.SetActive(false);
    }

    public void ChangeContent(string name, int currVersion)
    {
        update = true;
        version = currVersion;
        nameInput.text = name;
        nameInput.interactable = false;
        confirmButton.interactable = true;
        StartCoroutine(LoadQuestions());
    }

    private IEnumerator LoadQuestions()
    {
        if (!update) yield return null;

        ClearQuestions();
        QuestionList questionList = new QuestionList();
        yield return SaveSystem.LoadContent(questionList, nameInput.text);

        foreach (var question in questionList.questions)
        {
            AddQuestion();
            CreateQuestion newQuestion = questions[questions.Count - 1];
            newQuestion.LoadQuestion(question);
        }
    }

    public void AddQuestion()
    {
        GameObject newPanel = Instantiate(questionPanelPrefab, container);
        CreateQuestion newQuestion = newPanel.GetComponent<CreateQuestion>();
        questions.Add(newQuestion);
        newQuestion.deleteButton.onClick.AddListener(delegate { DeleteQuestion(questions.Count - 1); });
    }

    private void DeleteQuestion(int index)
    {
        Destroy(questions[index].gameObject);
        questions.RemoveAt(index);
    }

    public void CreateContentBundle()
    {
        confirmButton.interactable = false;
        QuestionList questionList = new QuestionList();

        foreach (var question in questions)
        {
            QuestionData questionData = question.CreateQuestionData();
            if (questionData == null)
            {
                nameError.text = "Faltan campos por completar en una pregunta";
                nameError.gameObject.SetActive(true);
                return;
            }

            questionList.questions.Add(questionData);
        }

        if (update)
        {
            StartCoroutine(SaveSystem.UpdateContent(questionList, nameInput.text, version));
            MenuManager.Instance.OpenMessagePopup("Contenido modificado con éxito.");
        }
        else
        {
            StartCoroutine(SaveSystem.SaveContent(questionList, nameInput.text));
            MenuManager.Instance.OpenMessagePopup("Contenido creado con éxito.");
        }

        MenuManager.Instance.OpenConfirmCreatePopup(false);
        MenuManager.Instance.OpenContentMenu();
        confirmButton.interactable = true;
    }

}