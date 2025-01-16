using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using System.Linq;
using Unity.VisualScripting;

public static class ContentData
{
    // FIXME: Cambiar a rama Fintual
    public static readonly string GitHubContentUrl = "https://github.com/JonaSotoAguilar/WealthQuest/raw/Assets/Content";

    // Contenidos local
    [SerializeField] public static List<Content> contentList = new List<Content>();
    [SerializeField] public static List<string> localContentList = new List<string>();

    // Contenidos remoto
    [SerializeField] public static List<string> allRemoteContentList = new List<string>();
    [SerializeField] public static List<string> remoteContentList = new List<string>();
    [SerializeField] public static List<string> updateContentList = new List<string>();

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

}