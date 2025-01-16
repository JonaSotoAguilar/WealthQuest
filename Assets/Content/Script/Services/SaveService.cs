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
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public static class SaveService
{
    // Rutas de guardado
    public static readonly string saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
    public static readonly string historyDirectory = Path.Combine(Application.persistentDataPath, "History");
    public static readonly string contentDirectory = Path.Combine(Application.persistentDataPath, "Content");
    private static readonly string savePathSlotSingle = Path.Combine(saveDirectory, "game_Single.save");
    private static readonly string savePathSlotLocalMulti = Path.Combine(saveDirectory, "game_Local.save");
    private static readonly string savePathSlotOnline = Path.Combine(saveDirectory, "game_Online.save");

    // Contenido por defecto
    public static string defaultContentName = "Contenido Basico";

    // Clave de Encriptación AES
    private static readonly byte[] aesKey = Encoding.UTF8.GetBytes("1234567890123456");
    private static readonly byte[] aesIV = Encoding.UTF8.GetBytes("abcdefghijklmnop");

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

    public static async Task SaveHistory(FinishGameData finishGameData, int slotData)
    {
        string json = JsonUtility.ToJson(finishGameData);
        byte[] encryptedData = EncryptStringToBytes_Aes(json);

        // Crear el directorio si no existe
        if (!Directory.Exists(historyDirectory))
            Directory.CreateDirectory(historyDirectory);

        // Guardar el archivo
        string historyPath = Path.Combine(historyDirectory, "game_" + finishGameData.gameID + ".save");
        await Task.Run(() => File.WriteAllBytes(historyPath, encryptedData));

        // Eliminar el guardado anterior (si aplica)
        DeleteSave(slotData);

        // Actualizar estadísticas del usuario
        if (slotData != 0)
        {
            PlayerPrefs.SetInt("gameId", finishGameData.gameID);
            ProfileUser.UpdateStats(finishGameData);
        }
    }

    public static async Task LoadHistory()
    {
        if (Directory.Exists(historyDirectory))
        {
            // Obtener todos los archivos .save
            string[] files = Directory.GetFiles(historyDirectory, "*.save");

            // Invertir el orden de los archivos
            Array.Reverse(files);

            // Leer y desencriptar cada archivo
            foreach (string file in files)
            {
                byte[] encryptedData = await Task.Run(() => File.ReadAllBytes(file));
                string decryptedData = DecryptStringFromBytes_Aes(encryptedData);
                FinishGameData finishGameData = JsonUtility.FromJson<FinishGameData>(decryptedData);
                ProfileUser.history.Add(finishGameData);
            }
        }
    }

    #endregion

    #region Save and Load Content

    public static void LoadLocalContent()
    {
        ExistsDirectoryContent();
        InitializateDefaultContent();
        string[] files = Directory.GetFiles(contentDirectory, "*.content");
        Array.Reverse(files);
        foreach (var file in files)
        {
            byte[] encryptedData = File.ReadAllBytes(file);
            string decryptedData = DecryptStringFromBytes_Aes(encryptedData);
            Content content = new Content();
            JsonUtility.FromJsonOverwrite(decryptedData, content);
            string name = content.name;
            int version = content.version;
            string nameWithVersion = Path.GetFileNameWithoutExtension(file);

            // Guardar el contenido en la lista
            if (!ContentData.contentList.Any(c => c.uid == content.uid))
            {
                ContentData.contentList.Add(content);
                ContentData.localContentList.Add(nameWithVersion);
            }
        }

        // Posicionar contenido basico como primero
        int index = ContentData.localContentList.FindIndex(name => Regex.IsMatch(name, @"^Contenido Basico_\d+$"));
        if (index != 0 && index != -1)
        {
            string contenidoBasico = ContentData.localContentList[index];
            ContentData.localContentList.RemoveAt(index);
            ContentData.localContentList.Insert(0, contenidoBasico);

            Content contentDefault = ContentData.contentList.Find(content => content.name == defaultContentName);
            if (contentDefault != null)
            {
                ContentData.contentList.Remove(contentDefault);
                ContentData.contentList.Insert(0, contentDefault);
            }
        }
    }

    private static void InitializateDefaultContent()
    {
        ExistsDirectoryContent();

        // Rutas de origen y destino
        string fileName = defaultContentName + "_1.content";
        string sourcePath = Path.Combine(Application.streamingAssetsPath, fileName);
        string destinationPath = Path.Combine(contentDirectory, fileName);

        // Verificar si ya existe una versión en PersistentDataPath
        if (!CheckContentFile(defaultContentName))
        {
            // Copiar el archivo desde StreamingAssets
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, destinationPath, overwrite: true);
                Debug.Log($"Archivo {fileName} copiado a: {destinationPath}");
            }
            else
            {
                Debug.LogError($"El archivo de origen no existe: {sourcePath}");
            }
        }
    }

    public static void SaveContent(Content content)
    {
        ExistsDirectoryContent();

        // Serializar el contenido a JSON y cifrarlo
        string json = JsonUtility.ToJson(content);
        byte[] encryptedData = EncryptStringToBytes_Aes(json);

        // Buscar en contentList por uid y actualizar
        string uid = content.uid;
        Content existingContent = ContentData.contentList.Find(c => c.uid == content.uid);
        if (existingContent != null)
        {
            if (existingContent.name == content.name &&
                existingContent.questions.SequenceEqual(content.questions))
            {
                Debug.Log("El contenido es exactamente igual al existente.");
                return;
            }

            // Guardar la ruta del archivo de la versión anterior
            string oldContentPath = Path.Combine(contentDirectory, $"{existingContent.name}_{existingContent.version}.content");

            // Actualizar contenido existente
            Debug.Log($"Versión actualizada a: {content.version}");
            existingContent.name = content.name;
            existingContent.version = content.version;
            existingContent.questions = new List<QuestionData>(content.questions);

            // Actualizar la lista localContentList
            int index = ContentData.localContentList.FindIndex(name => name.StartsWith($"{content.name}_"));
            ContentData.localContentList[index] = $"{content.name}_{content.version}";

            // Eliminar la versión anterior del archivo
            if (File.Exists(oldContentPath))
            {
                File.Delete(oldContentPath);
                Debug.Log($"Versión antigua eliminada: {oldContentPath}");
            }
        }
        else
        {
            // Crear nuevo contenido
            Debug.Log("Nuevo contenido guardado.");
            ContentData.contentList.Add(content);
            ContentData.localContentList.Add($"{content.name}_{content.version}");
        }

        // Guardar el contenido en el directorio
        string contentPath = Path.Combine(contentDirectory, $"{content.name}_{content.version}.content");
        File.WriteAllBytes(contentPath, encryptedData);
    }

    public static IEnumerator LoadContent(string name)
    {
        ExistsDirectoryContent();

        string searchPattern = name + "_*.content";
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
            Content content = new Content();
            JsonUtility.FromJsonOverwrite(decryptedData, content);
            ContentData.contentList.Add(content);
        }
        catch (Exception ex)
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
            string nameWithVersion = Path.GetFileNameWithoutExtension(file);
            ContentData.localContentList.RemoveAll(localContent => localContent == nameWithVersion);

            string baseName = ExtractNameContent(nameWithVersion);
            int version = ExtractVersionContent(nameWithVersion);
            ContentData.contentList.RemoveAll(content => content.name == baseName && content.version == version);

            File.Delete(file);
        }

        return files.Length > 0;
    }

    public static void ExportContentFile(string name)
    {
        // Buscar archivos que coincidan con el patrón del nombre base
        string searchPattern = $"{name}_*.content";
        string[] files = Directory.GetFiles(contentDirectory, searchPattern);

        // Validar si no hay archivos encontrados
        if (files.Length == 0)
        {
            Debug.LogError($"No content file found for name: {name}");
            return;
        }

        // Validar si hay más de un archivo que coincida
        string fileToExport = null;
        foreach (var file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string[] parts = fileName.Split('_');
            if (parts.Length == 2 && parts[0] == name && int.TryParse(parts[1], out _))
            {
                fileToExport = file;
                break;
            }
        }

        if (fileToExport == null)
        {
            Debug.LogError($"No valid file found for name: {name}");
            return;
        }

        string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        if (!Directory.Exists(downloadsPath))
        {
            Debug.LogError("Downloads folder not found.");
            return;
        }

        string exportPath = Path.Combine(downloadsPath, Path.GetFileName(fileToExport));
        File.Copy(fileToExport, exportPath, overwrite: true);

        Debug.Log($"Content file exported to: {exportPath}");
    }

    public static string ExtractNameContent(string contentName)
    {
        int underscoreIndex = contentName.LastIndexOf('_');
        if (underscoreIndex == -1)
        {
            return contentName;
        }

        return contentName.Substring(0, underscoreIndex);
    }

    public static int ExtractVersionContent(string contentName)
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

    public static void ExistsDirectoryContent()
    {
        if (!Directory.Exists(contentDirectory))
        {
            Directory.CreateDirectory(contentDirectory);
        }
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
