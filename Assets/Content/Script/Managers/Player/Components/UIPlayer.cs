using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class UIPlayer : MonoBehaviour
{
    private static CultureInfo chileanCulture = new CultureInfo("es-CL");
    [Header("System")]
    [SerializeField] private MultiplayerEventSystem systemLocal;
    [SerializeField] public CanvasGroup canvasGroupUI;

    [Header("Questions")]
    [SerializeField] private GameObject answerPrefab;
    [SerializeField] private Transform answerParent;
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private GameObject questionTextPanel;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private GameObject optionsParent;
    [SerializeField] private TextMeshProUGUI timerText;
    private List<Button> optionButtons;
    public event Action<int, bool> OnQuestionAnswered;

    [Header("Attempts")]
    [SerializeField] private GameObject attemptsPanel;
    [SerializeField] private TextMeshProUGUI attemptsText;
    [SerializeField] private TextMeshProUGUI attemptsValue;
    public event Action<bool> OnAttemptFinished;

    [Header("Cards")]
    [SerializeField] private GameObject cardsPanel;
    [SerializeField] private Transform cardGrid;
    [SerializeField] private GameObject cardPrefab;
    private List<Button> cardButtons = new List<Button>();
    private bool isInvestmentCard = false;
    public event Action<Card> OnCardSelected;
    private SquareType selectedCardType;

    [Header("Invest")]
    [SerializeField] private GameObject investPanel;
    [SerializeField] public TMP_InputField amountText;
    [SerializeField] private GameObject investText;
    [SerializeField] private Button increaseAmount;
    [SerializeField] private Button lowerAmount;
    [SerializeField] private Button cancelButton;
    [SerializeField] private GameObject notInvestCardPrefab;

    [Header("Invest Settings")]
    [SerializeField] private int amountChange = 100;
    private int moneyPlayer;
    private int amountInvest;
    private bool isOwned = true;

    #region Getters

    public int AmountChange { get => amountChange; }
    public int AmountInvest { get => amountInvest; }

    #endregion

    #region Initialization

    void Awake()
    {
        cancelButton.onClick.AddListener(CancelSelection);
        amountText.onEndEdit.AddListener(ValidateAmountInput);
    }

    public void IsNotOwner()
    {
        isOwned = false;
        DesactiveCanvaGroup(false);
    }

    public void RemoveLocalListeners()
    {
        amountText.onEndEdit.RemoveListener(ValidateAmountInput);
    }

    private void DesactiveCanvaGroup(bool active)
    {
        canvasGroupUI.interactable = active;
        canvasGroupUI.blocksRaycasts = active;
    }

    #endregion

    #region QuestionPanel

    public void SetupQuestion(Question questionData, int attempts)
    {
        questionText.text = questionData.question;

        // Crea una nueva lista de botones según las respuestas
        optionButtons = new List<Button>(questionData.answers.Length);

        for (int i = 0; i < questionData.answers.Length; i++)
        {
            int localIndex = i;

            // Instancia un nuevo botón desde el prefab
            GameObject answerGO = Instantiate(answerPrefab, answerParent);

            // Configura el texto del botón
            answerGO.GetComponentInChildren<TextMeshProUGUI>().text = questionData.answers[i];

            // Obtiene el componente Button
            Button answerButton = answerGO.GetComponent<Button>();

            // Limpia listeners y añade uno nuevo si es dueño
            answerButton.onClick.RemoveAllListeners();
            if (isOwned)
            {
                answerButton.onClick.AddListener(() => Answer(localIndex, questionData));
            }

            // Añade el botón a la lista
            optionButtons.Add(answerButton);
        }

        attemptsValue.text = attempts.ToString();
        ShowQuestion();
    }

    void Answer(int index, Question questionData)
    {
        canvasGroupUI.interactable = false;
        bool isCorrect = index == questionData.indexCorrectAnswer;

        OnQuestionAnswered?.Invoke(index, isCorrect);
    }

    public void ShowQuestion()
    {
        canvasGroupUI.interactable = false;
        questionPanel.transform.localScale = Vector3.zero;
        questionTextPanel.SetActive(true);
        foreach (Button button in optionButtons)
        {
            button.gameObject.SetActive(true);
            button.interactable = true;
        }
        questionPanel.SetActive(true);

        LeanTween.scale(questionPanel, Vector3.one, 0.5f).setEaseOutBack().setOnComplete(() =>
            {
                if (isOwned)
                {
                    SetFirstSelectable();
                    canvasGroupUI.interactable = true;
                }
            });
        AudioManager.PlayOpenCard();
    }

    public async Task AnsweredQuestion(int index, bool isCorrect)
    {
        if (index < 0 || index >= optionButtons.Count) return;

        canvasGroupUI.interactable = false;
        questionTextPanel.SetActive(false);
        optionsParent.transform.localPosition = new Vector3(0, -50, 0);

        // Esconde los botones que no son el índice seleccionado
        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (i != index) optionButtons[i].gameObject.SetActive(false);
        }

        // Obtiene CardAnimation
        CardAnimation cardAnimation = optionButtons[index].GetComponent<CardAnimation>();
        cardAnimation.enabled = false;

        // Obtener el componente Outline del botón
        Outline outline = optionButtons[index].GetComponent<Outline>();

        // Guardar el color original (para restaurarlo después)
        Color originalOutlineColor = outline != null ? outline.effectColor : Color.clear;

        // Reproduce sonido según si es correcto o incorrecto
        if (isCorrect)
        {
            AudioManager.PlaySoundCorrectAnswer();
            if (outline != null)
            {
                outline.enabled = true;
                outline.effectColor = Color.green; // Cambia el borde a verde
            }
        }
        else
        {
            AudioManager.PlaySoundWrongAnswer();
            if (outline != null)
            {
                outline.enabled = true;
                outline.effectColor = Color.red; // Cambia el borde a rojo
            }
        }

        // Escala el botón seleccionado y espera a que termine la animación
        await ScaleButtonAsync(optionButtons[index].gameObject, Vector3.one * 1.4f, 0.8f);

        // Restaurar el color original del borde
        if (outline != null)
        {
            outline.effectColor = originalOutlineColor;
            outline.enabled = false;
        }

        // Esconde el botón después de la animación
        optionButtons[index].gameObject.SetActive(false);
        optionButtons[index].transform.localScale = Vector3.one;
        optionsParent.transform.localPosition = new Vector3(0, -216, 0);
        cardAnimation.enabled = true;
        CloseQuestion();
    }

    public void CloseQuestion()
    {
        questionPanel.SetActive(false);
        ClearAnswers();
    }

    public void ClearAnswers()
    {
        if (optionButtons == null || optionButtons.Count == 0) return;
        optionButtons.Clear();
        foreach (Transform child in answerParent)
            Destroy(child.gameObject);
    }

    public void UpdateTimerDisplay(int secondsRemaining)
    {
        timerText.text = $"{secondsRemaining}s";
    }

    #endregion

    #region Attempts

    public void ShowMoreAttempts()
    {
        Debug.Log("ShowMoreAttempts");
        int points = ProfileUser.bGamesProfile.points;
        attemptsText.text = "Tienes " + points + " puntos de bGames, ¿quieres usar 1 punto para tener un intento extra?";

        canvasGroupUI.interactable = true;
        attemptsPanel.SetActive(true);
        SetFirstSelectable();
    }

    public void CloseMoreAttempts()
    {
        attemptsPanel.SetActive(false);
    }

    public void YesMoreAttempts()
    {
        canvasGroupUI.interactable = false;
        OnAttemptFinished?.Invoke(true);
    }

    public void NoMoreAttempts()
    {
        canvasGroupUI.interactable = false;
        OnAttemptFinished?.Invoke(false);
    }

    #endregion

    #region CardsPanel

    public void SetupCard(Card card, int points)
    {
        // Instanciar la carta
        GameObject cardInstance = Instantiate(cardPrefab, cardGrid);

        // Configurar el gráfico si es una carta de inversión
        GameObject grafic = cardInstance.transform.Find("Grafic").gameObject;
        if (card is InvestmentCard)
        {
            isInvestmentCard = true;
            ConfigureGrafic(card, grafic);
            grafic.SetActive(true);
        }
        else
        {
            isInvestmentCard = false;
            grafic.SetActive(false);
        }

        // Configurar imagen, descripción y costo
        RawImage cardImage = cardInstance.transform.Find("CardImage").GetComponent<RawImage>();
        cardImage.texture = card.image.texture;

        TextMeshProUGUI descriptionText = cardInstance.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
        descriptionText.text = card.title;

        TextMeshProUGUI costText = cardInstance.transform.Find("CostText").GetComponent<TextMeshProUGUI>();
        costText.text = card.GetFormattedText(points);

        // Configurar el botón de la carta
        Button cardButton = cardInstance.GetComponent<Button>();
        cardButton.onClick.AddListener(() => HandleOptionSelected(card));
        selectedCardType = card.GetCardType();
        cardButtons.Add(cardButton);
    }

    public void HandleOptionSelected(Card selectedCard)
    {
        canvasGroupUI.interactable = false;
        OnCardSelected?.Invoke(selectedCard);
    }

    public void CloseCards()
    {
        cardsPanel.SetActive(false);
        investPanel.SetActive(false);
        ClearCards();
    }

    private void ClearCards()
    {
        cardButtons.Clear();
        foreach (Transform child in cardGrid)
            Destroy(child.gameObject);
    }

    public void ShowCards(int money)
    {
        canvasGroupUI.interactable = false;
        if (isInvestmentCard)
        {
            if (money <= 0) ActiveAmount(false);
            else ActiveAmount(true);

            moneyPlayer = money;
            ChangeAmountInvest(0);
            ShowInvest(true);
        }
        else
        {
            ShowInvest(false);
            EnableCards(true);
        }

        cardsPanel.transform.localScale = Vector3.zero;
        cardsPanel.SetActive(true);
        AudioManager.PlayOpenCard();
        LeanTween.scale(cardsPanel, Vector3.one, 0.5f).setEaseOutBack().setOnComplete(() =>
            {
                if (isOwned)
                {
                    SetFirstSelectable();
                    canvasGroupUI.interactable = true;
                }
            });
    }

    public async Task CardSelected(int index)
    {
        canvasGroupUI.interactable = false;
        investPanel.SetActive(false);

        if (index < 0 && isInvestmentCard)
        {
            GameObject cardInstance = Instantiate(notInvestCardPrefab, cardGrid);
            Button cardButton = cardInstance.GetComponent<Button>();
            cardButtons.Add(cardButton);
            index = cardButtons.Count - 1;
        }

        // Esconde los botones que no son el índice seleccionado
        for (int i = 0; i < cardButtons.Count; i++)
        {
            if (i != index) cardButtons[i].gameObject.SetActive(false);
        }

        CardAnimation cardAnimation = cardButtons[index].GetComponent<CardAnimation>();
        cardAnimation.enabled = false;

        Outline outline = cardButtons[index].GetComponent<Outline>();
        if (outline != null) outline.enabled = true;

        AudioManager.PlaySoundCard(selectedCardType);

        // Escala el botón seleccionado y espera a que termine la animación
        await ScaleButtonAsync(cardButtons[index].gameObject, Vector3.one * 1.4f, 0.8f);

        // Esconde el botón después de la animación
        cardButtons[index].gameObject.SetActive(false);
        CloseCards();
    }

    #endregion

    #region InvestmentPanel

    private void EnableCards(bool enable)
    {
        foreach (Button cardButton in cardButtons)
        {
            cardButton.enabled = enable;
            cardButton.interactable = enable;
            CardAnimation cardAnimation = cardButton.GetComponent<CardAnimation>();
            cardAnimation.enabled = enable;
        }
    }

    private void ConfigureGrafic(Card card, GameObject grafic)
    {
        InvestmentCard investmentCard = (InvestmentCard)card;
        Sprite image = investmentCard.GenerateGraphSprite();
        card.image = image;
        int year = investmentCard.startYear;
        int prevoiusYear = year - investmentCard.pctChangePrevious.Count - 1;
        TextMeshProUGUI graficText = grafic.transform.Find("Year").GetComponent<TextMeshProUGUI>();
        graficText.text = prevoiusYear + " - " + (year - 1) + " (aprox. " + investmentCard.CalculateCompoundPercentage() + "%)";
    }

    private void ActiveAmount(bool active)
    {
        increaseAmount.interactable = active;
        lowerAmount.interactable = active;
        cancelButton.interactable = true;
    }

    public void IncreaseAmount()
    {
        if (amountInvest + amountChange <= moneyPlayer)
        {
            ChangeAmountInvest(amountInvest + amountChange);
        }
        else
        {
            ChangeAmountInvest(moneyPlayer);
        }
    }

    public void LowerAmount()
    {
        if (amountInvest - amountChange > 0)
        {
            ChangeAmountInvest(amountInvest - amountChange);
        }
        else
        {
            ChangeAmountInvest(0);
        }
    }

    private void ValidateAmountInput(string input)
    {
        ParseAmount();
        ChangeAmountInvest(amountInvest);
    }

    public void ChangeAmountInvest(int newAmount)
    {
        amountInvest = Mathf.Clamp(newAmount, 0, moneyPlayer);
        amountText.text = amountInvest.ToString("C0", chileanCulture);

        if (amountInvest > 0)
        {
            EnableCards(true);
        }
        else
        {
            EnableCards(false);
        }
    }

    private void ParseAmount()
    {
        string amount = amountText.text.Replace("$", "").Replace(".", "").Trim();

        if (int.TryParse(amount, out int parsedAmount))
        {
            amountInvest = Mathf.Clamp(parsedAmount, 0, moneyPlayer);
        }
        else
        {
            amountInvest = 0;
        }
    }


    public void CancelSelection()
    {
        canvasGroupUI.interactable = false;
        OnCardSelected?.Invoke(null);
    }

    public void ShowInvest(bool show)
    {
        investPanel.SetActive(show);
    }

    public void NotOwnerInvest()
    {
        investText.SetActive(false);
        increaseAmount.gameObject.SetActive(false);
        lowerAmount.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);
    }

    #endregion

    #region EventSystem

    private void SetFirstSelectable()
    {
        Selectable firstSelectable = gameObject.GetComponentInChildren<Selectable>();
        if (systemLocal != null)
        {
            systemLocal.SetSelectedGameObject(firstSelectable.gameObject);
        }
        else if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectable.gameObject);
        }

        PauseMenu.SetCanvasGroup(canvasGroupUI);
    }

    #endregion

    #region Animation

    private Task ScaleButtonAsync(GameObject button, Vector3 targetScale, float duration)
    {
        var tcs = new TaskCompletionSource<bool>();

        // Usa LeanTween para escalar el botón y completa la tarea al finalizar
        LeanTween.scale(button, targetScale, duration)
            .setEaseOutBack()
            .setOnComplete(() => tcs.SetResult(true));

        return tcs.Task;
    }

    #endregion

}