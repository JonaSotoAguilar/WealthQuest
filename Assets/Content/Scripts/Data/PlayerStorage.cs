using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections;

[System.Serializable]
public class NewPlayer
{
    public int index;
    public string name;
    public Character model;
    public InputDevice device;
    public string controlScheme;
}

public static class PlayerStorage
{
    public static List<NewPlayer> players = new List<NewPlayer>();

    public static void SavePlayerStorage(int index, InputDevice device, string controlScheme, string playerName, Character character)
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
    }

    public static void ClearData()
    {
        players.Clear();
        players = new List<NewPlayer>();
    }

}
