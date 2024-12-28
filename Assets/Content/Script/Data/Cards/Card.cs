using Mirror;
using UnityEngine;

public abstract class Card : ScriptableObject
{
    [Tooltip("Titulo carta")] public string title; 
    [Tooltip("Imagen de carta")] public Sprite image;  

    // Método abstracto que deberá ser implementado por las subclases para formatear el texto
    public abstract string GetFormattedText(int playerKFP);

    public abstract void ApplyEffect(int capital = 0, bool isLocalGame = true);
}

public static class CardSerializer
{
    // Escribir la carta (serialización)
    public static void WriteCard(this NetworkWriter writer, Card card)
    {
        writer.WriteString(card.name);

        // Determinamos el tipo de carta y lo escribimos
        string cardType = card.GetType().Name;
        writer.WriteString(cardType);
    }

    // Leer la carta (deserialización)
    public static Card ReadCard(this NetworkReader reader)
    {
        string cardName = reader.ReadString(); 
        string cardType = reader.ReadString();
        Debug.Log($"Card name: {cardName}, Card type: {cardType}");

        // Construir la ruta según el tipo
        string folder = GetFolderByCardType(cardType);

        if (string.IsNullOrEmpty(folder))
        {
            Debug.LogError($"Tipo de carta desconocido: {cardType}");
            return null;
        }

        // Buscar la carta en Resources
        Card card = Resources.Load<Card>($"Card/{folder}/{cardName}");

        if (card == null)
        {
            Debug.LogError($"No se encontró la carta con el nombre: {cardName} en la carpeta {folder}");
        }

        return card;
    }

    // Método auxiliar para mapear el tipo de carta a una carpeta
    private static string GetFolderByCardType(string cardType)
    {
        return cardType switch
        {
            "ExpenseCard" => "Expense",
            "InvestmentCard" => "Investment",
            "IncomeCard" => "Income",
            "EventCard" => "Event",
            _ => null, 
        };
    }
}
