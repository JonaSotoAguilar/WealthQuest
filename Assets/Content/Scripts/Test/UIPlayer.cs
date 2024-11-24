using System;
using System.Collections.Generic;
using System.Globalization;
using FishNet;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayer : MonoBehaviour
{
    [Header("Questions")]
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button[] optionButtons;
    public event Action<bool> OnQuestionAnswered;

    [Header("Cards")]
    [SerializeField] private GameObject cardsPanel;
    [SerializeField] private Transform cardGrid;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private IPlayer currPlayer;
    public event Action OnCardSelected;

    [Header("Invest")]
    [SerializeField] private GameObject investPanel;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Button increaseAmount;
    [SerializeField] private Button lowerAmount;
    [SerializeField] private Button cancelButton;
    private CultureInfo chileanCulture = new CultureInfo("es-CL");


    //FIXME: Revisar
    private int amountInvest;
    private int moneyPlayer;

    void Awake()
    {
        ShowQuestion(false);
        ShowInvest(false);
        ShowCards(false);
    }

    #region QuestionPanel

    public void SetupQuestion(QuestionData questionData, bool isOwner)
    {
        questionText.text = questionData.question;

        for (int i = 0; i < questionData.answers.Length; i++)
        {
            int localIndex = i;
            optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = questionData.answers[i];
            optionButtons[i].onClick.RemoveAllListeners();
            if (isOwner)
                optionButtons[i].onClick.AddListener(() => Answer(localIndex, questionData));
        }

        ShowQuestion(true);
    }

    //FIXME: Revisar
    void Answer(int index, QuestionData questionData)
    {
        Debug.Log("Respuesta seleccionada: " + index);
        bool isCorrect = index == questionData.indexCorrectAnswer;

        if (isCorrect)
        {
            //FIXME: Revisar
            Debug.Log("Respuesta correcta");
            //player.AddPoints(questionData.scoreForCorrectAnswer);
            //GameOnline.Instance.CmdDeleteQuestion(questionData); //FIXME: Revisar
        }

        OnQuestionAnswered?.Invoke(isCorrect);
    }

    public void ShowQuestion(bool visible)
    {
        questionPanel.SetActive(visible);
    }

    #endregion

    #region CardsPanel

    public void SetupCards(IPlayer player, List<CardBase> selectedCards)
    {
        currPlayer = player;
        if (selectedCards.Count > 0)
        {
            foreach (var card in selectedCards)
                SetupCard(card);
            ShowCards(true);
            if (selectedCards[0] is InvestmentCard)
            {
                ResetAmount();
                moneyPlayer = currPlayer.Money;
                ShowInvest(true);
            }
        }
        else
            Debug.LogError("No hay suficientes tarjetas disponibles.");
    }

    private void SetupCard(CardBase card)
    {
        GameObject cardInstance = Instantiate(cardPrefab, cardGrid);

        RawImage cardImage = cardInstance.transform.Find("CardImage").GetComponent<RawImage>();
        cardImage.texture = card.image.texture;

        TextMeshProUGUI descriptionText = cardInstance.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
        descriptionText.text = card.title;

        TextMeshProUGUI costText = cardInstance.transform.Find("CostText").GetComponent<TextMeshProUGUI>();
        costText.text = card.GetFormattedText(currPlayer.Points);

        Button cardButton = cardInstance.GetComponent<Button>();
        cardButton.onClick.AddListener(() => HandleOptionSelected(card));
    }

    public void HandleOptionSelected(CardBase selectedCard)
    {
        int amountInt = 0;
        if (selectedCard is InvestmentCard)
            amountInt = GetInvestmentAmount();
        selectedCard.ApplyEffect(currPlayer, amountInt);
        OnCardSelected?.Invoke();
    }

    // Limpiar las tarjetas previas
    private void ClearCards()
    {
        foreach (Transform child in cardGrid)
        {
            InstanceFinder.ServerManager.Despawn(child.GetComponent<NetworkObject>());
            Destroy(child.gameObject);
        }
    }

    public void CancelSelection()
    {
        ShowInvest(false);
        ShowCards(false);
        ClearCards();
        OnCardSelected?.Invoke();
    }

    public void CloseCards()
    {
        // FIXME: Agregar animacion respuesta escogida
        if (investPanel.gameObject.activeSelf) ShowInvest(false);
        ShowCards(false);
        ClearCards();
    }

    public void ShowCards(bool visible)
    {
        cardsPanel.SetActive(visible);
    }

    #endregion

    #region InvestmentPanel

    public void IncreaseAmount()
    {
        // Pasar de formato chileno a int
        string amount = amountText.text.Substring(1);
        amountInvest = int.Parse(amount.Replace(".", ""));
        if (amountInvest + 100 <= moneyPlayer)
        {
            amountInvest += 100;
            amountText.text = amountInvest.ToString("C0", chileanCulture);
        }
    }

    public void LowerAmount()
    {
        string amount = amountText.text.Substring(1);
        amountInvest = int.Parse(amount.Replace(".", ""));
        if (amountInvest - 100 >= 100)
        {
            amountInvest -= 100;
            amountText.text = amountInvest.ToString("C0", chileanCulture);
        }
    }

    public int GetInvestmentAmount()
    {
        string amount = amountText.text.Substring(1);
        return int.Parse(amount.Replace(".", ""));
    }
    public void ResetAmount()
    {
        //FIXME: Si tiene menos de 100, no se puede invertir
        amountText.text = "$100";
        amountInvest = 100;
    }

    public void ShowInvest(bool show)
    {
        investPanel.SetActive(show);
    }

    #endregion
}
