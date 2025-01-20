using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateContent : MonoBehaviour
{
    [Header("Create Content Panel")]
    [SerializeField] private Transform container;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject questionPanelPrefab;
    [SerializeField] private List<CreateQuestion> questions = new List<CreateQuestion>();

    [Header("Confirm Panel")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TextMeshProUGUI nameError;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI confirmButtonText;

    [Header("Popup Confirm")]
    [SerializeField] private TextMeshProUGUI confirmTitleText;
    [SerializeField] private TextMeshProUGUI confirmButtonPopupText;

    private bool update = false;
    private int version = 1;
    private string uidContent = "";

    #region Initialization

    private void Start()
    {
        nameInput.onValueChanged.AddListener(delegate { ValidateNameInput(); });
    }

    private void OnDisable()
    {
        ClearQuestions();
    }

    public void NewContent()
    {
        update = false;
        version = 1;
        uidContent = "";
        nameInput.text = "";
        nameInput.interactable = true;
        confirmButton.interactable = false;
        confirmButtonText.text = "Crear";
        confirmTitleText.text = "Crear Contenido";
        confirmButtonPopupText.text = "Crear";
        AddQuestion();
    }

    public void UpdateContent(Content content)
    {
        update = true;
        version = content.version;
        Debug.Log("Version Inicial: " + version);
        uidContent = content.uid;
        nameInput.text = content.name;
        nameInput.interactable = false;
        confirmButton.interactable = true;
        confirmButtonText.text = "Actualizar";
        confirmTitleText.text = "Actualizar Contenido";
        confirmButtonPopupText.text = "Actualizar";
        StartCoroutine(LoadQuestions(content));
    }

    private void ClearQuestions()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        questions.Clear();
    }

    #endregion

    #region Validation

    private void ValidateNameInput()
    {
        if (update) return;

        if (string.IsNullOrWhiteSpace(nameInput.text))
        {
            confirmButton.interactable = false;
            return;
        }

        if (ContentDatabase.ExistsContent(nameInput.text))
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

    #endregion

    #region Update Content

    private IEnumerator LoadQuestions(Content content)
    {
        if (!update) yield return null;

        ClearQuestions();

        foreach (var question in content.questions)
        {
            AddQuestion();
            CreateQuestion newQuestion = questions[questions.Count - 1];
            newQuestion.LoadQuestion(question);
        }
    }

    public void AddQuestion()
    {
        // Instanciar el panel de la pregunta
        GameObject newPanel = Instantiate(questionPanelPrefab, container);
        CreateQuestion newQuestion = newPanel.GetComponent<CreateQuestion>();

        // Agregar la nueva pregunta a la lista
        questions.Add(newQuestion);

        // Configurar el botón de eliminación para buscar y eliminar la pregunta
        if (questions.Count > 1)
            newQuestion.deleteButton.onClick.AddListener(() => DeleteQuestion(newQuestion));
        else
            newQuestion.deleteButton.gameObject.SetActive(false);

        // Desplazar el scroll hacia abajo
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
        Canvas.ForceUpdateCanvases();
    }

    public void DeleteQuestion(CreateQuestion question)
    {
        if (questions.Contains(question))
        {
            questions.Remove(question);
            Destroy(question.gameObject);
        }
    }

    #endregion

    #region Create Content

    public void OpenCreatePopup()
    {
        MenuManager.Instance.OpenConfirmCreatePopup(true);
        nameError.gameObject.SetActive(false);
    }

    public void CreateContentBundle()
    {
        confirmButton.interactable = false;
        List<Question> questionList = new List<Question>();

        foreach (var question in questions)
        {
            Question questionData = question.CreateQuestionData();
            if (questionData == null)
            {
                nameError.text = "Faltan campos por completar en una pregunta";
                nameError.gameObject.SetActive(true);
                return;
            }

            questionList.Add(questionData);
        }

        Content content = new Content(nameInput.text, questionList);
        if (update)
        {
            Debug.Log("Update content");
            content.uid = uidContent;
            content.version = ++version;
        }

        Debug.Log("Content: " + content.name + " - " + content.uid + " - " + content.version);
        SaveService.SaveContent(content);
        if (update)
        {
            MenuManager.Instance.OpenMessagePopup("Contenido modificado con éxito.");
        }
        else
        {
            MenuManager.Instance.OpenMessagePopup("Contenido creado con éxito.");
        }

        MenuManager.Instance.OpenConfirmCreatePopup(false);
        MenuManager.Instance.OpenContentMenu();
        confirmButton.interactable = true;
    }

    #endregion

}