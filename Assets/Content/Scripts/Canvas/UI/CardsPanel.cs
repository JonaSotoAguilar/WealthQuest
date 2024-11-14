using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using System.Collections;

public class CardsPanel : MonoBehaviour
{
    [SerializeField] private EventSystem playerEventSystem;
    [SerializeField] private Transform cardGrid;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private InvestPanel investPanel;
    [SerializeField] private IPlayer currPlayer;

    public event Action OnCardSelected;
    public Transform CardGrid { get => cardGrid; set => cardGrid = value; }
    public InvestPanel InvestPanel { get => investPanel; set => investPanel = value; }

    public void SetupCards(IPlayer player, List<CardBase> selectedCards)
    {
        currPlayer = player;
        if (selectedCards.Count > 0)
        {
            foreach (var card in selectedCards)
                SetupCard(card);
            ShowPanel(true);
            if (selectedCards[0] is InvestmentCard)
            {
                investPanel.ShowPanel(true);
                investPanel.MoneyPlayer = currPlayer.Money;
                StartCoroutine(InvestmentActive());
            }
            playerEventSystem.SetSelectedGameObject(cardGrid.GetChild(0).gameObject);
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

    private IEnumerator InvestmentActive()
    {
        while (investPanel.gameObject.activeSelf)
        {
            if (investPanel.MoneyPlayer > investPanel.AmountInvest)
                foreach (Transform child in cardGrid) child.GetComponent<Button>().interactable = true;
            else
                foreach (Transform child in cardGrid) child.GetComponent<Button>().interactable = false;
        }
        yield return null;
    }

    public void HandleOptionSelected(CardBase selectedCard)
    {
        int amountInt = 0;
        if (selectedCard is InvestmentCard)
            amountInt = investPanel.GetInvestmentAmount();
        selectedCard.ApplyEffect(currPlayer, amountInt);
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

    public void CancelSelection()
    {
        investPanel.ShowPanel(false);
        ShowPanel(false);
        ClearCards();
        OnCardSelected?.Invoke();
    }

    public void ClosePanel()
    {
        // FIXME: Agregar animacioN respuesta escogida
        if (investPanel.gameObject.activeSelf) investPanel.ShowPanel(false);
        ShowPanel(false);
        ClearCards();
    }

    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
