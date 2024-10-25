using UnityEngine;
using System.IO;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

public static class SaveSystem
{
    // Rutas de guardado
    public static readonly string savePathSlotLocalMulti = Application.persistentDataPath + "/game_01.json";
    public static readonly string savePathSlot2 = Application.persistentDataPath + "/game_02.json";
    public static readonly string savePathSlot3 = Application.persistentDataPath + "/game_03.json";

    // Clave de Encriptación AES
    // Cambia tu clave y IV a 16 bytes (128 bits)
    private static readonly byte[] aesKey = Encoding.UTF8.GetBytes("1234567890123456");  // 16 bytes exactos
    private static readonly byte[] aesIV = Encoding.UTF8.GetBytes("abcdefghijklmnop");   // 16 bytes exactos

    // Guardar el juego
    public static IEnumerator SaveGame(GameData data, int slot)
    {
        string json = JsonUtility.ToJson(data);
        byte[] encryptedData = EncryptStringToBytes_Aes(json);

        switch (slot)
        {
            case 1:
                File.WriteAllBytes(savePathSlotLocalMulti, encryptedData);
                PlayerPrefs.SetString("slotDate1", System.DateTime.Now.ToString());
                break;
            case 2:
                File.WriteAllBytes(savePathSlot2, encryptedData);
                PlayerPrefs.SetString("slotDate2", System.DateTime.Now.ToString());
                break;
            case 3:
                File.WriteAllBytes(savePathSlot3, encryptedData);
                PlayerPrefs.SetString("slotDate3", System.DateTime.Now.ToString());
                break;
            default:
                yield break;
        }

        yield return null;
    }

    // Cargar el juego
    public static IEnumerator LoadGame(GameData data, int slot)
    {
        byte[] encryptedData = null;

        switch (slot)
        {
            case 1:
                encryptedData = File.ReadAllBytes(savePathSlotLocalMulti);
                break;
            case 2:
                encryptedData = File.ReadAllBytes(savePathSlot2);
                break;
            case 3:
                encryptedData = File.ReadAllBytes(savePathSlot3);
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
}
