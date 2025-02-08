using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Mirror;
using TMPro;
using Unity.VisualScripting;
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
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private GameObject questionTextPanel;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private GameObject optionsParent;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TextMeshProUGUI timerText;
    public event Action<int, bool> OnQuestionAnswered;

    [Header("Attempts")]
    [SerializeField] private GameObject attemptsPanel;
    [SerializeField] private TextMeshProUGUI attemptsText;
    [SerializeField] private TextMeshProUGUI attemptsValue;
    [SerializeField] private Button[] attemptsButtons;
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
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private GameObject investText;
    [SerializeField] private Button increaseAmount;
    [SerializeField] private Button lowerAmount;
    [SerializeField] private Button cancelButton;
    [SerializeField] private GameObject notInvestCardPrefab;

    [Header("Invest Settings")]
    [SerializeField] private int minInvestment = 100;
    [SerializeField] private int amountChange = 100;
    private int moneyPlayer;
    private int amountInvest;

    #region Getters

    public int AmountChange { get => amountChange; }
    public int MinInvestment { get => minInvestment; }
    public int AmountInvest { get => amountInvest; }

    #endregion

    #region Initialization

    void Awake()
    {
        ShowQuestion(false);
        ShowAttempts(false);
        CloseCards();

        cancelButton.onClick.AddListener(CancelSelection);
    }

    public void DesactiveCanvaGroup()
    {
        canvasGroupUI.interactable = false;
        canvasGroupUI.blocksRaycasts = false;
    }

    #endregion

    #region QuestionPanel

    public void SetupQuestion(Question questionData, int attemps, bool isOwned)
    {
        questionText.text = questionData.question;

        for (int i = 0; i < questionData.answers.Length; i++)
        {
            int localIndex = i;
            optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = questionData.answers[i];
            optionButtons[i].onClick.RemoveAllListeners();
            if (isOwned) optionButtons[i].onClick.AddListener(() => Answer(localIndex, questionData));
        }

        attemptsValue.text = attemps.ToString();
        ShowQuestion(true);


        if (systemLocal != null && isOwned) systemLocal.SetSelectedGameObject(optionButtons[0].gameObject);
        else if (isOwned) EventSystem.current.SetSelectedGameObject(optionButtons[0].gameObject);

        PauseMenu.SetCanvasGroup(canvasGroupUI);
    }

    void Answer(int index, Question questionData)
    {
        foreach (Button button in optionButtons)
            button.interactable = false;
        bool isCorrect = index == questionData.indexCorrectAnswer;

        OnQuestionAnswered?.Invoke(index, isCorrect);
    }

    public void UpdateTimerDisplay(int secondsRemaining)
    {
        timerText.text = $"{secondsRemaining}s";
    }

    public void ShowQuestion(bool visible)
    {
        if (visible)
        {
            questionPanel.transform.localScale = Vector3.zero;
            questionTextPanel.SetActive(true);
            foreach (Button button in optionButtons)
            {
                button.gameObject.SetActive(true);
                button.interactable = true;
            }
            questionPanel.SetActive(true);

            LeanTween.scale(questionPanel, Vector3.one, 0.3f).setEaseOutBack();
            AudioManager.PlayOpenCard();
        }
        else
        {
            questionPanel.SetActive(false);
            ResertOutline();
        }
    }

    public async Task AnsweredQuestion(int index, bool isCorrect)
    {
        if (index < 0 || index >= optionButtons.Length) return;

        questionTextPanel.SetActive(false);
        optionsParent.transform.localPosition = new Vector3(0, -50, 0);

        // Esconde los botones que no son el índice seleccionado
        for (int i = 0; i < optionButtons.Length; i++)
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
            outline.enabled = false;
            outline.effectColor = originalOutlineColor;
        }

        // Esconde el botón después de la animación
        optionButtons[index].gameObject.SetActive(false);
        optionButtons[index].transform.localScale = Vector3.one;
        optionsParent.transform.localPosition = new Vector3(0, -216, 0);
        cardAnimation.enabled = true;
    }

    private void ResertOutline()
    {
        foreach (Button button in optionButtons)
        {
            Outline outline = button.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = false;
            }
        }
    }

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

    #region Attempts

    public void UpdateAttempts(int attempts)
    {
        attemptsValue.text = attempts.ToString();
    }

    public void ShowAttempts(bool show)
    {
        if (show)
        {
            int points = ProfileUser.bGamesProfile.points;
            attemptsText.text = "Tienes " + points + " puntos de bGames, ¿quieres usar 1 punto para tener un intento extra?";
            if (systemLocal != null) systemLocal.SetSelectedGameObject(attemptsButtons[0].gameObject);
            else EventSystem.current.SetSelectedGameObject(attemptsButtons[0].gameObject);
        }
        attemptsPanel.SetActive(show);
    }

    public void YesMoreAttempts()
    {
        OnAttemptFinished?.Invoke(true);
    }

    public void NoMoreAttempts()
    {
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
        cardButtons.Add(cardButton);
        selectedCardType = card.GetCardType();
    }

    public void HandleOptionSelected(Card selectedCard)
    {
        foreach (Button button in cardButtons)
            button.interactable = false;

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

    public void ShowCards(int money, bool isOwned = true)
    {
        if (isInvestmentCard)
        {
            if (money < minInvestment) ActiveAmount(false);
            else ActiveAmount(true);

            moneyPlayer = money;
            ResetAmount();
            ShowInvest(true);
        }
        else
        {
            ShowInvest(false);
        }

        cardsPanel.transform.localScale = Vector3.zero;
        cardsPanel.SetActive(true);
        AudioManager.PlayOpenCard();
        LeanTween.scale(cardsPanel, Vector3.one, 0.3f).setEaseOutBack();

        if (systemLocal != null && isOwned) systemLocal.SetSelectedGameObject(cardButtons[0].gameObject);
        else if (isOwned) EventSystem.current.SetSelectedGameObject(cardButtons[0].gameObject);
        PauseMenu.SetCanvasGroup(canvasGroupUI);
    }

    public bool ActiveCards()
    {
        return cardsPanel.activeSelf;
    }

    public async Task CardSelected(int index)
    {
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
        if (!active)
        {
            if (systemLocal != null) systemLocal.SetSelectedGameObject(cancelButton.gameObject);
            else EventSystem.current.SetSelectedGameObject(cancelButton.gameObject);
        }

        increaseAmount.interactable = active;
        lowerAmount.interactable = active;
        cancelButton.interactable = true;

        foreach (Button cardButton in cardButtons)
            cardButton.interactable = active;
    }

    public void IncreaseAmount()
    {
        string amount = amountText.text.Substring(1);
        amountInvest = int.Parse(amount.Replace(".", ""));
        if (amountInvest + amountChange <= moneyPlayer)
        {
            ChangeAmountInvest(amountInvest + amountChange);
        }
    }

    public void LowerAmount()
    {
        string amount = amountText.text.Substring(1);
        amountInvest = int.Parse(amount.Replace(".", ""));
        if (amountInvest - amountChange >= minInvestment)
        {
            ChangeAmountInvest(amountInvest - amountChange);
        }
    }

    public void CancelSelection()
    {
        OnCardSelected?.Invoke(null);
    }

    public void ResetAmount()
    {
        amountInvest = minInvestment;
        amountText.text = amountInvest.ToString("C0", chileanCulture);
    }

    public void ChangeAmountInvest(int newAmount)
    {
        amountInvest = newAmount;
        amountText.text = amountInvest.ToString("C0", chileanCulture);
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
}