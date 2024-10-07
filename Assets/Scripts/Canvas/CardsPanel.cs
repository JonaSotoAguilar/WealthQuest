using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class CardsPanel : MonoBehaviour
{

    [SerializeField] private GameObject cardPrefab;
    private GameObject cardInstance1;
    private GameObject cardInstance2;
    public event Action OnCardSelected;

    // Mostrar dos tarjetas aleatorias y permitir la selección
    public void SetupExpense(PlayerData player)
    {
        // Limpiar las tarjetas anteriores si existen
        ClearCards();

        // Obtener dos tarjetas aleatorias de la lista de GameData
        List<ExpenseCard> selectedCards = GameData.Instance.GetRandomExpenseCards(2);

        if (selectedCards.Count == 2)
        {
            // Crear y mostrar la primera tarjeta como hija del panel (CardsPanel usa transform directamente)
            cardInstance1 = SetupCard(selectedCards[0], player);

            // Crear y mostrar la segunda tarjeta como hija del panel (CardsPanel usa transform directamente)
            cardInstance2 = SetupCard(selectedCards[1], player);
            
            // Seleccionar la primera tarjeta
            EventSystem.current.SetSelectedGameObject(cardInstance1);

            ShowPanel(true);
        }
        else
        {
            Debug.LogError("No hay suficientes tarjetas disponibles.");
        }
    }

    // Método para configurar cada tarjeta individualmente
    private GameObject SetupCard(ExpenseCard expenseCard, PlayerData player)
    {
        // Instanciar el prefab de la tarjeta como hijo del panel
        GameObject cardInstance = Instantiate(cardPrefab, transform);

        // Asignar la imagen de la tarjeta
        RawImage cardImage = cardInstance.transform.Find("CardImage").GetComponent<RawImage>();
        cardImage.texture = expenseCard.image.texture;

        // Asignar la descripción de la tarjeta
        TextMeshProUGUI descriptionText = cardInstance.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
        descriptionText.text = expenseCard.description;

        // Asignar el costo de la tarjeta
        TextMeshProUGUI costText = cardInstance.transform.Find("CostText").GetComponent<TextMeshProUGUI>();
        costText.text = expenseCard.GetFormattedText(player.ScoreKFP);

        // Asignar el comportamiento de selección
        Button cardButton = cardInstance.GetComponent<Button>();
        cardButton.onClick.AddListener(() => HandleOptionSelected(expenseCard, player));

        return cardInstance;  // Devolver la instancia creada para su gestión posterior
    }

    // Método que maneja la selección de la tarjeta
    private void HandleOptionSelected(ExpenseCard selectedCard, PlayerData player)
    {
        // Crear un gasto a partir de la tarjeta seleccionada y el puntaje del jugador
        PlayerExpense expense = selectedCard.CreateExpense(player.ScoreKFP);

        // Aplicar el gasto al jugador (fijo o recurrente)
        player.ApplyExpense(expense, expense.Turns > 0);

        // Eliminar la tarjeta seleccionada de la lista
        GameData.Instance.ExpenseCards.Remove(selectedCard);

        // Destruir ambas tarjetas del panel
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