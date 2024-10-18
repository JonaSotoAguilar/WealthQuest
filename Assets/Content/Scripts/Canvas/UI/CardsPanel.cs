using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

// FIXME: Agregar cartas de inversión no habilitadas por fondo insuficiente
public class CardsPanel : MonoBehaviour
{
    [SerializeField] private EventSystem playerEventSystem;
    [SerializeField] private Transform cardGrid;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private InvestPanel investPanel;

    public event Action OnCardSelected;
    public Transform CardGrid { get => cardGrid; set => cardGrid = value; }
    public InvestPanel InvestPanel { get => investPanel; set => investPanel = value; }

    // Si hago click fuera de las tarjetas, se selecciona la primera nuevamente
    private void OnEnable() => playerEventSystem.SetSelectedGameObject(cardGrid.GetChild(0).gameObject);

    // Mostrar tarjetas y permitir la selección
    public void SetupCards(PlayerData player, List<CardBase> selectedCards)
    {
        // Limpiar las tarjetas anteriores si existen
        ClearCards();

        if (selectedCards.Count > 0)
        {
            foreach (var card in selectedCards)
            {
                // Crear y mostrar la tarjeta como hija del panel
                SetupCard(card, player);
            }

            // Seleccionar la primera tarjeta
            playerEventSystem.SetSelectedGameObject(cardGrid.GetChild(0).gameObject);

            // Mostrar panel
            ShowPanel(true);
            if (selectedCards[0] is InvestmentCard)
                investPanel.ShowPanel(true);
        }
        else
        {
            Debug.LogError("No hay suficientes tarjetas disponibles.");
        }
    }

    // Método para configurar cada tarjeta individualmente
    private void SetupCard(CardBase card, PlayerData player)
    {
        // Instanciar el prefab de la tarjeta como hijo del panel
        GameObject cardInstance = Instantiate(cardPrefab, cardGrid);

        // Asignar la imagen de la tarjeta
        RawImage cardImage = cardInstance.transform.Find("CardImage").GetComponent<RawImage>();
        cardImage.texture = card.image.texture;

        // Asignar la descripción de la tarjeta
        TextMeshProUGUI descriptionText = cardInstance.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
        descriptionText.text = card.title;

        // Asignar el costo de la tarjeta
        TextMeshProUGUI costText = cardInstance.transform.Find("CostText").GetComponent<TextMeshProUGUI>();
        costText.text = card.GetFormattedText(player.ScoreKFP);

        // Si no es una tarjeta de inversión, asignar el evento de selección
        Button cardButton = cardInstance.GetComponent<Button>();
        cardButton.onClick.AddListener(() => HandleOptionSelected(card, player));
    }

    // Método que maneja la selección de la tarjeta
    public void HandleOptionSelected(CardBase selectedCard, PlayerData player)
    {
        int amountInt = 0;
        if (selectedCard is InvestmentCard)
        {
            amountInt = investPanel.GetInvestmentAmount();
            investPanel.ShowPanel(false);
        }
        ShowPanel(false);
        ClearCards();

        // Aplicar el efecto de la tarjeta seleccionada
        selectedCard.ApplyEffect(player, amountInt);

        // Invocar el evento para notificar que se ha seleccionado una opción
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
