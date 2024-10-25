using UnityEngine;
using System.IO;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public static class SaveSystem
{
    // Rutas de guardado
    public static readonly string saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
    public static readonly string savePathSlotSingle = Path.Combine(saveDirectory, "game_Single.save");
    public static readonly string savePathSlotLocalMulti = Path.Combine(saveDirectory, "game_Local.save");
    public static readonly string savePathSlotOnline = Path.Combine(saveDirectory, "game_Online.save");

    // Clave de Encriptación AES
    private static readonly byte[] aesKey = Encoding.UTF8.GetBytes("1234567890123456");  // 16 bytes exactos
    private static readonly byte[] aesIV = Encoding.UTF8.GetBytes("abcdefghijklmnop");   // 16 bytes exactos

    private static int slotData;

    public static int SlotData { set => slotData = value; }

    // Guardar el juego
    public static IEnumerator SaveGame(GameData data)
    {
        string json = JsonUtility.ToJson(data);
        byte[] encryptedData = EncryptStringToBytes_Aes(json);

        if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);

        Debug.Log("Guardando en la ranura: " + slotData);

        switch (slotData)
        {
            case 1:
                File.WriteAllBytes(savePathSlotSingle, encryptedData);
                PlayerPrefs.SetString("slotDate1", System.DateTime.Now.ToString());
                break;
            case 2:
                File.WriteAllBytes(savePathSlotLocalMulti, encryptedData);
                PlayerPrefs.SetString("slotDate2", System.DateTime.Now.ToString());
                break;
            case 3:
                File.WriteAllBytes(savePathSlotOnline, encryptedData);
                PlayerPrefs.SetString("slotDate3", System.DateTime.Now.ToString());
                break;
            default:
                yield break;
        }

        yield return null;
    }

    // Cargar el juego
    public static IEnumerator LoadGame(GameData data)
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
}
