using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class CardsPanel : MonoBehaviour
{
    [SerializeField] private EventSystem playerEventSystem;
    [SerializeField] private Transform cardGrid;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private InvestPanel investPanel;
    [SerializeField] private PlayerController currentPlayer;

    public event Action OnCardSelected;
    public Transform CardGrid { get => cardGrid; set => cardGrid = value; }
    public InvestPanel InvestPanel { get => investPanel; set => investPanel = value; }

    private void OnEnable() => playerEventSystem.SetSelectedGameObject(cardGrid.GetChild(0).gameObject);

    void Update()
    {
        if (investPanel.gameObject.activeSelf)
        {
            if (currentPlayer.PlayerData.Money < investPanel.GetInvestmentAmount())
            {
                foreach (Transform child in cardGrid)
                    child.GetComponent<Button>().interactable = false;
            }
            else
            {
                foreach (Transform child in cardGrid)
                    child.GetComponent<Button>().interactable = true;
            }
        }
    }

    public void SetupCards(PlayerController player, List<CardBase> selectedCards)
    {
        ClearCards();
        currentPlayer = player;
        if (selectedCards.Count > 0)
        {
            foreach (var card in selectedCards)
                SetupCard(card);
            ShowPanel(true);
            if (selectedCards[0] is InvestmentCard)
            {
                investPanel.ResetAmount();
                investPanel.MoneyPlayer = currentPlayer.PlayerData.Money;
                investPanel.ShowPanel(true);
            }
            playerEventSystem.SetSelectedGameObject(cardGrid.GetChild(0).gameObject);
        }
        else
            Debug.LogError("No hay suficientes tarjetas disponibles.");
    }

    // Método para configurar cada tarjeta individualmente
    private void SetupCard(CardBase card)
    {
        GameObject cardInstance = Instantiate(cardPrefab, cardGrid);

        RawImage cardImage = cardInstance.transform.Find("CardImage").GetComponent<RawImage>();
        cardImage.texture = card.image.texture;

        TextMeshProUGUI descriptionText = cardInstance.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
        descriptionText.text = card.title;

        TextMeshProUGUI costText = cardInstance.transform.Find("CostText").GetComponent<TextMeshProUGUI>();
        costText.text = card.GetFormattedText(currentPlayer.PlayerData.ScoreKFP);

        Button cardButton = cardInstance.GetComponent<Button>();
        cardButton.onClick.AddListener(() => HandleOptionSelected(card));
    }

    // Método que maneja la selección de la tarjeta
    public void HandleOptionSelected(CardBase selectedCard)
    {
        int amountInt = 0;
        if (selectedCard is InvestmentCard)
        {
            amountInt = investPanel.GetInvestmentAmount();
            investPanel.ShowPanel(false);
        }
        ShowPanel(false);
        ClearCards();
        selectedCard.ApplyEffect(currentPlayer, amountInt);
        OnCardSelected?.Invoke();
    }

    // Limpiar las tarjetas previas
    private void ClearCards()
    {
        foreach (Transform child in cardGrid)
        {
            Destroy(child.gameObject);
        }
    }

    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void CancelSelection()
    {
        investPanel.ShowPanel(false);
        ShowPanel(false);
        ClearCards();
        OnCardSelected?.Invoke();
    }
}
