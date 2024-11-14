using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class NewPlayer
{
    public int index;
    public string name;
    public int character;
    public InputDevice device;
    public string controlScheme;
}

[System.Serializable]
public class DataPlayerNetwork
{
    public string uid;
    public int index;
    public string name;
    public int character;
}

public static class PlayerStorage
{
    public static List<NewPlayer> players = new List<NewPlayer>();
    public static List<DataPlayerNetwork> playersNetwork = new List<DataPlayerNetwork>();

    public static void SavePlayerStorage(int index, InputDevice device, string controlScheme, string playerName, int characterIndex)
    {
        if (device == null)
        {
            Debug.LogError("Dispositivo es null. No se puede guardar el jugador.");
            return;
        }

        NewPlayer newPlayer = new NewPlayer
        {
            index = index,
            name = playerName,
            character = characterIndex,
            device = device,
            controlScheme = controlScheme
        };

        players.Add(newPlayer);
    }


    public static void SavePlayerStorageNetwork(string uidUser, int index, string playerName, int characterIndex)
    {

        DataPlayerNetwork newPlayer = new DataPlayerNetwork
        {
            uid = uidUser,
            index = index,
            name = playerName,
            character = characterIndex,
        };

        playersNetwork.Add(newPlayer);
    }

    public static void ClearData() => players.Clear();

    public static void ClearDataNetwork() => playersNetwork.Clear();

}
