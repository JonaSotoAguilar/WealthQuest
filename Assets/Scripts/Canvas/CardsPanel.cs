using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class CardsPanel : MonoBehaviour
{
    [SerializeField] private EventSystem playerEventSystem;
    [SerializeField] private GameObject cardPrefab;
    public event Action OnCardSelected;

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
            playerEventSystem.SetSelectedGameObject(transform.GetChild(0).gameObject);

            // Mostrar el panel
            ShowPanel(true);
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
        GameObject cardInstance = Instantiate(cardPrefab, transform);

        // Asignar la imagen de la tarjeta
        RawImage cardImage = cardInstance.transform.Find("CardImage").GetComponent<RawImage>();
        cardImage.texture = card.image.texture;

        // Asignar la descripción de la tarjeta
        TextMeshProUGUI descriptionText = cardInstance.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
        descriptionText.text = card.description;

        // Asignar el costo de la tarjeta
        TextMeshProUGUI costText = cardInstance.transform.Find("CostText").GetComponent<TextMeshProUGUI>();
        costText.text = card.GetFormattedText(player.ScoreKFP);

        // Asignar el comportamiento de selección
        Button cardButton = cardInstance.GetComponent<Button>();
        cardButton.onClick.AddListener(() => HandleOptionSelected(card, player));
    }

    // Método que maneja la selección de la tarjeta
    private void HandleOptionSelected(CardBase selectedCard, PlayerData player)
    {
        // Aplicar el efecto de la tarjeta seleccionada
        selectedCard.ApplyEffect(player);

        // Eliminar la tarjeta de su respectiva lista
        // FIXME: selectedCard.RemoveFromGameData();

        // Limpiar el panel de cartas
        ClearCards();
        ShowPanel(false);

        // Invocar el evento para notificar que se ha seleccionado una opción
        OnCardSelected?.Invoke();
    }


    // Limpiar las tarjetas previas
    private void ClearCards()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
