using System;
using System.Collections.Generic;
using System.Globalization;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIPlayer : MonoBehaviour
{
    private static CultureInfo chileanCulture = new CultureInfo("es-CL");
    [SerializeField] private EventSystem system;

    [Header("Questions")]
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button[] optionButtons;
    public event Action<bool> OnQuestionAnswered;

    [Header("Cards")]
    [SerializeField] private GameObject cardsPanel;
    [SerializeField] private Transform cardGrid;
    [SerializeField] private GameObject cardPrefab;
    private List<Card> selectedCards = new List<Card>();
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
        selectedCards = new List<Card>();
        ShowQuestion(false);
        CloseCards();

        cancelButton.onClick.AddListener(CancelSelection);
    }

    #region QuestionPanel

    public void SetupQuestion(QuestionData questionData, bool isOwned)
    {
        questionText.text = questionData.question;

        for (int i = 0; i < questionData.answers.Length; i++)
        {
            int localIndex = i;
            optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = questionData.answers[i];
            optionButtons[i].onClick.RemoveAllListeners();
            if (isOwned) optionButtons[i].onClick.AddListener(() => Answer(localIndex, questionData));
        }

        if (system != null) system.SetSelectedGameObject(optionButtons[0].gameObject);

        ShowQuestion(true);
    }

    //FIXME: Revisar
    void Answer(int index, QuestionData questionData)
    {
        bool isCorrect = index == questionData.indexCorrectAnswer;

        OnQuestionAnswered?.Invoke(isCorrect);
    }

    public void ShowQuestion(bool visible)
    {
        questionPanel.SetActive(visible);
    }

    #endregion

    #region CardsPanel

    public void SetupCard(Card card, int points, int money)
    {
        GameObject cardInstance = Instantiate(cardPrefab, cardGrid);

        RawImage cardImage = cardInstance.transform.Find("CardImage").GetComponent<RawImage>();
        cardImage.texture = card.image.texture;

        TextMeshProUGUI descriptionText = cardInstance.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
        descriptionText.text = card.title;

        TextMeshProUGUI costText = cardInstance.transform.Find("CostText").GetComponent<TextMeshProUGUI>();
        costText.text = card.GetFormattedText(points);

        Button cardButton = cardInstance.GetComponent<Button>();
        cardButton.onClick.AddListener(() => HandleOptionSelected(card));

        if (card is InvestmentCard && money < minInvestment) cardButton.interactable = false;
        selectedCards.Add(card);
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
        selectedCards.Clear();
        foreach (Transform child in cardGrid)
            Destroy(child.gameObject);
    }

    public void ShowCards(int money)
    {
        cardsPanel.SetActive(true);
        if (selectedCards[0] is InvestmentCard)
        {
            if (money < minInvestment) ActiveAmount(false);
            else ActiveAmount(true);

            moneyPlayer = money;
            ResetAmount();
            ShowInvest(true);
        }
    }

    public bool ActiveCards()
    {
        return cardsPanel.activeSelf;
    }

    #endregion

    #region InvestmentPanel

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

    private void ActiveAmount(bool active)
    {
        if (active && system != null) system.SetSelectedGameObject(cardGrid.GetChild(0).gameObject);
        else if (system != null) system.SetSelectedGameObject(cancelButton.gameObject);

        increaseAmount.interactable = active;
        lowerAmount.interactable = active;
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