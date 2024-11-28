using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections;

[System.Serializable]
public class UserPlayer
{
    public int index;
    public string name;
    public int model;
    public InputDevice device;
    public string controlScheme;
}

public static class PlayerStorage
{
    public static List<UserPlayer> players = new List<UserPlayer>();

    public static void SavePlayerStorage(int index, InputDevice device, string controlScheme, string playerName, int character)
    {
        if (device == null)
        {
            Debug.LogError("Dispositivo es null. No se puede guardar el jugador.");
            return;
        }

        UserPlayer newPlayer = new UserPlayer
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
        players = new List<UserPlayer>();
    }

}
