using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine.Networking;


[System.Serializable]
public class AssetBundleList
{
    public List<string> bundles;
}

[CreateAssetMenu(fileName = "Topics", menuName = "Topics")]
public class Topics : ScriptableObject
{
    [SerializeField] private List<string> localTopicList;
    [SerializeField] private List<string> remoteTopicList;
    private string assetBundleDirectory;
    private const string GitHubBaseUrl = "https://github.com/JonaSotoAguilar/WealthQuest/raw/Assets";

    public List<string> LocalTopicList { get => localTopicList; set => localTopicList = value; }
    public List<string> RemoteTopicList { get => remoteTopicList; set => remoteTopicList = value; }

    private void OnEnable()
    {
        localTopicList = new List<string>();
        remoteTopicList = new List<string>();
        assetBundleDirectory = Path.Combine(Application.persistentDataPath, "AssetBundles");

        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        InicializateLocalTopics();
    }

    // Método para inicializar los temas locales
    private void InicializateLocalTopics()
    {
        string[] files = Directory.GetFiles(assetBundleDirectory, "*");

        localTopicList.Clear();
        foreach (string file in files)
        {
            string bundleName = Path.GetFileNameWithoutExtension(file);
            if (Path.GetExtension(file) == string.Empty && bundleName != "AssetBundles" && bundleName != "defaultbundle")
            {
                localTopicList.Add(bundleName);
            }
        }
    }

    // Método para obtener la lista de temas remotos desde un JSON alojado en GitHub
    public IEnumerator InicializateRemoteTopics(System.Action<string> onTopicLoaded)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(GitHubBaseUrl + "/assetBundles.json"))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error al descargar la lista de Asset Bundles: {request.error}");
            }
            else
            {
                string jsonText = request.downloadHandler.text;
                AssetBundleList bundleList = JsonUtility.FromJson<AssetBundleList>(jsonText);
                remoteTopicList.Clear();

                // Procesar cada bundle de la lista
                foreach (string bundleName in bundleList.bundles)
                {
                    remoteTopicList.Add(bundleName);
                    onTopicLoaded?.Invoke(bundleName);
                    yield return null;
                }
            }
        }
    }

    // Método para descargar un Asset Bundle desde GitHub
    public IEnumerator DownloadAssetBundle(string bundleName)
    {
        string url = $"{GitHubBaseUrl}/{bundleName}";
        string localPath = Path.Combine(assetBundleDirectory, bundleName);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error al descargar el Asset Bundle: {request.error}");
            }
            else
            {
                File.WriteAllBytes(localPath, request.downloadHandler.data);
                Debug.Log($"Asset Bundle descargado y guardado en: {localPath}");

                // Agregar el nombre del bundle a la lista de temas locales
                if (!localTopicList.Contains(bundleName))
                {
                    localTopicList.Add(bundleName);
                }
            }
        }
    }

    public bool DeleteLocalTopic(string bundleName)
    {
        string localPath = Path.Combine(assetBundleDirectory, bundleName);

        // Verificar si el archivo existe
        if (File.Exists(localPath))
        {
            File.Delete(localPath);
            Debug.Log($"Archivo {bundleName} eliminado localmente.");
            localTopicList.Remove(bundleName);
            return true;
        }

        return false;
    }
}
