using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using System.Linq;
using System;

public static class ContentDatabase
{
    public static readonly string GitHubContentUrl = "https://github.com/JonaSotoAguilar/WealthQuest/raw/Assets/Content";

    // Contenidos local
    [SerializeField] public static List<Content> contentList = new List<Content>();
    [SerializeField] public static List<string> localContentList = new List<string>();

    // Contenidos remoto (Diccionario uid, (nombre, version))
    [SerializeField] public static List<string> allRemoteContentList = new List<string>();
    [SerializeField] public static List<string> remoteContentList = new List<string>();
    [SerializeField] public static List<string> updateContentList = new List<string>();

    // Contenidos remoto (Diccionario uid, (nombre, version))
    public static Dictionary<string, (string, int)> remoteContent = new Dictionary<string, (string, int)>();

    #region Initialization

    public static IEnumerator InitializeContent()
    {
        InitializateLocalContent();
        yield return InitializateRemoteContent();
        InitializateUpdateContent();
    }

    public static void InitializateLocalContent()
    {
        SaveService.LoadLocalContent();
    }

    private static IEnumerator InitializateRemoteContent()
    {
        remoteContentList.Clear();

        // URL de la API para obtener el contenido de la carpeta "Content" en la rama "Assets"
        string apiUrl = "https://api.github.com/repos/JonaSotoAguilar/WealthQuest/contents/Content?ref=Assets";

        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            request.SetRequestHeader("User-Agent", "UnityWebRequest");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log($"Error al obtener la lista de contenidos remotos: {request.error}");
            }
            else
            {
                // Procesar la respuesta JSON como una lista de objetos
                string jsonText = request.downloadHandler.text;
                GitHubContent[] contentArray = JsonHelper.FromJson<GitHubContent>(jsonText);

                foreach (var content in contentArray)
                {
                    if (content.name.EndsWith(".content"))
                    {
                        string contentNameWithoutExtension = Path.GetFileNameWithoutExtension(content.name);
                        remoteContentList.Add(contentNameWithoutExtension);
                        allRemoteContentList.Add(contentNameWithoutExtension);
                    }

                    yield return null;
                }
            }
        }
    }

    private static void InitializateUpdateContent()
    {
        updateContentList.Clear();

        // Si tengo el remoto en local, lo elimina de remoto.
        foreach (string localContent in localContentList)
        {
            remoteContentList.RemoveAll(item => item.StartsWith(localContent));
        }

        for (int i = remoteContentList.Count - 1; i >= 0; i--)
        {
            string remoteContent = remoteContentList[i];

            // Verificar si hay una actualización disponible para el contenido remoto
            string localContent = localContentList.FirstOrDefault(item =>
                SaveService.ExtractNameContent(item) == SaveService.ExtractNameContent(remoteContent));

            if (!string.IsNullOrEmpty(localContent) && IsUpdateAvailable(localContent, remoteContent))
            {
                updateContentList.Add(remoteContent);
                localContentList.Remove(localContent);
                remoteContentList.RemoveAt(i);
            }
        }
    }

    private static bool IsUpdateAvailable(string localContent, string remoteContent)
    {
        int localVersion = SaveService.ExtractVersionContent(localContent);
        int remoteVersion = SaveService.ExtractVersionContent(remoteContent);

        return remoteVersion > localVersion;
    }

    #endregion

    #region Content management

    public static IEnumerator DownloadUpdateContent(string contentName)
    {
        // Eliminar version antigua local
        string baseName = SaveService.ExtractNameContent(contentName);
        SaveService.DeleteContent(SaveService.ExtractNameContent(baseName));

        // Descargar la nueva versión
        yield return DownloadContent(contentName);
        updateContentList.Remove(contentName);
    }

    public static IEnumerator DownloadContent(string contentName)
    {
        if (!contentName.EndsWith(".content")) contentName += ".content";

        string url = $"{GitHubContentUrl}/{contentName}";
        string contentDirectory = Path.Combine(Application.persistentDataPath, "Content");
        string localPath = Path.Combine(contentDirectory, contentName);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error al descargar el contenido: {request.error} URL: {url}");
            }
            else
            {
                SaveService.ExistsDirectoryContent();
                File.WriteAllBytes(localPath, request.downloadHandler.data);

                // Agregar a local
                string baseName = Path.GetFileNameWithoutExtension(contentName);
                string name = SaveService.ExtractNameContent(baseName);
                if (!localContentList.Contains(baseName))
                {
                    localContentList.Add(baseName);
                    yield return SaveService.LoadContent(name);
                }

                // Eliminar de remoto
                if (remoteContentList.Any(item => SaveService.ExtractNameContent(item) == name))
                {
                    remoteContentList.RemoveAll(item => SaveService.ExtractNameContent(item) == name);
                }
            }
        }
    }

    public static bool DeleteLocalContent(string contentName)
    {
        string baseName = SaveService.ExtractNameContent(contentName);
        bool success = SaveService.DeleteContent(baseName);

        // Revisa si el contenido eliminado tiene versión remota
        if (success)
        {
            string remoteContent = allRemoteContentList.FirstOrDefault(item => SaveService.ExtractNameContent(item) == baseName);
            if (!string.IsNullOrEmpty(remoteContent)) remoteContentList.Add(remoteContent);
            return true;
        }

        return false;
    }

    #endregion

    #region Getters

    public static bool ExistsContent(string name)
    {
        return localContentList.Any(item => item.StartsWith(name + "_") || item == name) ||
               allRemoteContentList.Any(item => item.StartsWith(name + "_") || item == name);
    }

    public static Content GetContent(string name, int version)
    {
        return contentList.FirstOrDefault(item => item.name == name && item.version == version);
    }

    public static Content GetContent(string name)
    {
        return contentList.FirstOrDefault(item => item.name == name);
    }

    #endregion

    #region Import and export

    public static void ImportContentFile(string filePath)
    {
        try
        {
            // Leer el contenido del archivo JSON
            string jsonContent = File.ReadAllText(filePath);

            // Deserializar el JSON en una lista de preguntas
            List<Question> questions = JsonUtility.FromJson<QuestionList>($"{{\"questions\":{jsonContent}}}").questions;

            if (questions != null && questions.Count > 0)
            {
                Debug.Log($"Importadas {questions.Count} preguntas desde el archivo.");

                // Crear el objeto Content
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string[] nameParts = fileName.Split('_');

                string name = nameParts[0];
                int version = nameParts.Length > 1 && int.TryParse(nameParts[1], out int parsedVersion) ? parsedVersion : 1;

                Content newContent = new Content(name, questions, version);

                // Guardar el contenido usando SaveContent
                SaveService.SaveContent(newContent);

                // Mostrar mensaje de éxito
                MenuManager.Instance.OpenMessagePopup("Contenido importado con éxito.");
            }
            else
            {
                Debug.LogWarning("El archivo no contiene preguntas válidas o el formato no es correcto.");
                MenuManager.Instance.OpenMessagePopup("El archivo no contiene preguntas válidas.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al importar el archivo: {ex.Message}");
            MenuManager.Instance.OpenMessagePopup($"Error al importar el archivo: {ex.Message}");
        }
    }

    public static void ImportJson(string filePath)
    {
        try
        {
            // Leer el contenido del archivo JSON
            string jsonContent = File.ReadAllText(filePath);

            // Deserializar el JSON en una lista de preguntas
            List<Question> questions = JsonUtility.FromJson<QuestionList>($"{{\"questions\":{jsonContent}}}").questions;

            if (questions != null && questions.Count > 0)
            {
                // Crear el contenido con el nombre del archivo
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string[] nameParts = fileName.Split('_');

                string name = nameParts[0];
                int version = nameParts.Length > 1 && int.TryParse(nameParts[1], out int parsedVersion) ? parsedVersion : 1;

                // Crear el nuevo objeto Content
                Content newContent = new Content(name, questions, version);

                // Guardar el contenido utilizando SaveContent
                SaveService.SaveContent(newContent);
                MenuManager.Instance.OpenMessagePopup("Contenido creado con éxito.");
            }
            else
            {
                Debug.LogWarning("El JSON no contiene preguntas o el formato no es válido.");
                MenuManager.Instance.OpenMessagePopup("El JSON no contiene preguntas válidas.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error al importar el archivo JSON: {ex.Message}");
            MenuManager.Instance.OpenMessagePopup($"Error al importar el archivo JSON: {ex.Message}");
        }
    }

    #endregion



}