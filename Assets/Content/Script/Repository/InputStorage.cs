using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public static class InputStorage
{
    public static List<InputDevice> devices = new List<InputDevice>();

    public static void SaveInputStorage(InputDevice device)
    {
        if (device == null)
        {
            Debug.LogError("Dispositivo es null. No se puede guardar el jugador.");
            return;
        }

        devices.Add(device);
    }

    public static void ClearData()
    {
        devices.Clear();
        devices = new List<InputDevice>();
    }

}
