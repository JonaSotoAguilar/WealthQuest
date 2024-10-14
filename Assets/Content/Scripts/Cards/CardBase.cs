using UnityEngine;

public abstract class CardBase : ScriptableObject
{
    public string description;  // Descripción de la carta
    public Sprite image;        // Imagen de la carta

    // Método abstracto que deberá ser implementado por las subclases para formatear el texto
    public abstract string GetFormattedText(int playerKFP);

    public abstract void ApplyEffect(PlayerData player, int capital = 0);

    public abstract void RemoveFromGameData();
}
