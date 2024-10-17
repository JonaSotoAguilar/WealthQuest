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

public class TopicsLoader : MonoBehaviour
{
    [SerializeField] private string assetBundleDirectory; // Carpeta de los Asset Bundles
    [SerializeField] private List<string> localTopicList; // Lista de bundles locales
    [SerializeField] private List<string> remoteTopicList; // Lista de bundles disponibles en GitHub
    private const string GitHubBaseUrl = "https://github.com/JonaSotoAguilar/WealthQuest/raw/Assets";

    public List<string> LocalTopicList { get => localTopicList; }
    public List<string> RemoteTopicList { get => remoteTopicList; }
    public string AssetBundleDirectory { get => assetBundleDirectory; set => assetBundleDirectory = value; }

    private void Awake()
    {
        localTopicList = new List<string>();
        remoteTopicList = new List<string>();
        assetBundleDirectory = Path.Combine(Application.persistentDataPath, "AssetBundles");

        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        StartCoroutine(InicializateTopics());
    }

    // Método de inicialización de los temas desde GitHub
    public IEnumerator InicializateTopics()
    {
        // Obtener lista de bundles locales
        InicializateLocalTopics();

        // Obtener la lista de bundles remotos desde el archivo JSON
        yield return StartCoroutine(FetchRemoteTopicList());
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
    private IEnumerator FetchRemoteTopicList()
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
                remoteTopicList = bundleList.bundles;
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
}
