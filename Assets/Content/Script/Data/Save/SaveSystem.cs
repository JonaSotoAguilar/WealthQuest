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
    private static readonly string savePathSlotSingle = Path.Combine(saveDirectory, "game_Single.save");
    private static readonly string savePathSlotLocalMulti = Path.Combine(saveDirectory, "game_Local.save");
    private static readonly string savePathSlotOnline = Path.Combine(saveDirectory, "game_Online.save");

    // Contenido por defecto
    public static string defaultContentName = "Contenido Basico_1.content";

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

    public static IEnumerator SaveHistory(FinishGameData finishGameData, int slotData)
    {
        string json = JsonUtility.ToJson(finishGameData);
        byte[] encryptedData = EncryptStringToBytes_Aes(json);
        if (!Directory.Exists(historyDirectory)) Directory.CreateDirectory(historyDirectory);

        string historyPath = Path.Combine(historyDirectory, "game_" + PlayerPrefs.GetInt("gameId") + ".save");
        File.WriteAllBytes(historyPath, encryptedData);
        DeleteSave(slotData);

        ProfileUser.UpdateStats(finishGameData);

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

    public static void InitializateDefaultContent()
    {
        // Rutas de origen y destino
        string sourcePath = Path.Combine(Application.streamingAssetsPath, defaultContentName);

        if (!Directory.Exists(contentDirectory))
        {
            Directory.CreateDirectory(contentDirectory);
        }

        string destinationPath = Path.Combine(contentDirectory, defaultContentName);

        // Obtener el nombre base del archivo sin ruta y sin extensión
        string baseName = ExtractName(Path.GetFileNameWithoutExtension(defaultContentName));

        // Verificar si ya existe una versión en PersistentDataPath
        if (!CheckContentFile(baseName))
        {
            // Copiar el archivo desde StreamingAssets
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, destinationPath);
                Debug.Log($"Archivo {defaultContentName} copiado a: {destinationPath}");
            }
            else
            {
                Debug.LogError($"El archivo {defaultContentName} no se encuentra en StreamingAssets.");
            }
        }
    }
    public static IEnumerator SaveContent(QuestionList questionList, string name)
    {
        string json = JsonUtility.ToJson(questionList);
        byte[] encryptedData = EncryptStringToBytes_Aes(json);

        if (!Directory.Exists(contentDirectory))
        {
            Directory.CreateDirectory(contentDirectory);
        }

        // Extraer el nombre base y la versión (si existe)
        string baseName = ExtractName(name);
        int version = ExtractVersion(name);

        if (version <= 0)
        {
            // Si no hay versión o no es válida, asignamos la versión 1
            version = 1;
        }

        string contentPath = Path.Combine(contentDirectory, $"{baseName}_{version}.content");

        // Verificar si ya existe un archivo con el mismo nombre base y versión
        if (CheckContentFile(baseName))
        {
            Debug.LogWarning($"A content file with this name and version already exists: {baseName}_{version}");
            yield break;
        }

        // Guardar el archivo .content
        File.WriteAllBytes(contentPath, encryptedData);
        Debug.Log($"Archivo guardado en: {contentPath}");

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

    public static void LoadContentNames(List<string> names)
    {
        if (!Directory.Exists(contentDirectory))
        {
            Debug.LogWarning("Content directory does not exist. No topics found.");
            return;
        }

        string[] files = Directory.GetFiles(contentDirectory, "*.content");

        Array.Reverse(files);

        foreach (string file in files)
        {
            string nameWithVersion = Path.GetFileNameWithoutExtension(file);
            names.Add(nameWithVersion);
        }

        // Mover "Contenido Basico_1" al inicio de la lista si existe
        int index = names.FindIndex(name => name.Equals("Contenido Basico_1"));
        if (index != -1)
        {
            string contenidoBasico = names[index];
            names.RemoveAt(index);
            names.Insert(0, contenidoBasico); // Insertar al inicio
        }
    }

    public static IEnumerator LoadContent(QuestionList questionList, string name)
    {
        // Buscar archivos que coincidan con el patrón del nombre base
        string searchPattern = name + "_*.content";

        if (!Directory.Exists(contentDirectory))
        {
            Debug.LogError("Content directory not found: " + contentDirectory);
            yield break;
        }

        string[] files = Directory.GetFiles(contentDirectory, searchPattern);

        if (files.Length == 0)
        {
            Debug.LogError("Content file not found for name: " + name);
            yield break;
        }

        // Tomar el archivo más reciente (ordenar por fecha de última modificación)
        string contentPath = files.OrderByDescending(File.GetLastWriteTime).FirstOrDefault();

        if (string.IsNullOrEmpty(contentPath))
        {
            Debug.LogError("No valid content file found for name: " + name);
            yield break;
        }

        try
        {
            // Leer y desencriptar el contenido
            byte[] encryptedData = File.ReadAllBytes(contentPath);
            string decryptedData = DecryptStringFromBytes_Aes(encryptedData);

            // Envolver el JSON si es necesario
            if (decryptedData.Trim().StartsWith("["))
            {
                decryptedData = $"{{\"questions\":{decryptedData}}}";
            }

            // Deserializar los datos al objeto QuestionList
            JsonUtility.FromJsonOverwrite(decryptedData, questionList);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error al cargar el contenido desde {contentPath}: {ex.Message}");
        }

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

    public static string ExtractName(string contentName)
    {
        int underscoreIndex = contentName.LastIndexOf('_');
        if (underscoreIndex == -1)
        {
            return contentName;
        }

        return contentName.Substring(0, underscoreIndex);
    }

    public static int ExtractVersion(string contentName)
    {
        int underscoreIndex = contentName.LastIndexOf('_');
        if (underscoreIndex == -1)
        {
            return 0;
        }

        string versionString = contentName.Substring(underscoreIndex + 1);
        if (int.TryParse(versionString, out int version))
        {
            return version;
        }
        return 0;
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

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key,
            aesAlg.IV);

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
