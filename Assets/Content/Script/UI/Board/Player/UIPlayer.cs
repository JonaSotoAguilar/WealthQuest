using System;
using System.Collections.Generic;
using System.Globalization;
using Mirror;
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
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TextMeshProUGUI timerText;
    public event Action<bool> OnQuestionAnswered;

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

    [Header("Invest")]
    [SerializeField] private GameObject investPanel;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Button increaseAmount;
    [SerializeField] private Button lowerAmount;
    [SerializeField] private Button cancelButton;

    [Header("Invest Settings")]
    [SerializeField] private int minInvestment = 100;
    [SerializeField] private int amountChange = 100;
    private int moneyPlayer;
    private int amountInvest;

    public int AmountChange { get => amountChange; }
    public int MinInvestment { get => minInvestment; }
    public int AmountInvest { get => amountInvest; }


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

    #region QuestionPanel

    public void SetupQuestion(QuestionData questionData, int attemps, bool isOwned)
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

        if (systemLocal != null) systemLocal.SetSelectedGameObject(optionButtons[0].gameObject);
        else EventSystem.current.SetSelectedGameObject(optionButtons[0].gameObject);

        PauseMenu.SetCanvasGroup(canvasGroupUI);
    }

    void Answer(int index, QuestionData questionData)
    {
        bool isCorrect = index == questionData.indexCorrectAnswer;

        OnQuestionAnswered?.Invoke(isCorrect);
    }

    public void UpdateTimerDisplay(int secondsRemaining)
    {
        timerText.text = $"{secondsRemaining}s";
    }

    public void ShowQuestion(bool visible)
    {
        questionPanel.SetActive(visible);
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
        GameObject cardInstance = Instantiate(cardPrefab, cardGrid);

        GameObject grafic = cardInstance.transform.Find("Grafic").gameObject;
        if (card is InvestmentCard)
        {
            ConfigureGrafic(card, grafic);
            grafic.SetActive(true);
        }
        else
        {
            grafic.SetActive(false);
        }

        RawImage cardImage = cardInstance.transform.Find("CardImage").GetComponent<RawImage>();
        cardImage.texture = card.image.texture;

        TextMeshProUGUI descriptionText = cardInstance.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
        descriptionText.text = card.title;

        TextMeshProUGUI costText = cardInstance.transform.Find("CostText").GetComponent<TextMeshProUGUI>();
        costText.text = card.GetFormattedText(points);

        Button cardButton = cardInstance.GetComponent<Button>();
        cardButton.onClick.AddListener(() => HandleOptionSelected(card));

        cardButtons.Add(cardButton);
    }

    public void HandleOptionSelected(Card selectedCard)
    {
        OnCardSelected?.Invoke(selectedCard);
    }

    public void CloseCards()
    {
        if (investPanel.activeSelf == true) ShowInvest(false);
        cardsPanel.SetActive(false);
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

        cardsPanel.SetActive(true);
        
        if (systemLocal != null) systemLocal.SetSelectedGameObject(cardButtons[0].gameObject);
        else EventSystem.current.SetSelectedGameObject(cardButtons[0].gameObject);
        PauseMenu.SetCanvasGroup(canvasGroupUI);
    }

    public bool ActiveCards()
    {
        return cardsPanel.activeSelf;
    }

    #endregion

    #region InvestmentPanel

    private void ConfigureGrafic(Card card, GameObject grafic)
    {
        isInvestmentCard = true;
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

    #endregion
}