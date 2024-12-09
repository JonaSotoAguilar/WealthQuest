using Mirror;
using UnityEngine;

public abstract class Card : ScriptableObject
{
    [Tooltip("Titulo carta")] public string title;  // Descripción de la carta
    [Tooltip("Imagen de carta")] public Sprite image;        // Imagen de la carta

    // Método abstracto que deberá ser implementado por las subclases para formatear el texto
    public abstract string GetFormattedText(int playerKFP);

    public abstract void ApplyEffect(int capital = 0, bool isLocalGame = true);
}

public static class CardSerializer
{
    // Escribir la carta (serialización)
    public static void WriteCard(this NetworkWriter writer, Card card)
    {
        writer.WriteString(card.name); // Guarda solo el nombre de la carta
    }

    // Leer la carta (deserialización)
    public static Card ReadCard(this NetworkReader reader)
    {
        string cardName = reader.ReadString(); // Lee el nombre de la carta

        // Busca la carta en las listas de GameData (debes asegurarte de tener una instancia válida de GameData)
        Card card = GameNetManager.Data.GetCardByName(cardName);

        if (card == null)
        {
            Debug.LogError($"No se encontró la carta con el nombre: {cardName}");
        }

        return card;
    }
}
