using UnityEngine;
using System.IO;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;

public static class SaveSystem
{
    // Rutas de guardado
    public static readonly string saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
    public static readonly string historyDirectory = Path.Combine(Application.persistentDataPath, "History");
    public static readonly string savePathSlotSingle = Path.Combine(saveDirectory, "game_Single.save");
    public static readonly string savePathSlotLocalMulti = Path.Combine(saveDirectory, "game_Local.save");
    public static readonly string savePathSlotOnline = Path.Combine(saveDirectory, "game_Online.save");

    // Clave de Encriptación AES
    private static readonly byte[] aesKey = Encoding.UTF8.GetBytes("1234567890123456");  // 16 bytes exactos
    private static readonly byte[] aesIV = Encoding.UTF8.GetBytes("abcdefghijklmnop");   // 16 bytes exactos

    // Guardar el juego
    public static IEnumerator SaveGame(GameData data, int slotData)
    {
        string json = JsonUtility.ToJson(data);
        byte[] encryptedData = EncryptStringToBytes_Aes(json);
        if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);

        switch (slotData)
        {
            case 1:
                File.WriteAllBytes(savePathSlotSingle, encryptedData);
                break;
            case 2:
                File.WriteAllBytes(savePathSlotLocalMulti, encryptedData);
                break;
            case 3:
                File.WriteAllBytes(savePathSlotOnline, encryptedData);
                break;
            default:
                yield break;
        }

        yield return null;
    }

    // Cargar el juego
    public static IEnumerator LoadGame(GameData data, int slotData)
    {
        byte[] encryptedData = null;

        switch (slotData)
        {
            case 1:
                encryptedData = File.ReadAllBytes(savePathSlotSingle);
                break;
            case 2:
                encryptedData = File.ReadAllBytes(savePathSlotLocalMulti);
                break;
            case 3:
                encryptedData = File.ReadAllBytes(savePathSlotOnline);
                break;
            default:
                yield break;
        }

        if (encryptedData != null)
        {
            string descryptedData = DecryptStringFromBytes_Aes(encryptedData);
            JsonUtility.FromJsonOverwrite(descryptedData, data);
        }

        yield return null;
    }

    public static IEnumerator SaveHistory(GameData data, int slotData)
    {
        FinishGameData finishGameData = new FinishGameData(data);
        string json = JsonUtility.ToJson(finishGameData);
        byte[] encryptedData = EncryptStringToBytes_Aes(json);
        if (!Directory.Exists(historyDirectory)) Directory.CreateDirectory(historyDirectory);

        string historyPath = Path.Combine(historyDirectory, "game_" + PlayerPrefs.GetInt("gameId") + ".save");
        File.WriteAllBytes(historyPath, encryptedData);
        DeleteSave(slotData);

        yield return null;
    }

    public static IEnumerator LoadHistory(GameHistory data)
    {
        data.ClearHistory();
        string[] files = Directory.GetFiles(historyDirectory, "*.save");

        Array.Reverse(files);

        foreach (string file in files)
        {
            byte[] encryptedData = File.ReadAllBytes(file);
            string decryptedData = DecryptStringFromBytes_Aes(encryptedData);
            FinishGameData finishGameData = JsonUtility.FromJson<FinishGameData>(decryptedData);
            data.finishGameData.Add(finishGameData);
        }

        yield return null;
    }


    private static void DeleteSave(int slotData)
    {
        switch (slotData)
        {
            case 1:
                File.Delete(savePathSlotSingle);
                break;
            case 2:
                File.Delete(savePathSlotLocalMulti);
                break;
            case 3:
                File.Delete(savePathSlotOnline);
                break;
            default:
                break;
        }
        slotData = 0;
    }

    public static bool CheckSaveFile(int slot)
    {
        switch (slot)
        {
            case 1:
                return File.Exists(savePathSlotSingle);
            case 2:
                return File.Exists(savePathSlotLocalMulti);
            case 3:
                return File.Exists(savePathSlotOnline);
            default:
                return false;
        }
    }

    // Encritar los datos
    private static byte[] EncryptStringToBytes_Aes(string plainText)
    {
        byte[] encrypted;

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = aesKey;
            aesAlg.IV = aesIV;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
        }
        return encrypted;
    }

    // Descifrar los datos
    private static string DecryptStringFromBytes_Aes(byte[] cipherText)
    {
        string plaintext = null;

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = aesKey;
            aesAlg.IV = aesIV;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
        }
        return plaintext;
    }

}
