using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class NewPlayer
{
    public int index;
    public string name;
    public Character model;
    public InputDevice device;
    public string controlScheme;
}

[CreateAssetMenu(fileName = "PlayerStorage", menuName = "Player/PlayerStorage")]
public class PlayerStorage : ScriptableObject
{
    [SerializeField] public List<NewPlayer> players = new List<NewPlayer>();

    public void SavePlayerStorage(int index, InputDevice device, string controlScheme, string playerName, Character character)
    {
        if (device == null)
        {
            Debug.LogError("Dispositivo es null. No se puede guardar el jugador.");
            return;
        }

        if (character == null)
        {
            Debug.LogError("Character es null. No se puede guardar el jugador.");
            return;
        }

        NewPlayer newPlayer = new NewPlayer
        {
            index = index,
            name = playerName,
            model = character,
            device = device,
            controlScheme = controlScheme
        };

        players.Add(newPlayer);
        Debug.Log($"Jugador {newPlayer.name} agregado con el dispositivo: {device.displayName} y esquema de control: {controlScheme}");
    }

    // Método para limpiar los datos almacenados
    public void ClearData()
    {
        players.Clear();
        players = new List<NewPlayer>();
    }

}
