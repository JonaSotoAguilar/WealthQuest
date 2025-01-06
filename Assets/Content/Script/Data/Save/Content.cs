using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using System.Linq;

[CreateAssetMenu(fileName = "Content", menuName = "Game/Topics")]
public class Content : ScriptableObject
{
    public const string GitHubContentUrl = "https://github.com/JonaSotoAguilar/WealthQuest/raw/Assets/Content";

    [SerializeField] private List<string> localContentList = new List<string>();
    [SerializeField] private List<string> remoteContentList = new List<string>();
    [SerializeField] private List<string> remoteContentUpdateList = new List<string>();
    [SerializeField] private List<string> allRemoteContentList = new List<string>();

    public List<string> LocalTopicList { get => localContentList; set => localContentList = value; }
    public List<string> RemoteTopicList { get => remoteContentList; set => remoteContentList = value; }
    public List<string> RemoteTopicUpdateList { get => remoteContentUpdateList; set => remoteContentUpdateList = value; }

    public IEnumerator InitializeContent()
    {
        InitializateLocalContent();
        yield return InitializateRemoteContent();

        // Revisa si tiene la última versión de los temas locales
        foreach (string localContent in localContentList)
        {
            remoteContentList.RemoveAll(item => item.StartsWith(localContent));
        }

        // Revisa actualizaciones disponibles
        remoteContentUpdateList.Clear();
        for (int i = remoteContentList.Count - 1; i >= 0; i--)
        {
            string remoteContent = remoteContentList[i];

            // Verificar si hay una actualización disponible para el contenido remoto
            string localContent = localContentList.FirstOrDefault(item =>
                SaveSystem.ExtractName(item) == SaveSystem.ExtractName(remoteContent));

            if (!string.IsNullOrEmpty(localContent) && IsUpdateAvailable(localContent, remoteContent))
            {
                Debug.Log($"Update available for: {localContent}");

                remoteContentUpdateList.Add(remoteContent);
                localContentList.Remove(localContent);
                remoteContentList.RemoveAt(i);
            }
        }
    }

    public void GetLocalContent()
    {
        if (localContentList.Count == 0) InitializateLocalContent();
    }

    public void InitializateLocalContent()
    {
        localContentList.Clear();
        SaveSystem.InitializateDefaultContent();
        SaveSystem.LoadContentNames(localContentList);
    }

    // FIXME: Método para obtener la lista de Content remotos desde un JSON alojado en GitHub
    public IEnumerator InitializateRemoteContent()
    {
        remoteContentList.Clear();
        allRemoteContentList.Clear();

        // URL de la API para obtener el contenido de la carpeta "Content" en la rama "Assets"
        string apiUrl = "https://api.github.com/repos/JonaSotoAguilar/WealthQuest/contents/Content?ref=Assets";

        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            request.SetRequestHeader("User-Agent", "UnityWebRequest"); // Requerido por GitHub

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error al obtener la lista de contenidos remotos: {request.error}");
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

    // FIXME: Método para descargar un Content desde GitHub
    public IEnumerator UpdateContent(string contentName)
    {
        // Obtener el nombre base del contenido (sin versión)
        string baseName = SaveSystem.ExtractName(contentName);
        SaveSystem.DeleteContent(SaveSystem.ExtractName(baseName));

        // Descargar la nueva versión
        yield return DownloadContent(contentName);

        // Actualizar las listas
        remoteContentUpdateList.Remove(contentName);
    }

    public IEnumerator DownloadContent(string contentName)
    {
        // Asegurarse de que el nombre incluye ".content"
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
                if (!Directory.Exists(contentDirectory))
                {
                    Directory.CreateDirectory(contentDirectory);
                }

                File.WriteAllBytes(localPath, request.downloadHandler.data);

                string baseName = Path.GetFileNameWithoutExtension(contentName);
                if (!localContentList.Contains(baseName))
                {
                    localContentList.Add(baseName);
                }

                string name = SaveSystem.ExtractName(contentName);
                if (remoteContentList.Any(item => SaveSystem.ExtractName(item) == name))
                {
                    remoteContentList.RemoveAll(item => SaveSystem.ExtractName(item) == name);
                }
            }
        }
    }

    public bool DeleteLocalContent(string contentName)
    {
        string baseName = SaveSystem.ExtractName(contentName);
        bool success = SaveSystem.DeleteContent(baseName);

        if (success)
        {
            localContentList.Remove(contentName);

            string remoteContent = allRemoteContentList.FirstOrDefault(item => SaveSystem.ExtractName(item) == baseName);

            if (!string.IsNullOrEmpty(remoteContent))
            {
                remoteContentList.Add(remoteContent);
                return true;
            }
        }

        return false;
    }

    private bool IsUpdateAvailable(string localContent, string remoteContent)
    {
        int localVersion = SaveSystem.ExtractVersion(localContent);
        int remoteVersion = SaveSystem.ExtractVersion(remoteContent);

        return remoteVersion > localVersion;
    }

    public bool ExistsContent(string name)
    {
        return localContentList.Any(item => item.StartsWith(name + "_") || item == name) ||
               remoteContentList.Any(item => item.StartsWith(name + "_") || item == name) ||
               remoteContentUpdateList.Any(item => item.StartsWith(name + "_") || item == name);
    }

}

[System.Serializable]
public class GitHubContent
{
    public string name;
    public string path;
    public string sha;
    public string url;
    public string git_url;
    public string html_url;
    public string download_url;
    public string type;
}

[System.Serializable]
public class GitHubContentWrapper
{
    public List<GitHubContent> content;
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string wrappedJson = $"{{\"Items\":{json}}}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
        return wrapper.Items;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}


