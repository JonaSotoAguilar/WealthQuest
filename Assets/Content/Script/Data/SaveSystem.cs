using UnityEngine;
using System.IO;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
using Mirror;

public static class SaveSystem
{
    // Rutas de guardado
    public static readonly string saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
    public static readonly string historyDirectory = Path.Combine(Application.persistentDataPath, "History");
    public static readonly string contentDirectory = Path.Combine(Application.persistentDataPath, "Content");
    public static readonly string savePathSlotSingle = Path.Combine(saveDirectory, "game_Single.save");
    public static readonly string savePathSlotLocalMulti = Path.Combine(saveDirectory, "game_Local.save");
    public static readonly string savePathSlotOnline = Path.Combine(saveDirectory, "game_Online.save");

    // Clave de Encriptación AES
    private static readonly byte[] aesKey = Encoding.UTF8.GetBytes("1234567890123456");  // 16 bytes exactos
    private static readonly byte[] aesIV = Encoding.UTF8.GetBytes("abcdefghijklmnop");   // 16 bytes exactos

    #region Save and Load Game

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

    #endregion

    #region Save and Load History

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

    #endregion

    #region Save and Load Content

    private static bool CheckContentFile(string name)
    {
        string searchPattern = name + "_*.content";

        if (!Directory.Exists(contentDirectory))
        {
            Directory.CreateDirectory(contentDirectory);
        }

        string[] files = Directory.GetFiles(contentDirectory, searchPattern);
        return files.Length > 0;
    }

    public static IEnumerator SaveContent(QuestionList questionList, string name)
    {
        string json = JsonUtility.ToJson(questionList);
        byte[] encryptedData = EncryptStringToBytes_Aes(json);

        if (!Directory.Exists(contentDirectory))
        {
            Directory.CreateDirectory(contentDirectory);
        }

        // Verificar si ya existe un archivo con el mismo nombre base
        if (CheckContentFile(name))
        {
            Debug.LogWarning("A content file with this name already exists: " + name);
            yield break;
        }

        string contentPath = Path.Combine(contentDirectory, $"{name}_1.content");
        File.WriteAllBytes(contentPath, encryptedData);

        yield return null;
    }

    public static IEnumerator UpdateContent(QuestionList questionList, string name, int version)
    {
        version++;
        string json = JsonUtility.ToJson(questionList);
        byte[] encryptedData = EncryptStringToBytes_Aes(json);

        if (!Directory.Exists(contentDirectory))
        {
            Directory.CreateDirectory(contentDirectory);
        }

        // Eliminar el archivo existente si lo hay
        string searchPattern = name + "_*.content";
        string[] files = Directory.GetFiles(contentDirectory, searchPattern);
        foreach (var file in files)
        {
            File.Delete(file);
        }

        string contentPath = Path.Combine(contentDirectory, $"{name}_{version}.content");
        File.WriteAllBytes(contentPath, encryptedData);

        yield return null;
    }

    public static IEnumerator LoadContentNames(List<string> names)
    {
        if (!Directory.Exists(contentDirectory))
        {
            Debug.LogWarning("Content directory does not exist. No topics found.");
            yield break;
        }

        string[] files = Directory.GetFiles(contentDirectory, "*.content");

        Array.Reverse(files);

        foreach (string file in files)
        {
            string nameWithVersion = Path.GetFileNameWithoutExtension(file);
            names.Add(nameWithVersion);
        }

        yield return null;
    }

    public static IEnumerator LoadContent(QuestionList questionList, string name)
    {
        // Buscar archivos que coincidan con el patrón del nombre base
        string searchPattern = name + "_*.content";
        string[] files = Directory.GetFiles(contentDirectory, searchPattern);

        if (files.Length == 0)
        {
            Debug.LogError("Content file not found for name: " + name);
            yield break;
        }

        // Tomar el archivo más reciente (por ejemplo, basado en la última modificación)
        string contentPath = files.OrderByDescending(File.GetLastWriteTime).FirstOrDefault();

        if (contentPath == null)
        {
            Debug.LogError("No valid content file found for name: " + name);
            yield break;
        }

        byte[] encryptedData = File.ReadAllBytes(contentPath);
        string decryptedData = DecryptStringFromBytes_Aes(encryptedData);

        JsonUtility.FromJsonOverwrite(decryptedData, questionList);

        yield return null;
    }

    public static bool DeleteContent(string name)
    {
        string searchPattern = name + "_*.content";
        string[] files = Directory.GetFiles(contentDirectory, searchPattern);

        foreach (var file in files)
        {
            File.Delete(file);
        }

        return files.Length > 0;
    }

    public static void ExportContentFile(string name)
    {
        // Buscar un archivo que coincida con el patrón del nombre base
        string searchPattern = name + "_*.content";
        string[] files = Directory.GetFiles(contentDirectory, searchPattern);

        if (files.Length == 0)
        {
            Debug.LogError("No content file found for name: " + name);
            return;
        }

        if (files.Length > 1)
        {
            Debug.LogError($"Multiple files found for name: {name}. Ensure only one version exists.");
            return;
        }

        string contentPath = files[0];

        string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        if (!Directory.Exists(downloadsPath))
        {
            Debug.LogError("Downloads folder not found.");
            return;
        }

        string fileName = Path.GetFileName(contentPath);
        string exportPath = Path.Combine(downloadsPath, fileName);

        File.Copy(contentPath, exportPath, overwrite: true);
        Debug.Log("Content file exported to: " + exportPath);
    }

    #endregion

    #region Encrypt and Decrypt

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

    #endregion

}
