using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class TopicsMenu : MonoBehaviour
{
    [SerializeField] private Topics topics;
    [SerializeField] private GameObject topicPrefab;
    [SerializeField] private Transform container;
    [SerializeField] private TMP_InputField searchInput;

    private enum FilterMode { All, NotDownloaded, Downloaded }
    private FilterMode currentFilterMode = FilterMode.All;

    private void Start()
    {
        InitScrollView();
        searchInput.onValueChanged.AddListener(delegate { FilterBySearch(); });
    }

    public void InitScrollView()
    {
        ClearScrollView();
        StartCoroutine(InitializeAndPopulateScrollView());
    }

    // Inicia la carga de los temas y llena el ScrollView de forma incremental
    private IEnumerator InitializeAndPopulateScrollView()
    {
        yield return StartCoroutine(topics.InicializateRemoteTopics((bundleName) =>
        {
            CreateTopicPanel(bundleName);
        }));
    }

    // Crea un panel para cada tópico
    private void CreateTopicPanel(string bundleName)
    {
        GameObject newPanel = Instantiate(topicPrefab, container);

        newPanel.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = bundleName;
        bool isDownloaded = topics.LocalTopicList.Contains(bundleName);

        GameObject downloadButton = newPanel.transform.Find("Download").gameObject;
        GameObject downloadedButton = newPanel.transform.Find("Downloaded").gameObject;
        GameObject deleteButton = newPanel.transform.Find("Delete").gameObject;

        downloadButton.SetActive(!isDownloaded);
        downloadedButton.SetActive(isDownloaded);
        deleteButton.SetActive(isDownloaded);

        downloadButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
             StartCoroutine(DownloadBundle(bundleName, newPanel)));
        deleteButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
        {
            DeleteLocalTopic(bundleName, newPanel);
        });

        string searchText = searchInput.text.ToLower();
        string panelName = bundleName.ToLower();
        bool isVisible = panelName.Contains(searchText) && IsPanelVisible(isDownloaded);
        newPanel.SetActive(isVisible);
    }

    // Método para descargar un bundle y actualizar la UI
    private IEnumerator DownloadBundle(string bundleName, GameObject topicPanel)
    {
        topicPanel.transform.Find("Download").GetComponent<UnityEngine.UI.Button>().interactable = false;
        yield return StartCoroutine(topics.DownloadAssetBundle(bundleName));

        topicPanel.transform.Find("Download").gameObject.SetActive(false);
        topicPanel.transform.Find("Downloaded").gameObject.SetActive(true);
        topicPanel.transform.Find("Delete").gameObject.SetActive(true);

        RefreshPanels();
        FilterBySearch();
    }

    private void DeleteLocalTopic(string bundleName, GameObject topicPanel)
    {
        bool success = topics.DeleteLocalTopic(bundleName);

        if (success)
        {
            topicPanel.transform.Find("Download").gameObject.SetActive(true);
            topicPanel.transform.Find("Download").GetComponent<UnityEngine.UI.Button>().interactable = true;
            topicPanel.transform.Find("Downloaded").gameObject.SetActive(false);
            topicPanel.transform.Find("Delete").gameObject.SetActive(false);

            RefreshPanels();
            FilterBySearch();
        }
        else
        {
            Debug.LogError($"Error al eliminar el tópico {bundleName}. No se encontró el archivo.");
        }
    }


    public void ToggleFilterMode()
    {
        currentFilterMode = (FilterMode)(((int)currentFilterMode + 1) % 3);
        RefreshPanels();
        FilterBySearch();
    }

    // Refresca los paneles según el filtro actual
    private void RefreshPanels()
    {
        foreach (Transform child in container)
        {
            bool isDownloaded = child.Find("Downloaded").gameObject.activeSelf;
            child.gameObject.SetActive(IsPanelVisible(isDownloaded));
        }
    }

    // Devuelve si el panel debe ser visible según el filtro actual
    private bool IsPanelVisible(bool isDownloaded)
    {
        switch (currentFilterMode)
        {
            case FilterMode.All:
                return true;
            case FilterMode.NotDownloaded:
                return !isDownloaded;
            case FilterMode.Downloaded:
                return isDownloaded;
            default:
                return true;
        }
    }

    // Filtra los paneles por el texto en el campo de búsqueda
    private void FilterBySearch()
    {
        string searchText = searchInput.text.ToLower();

        foreach (Transform child in container)
        {
            string panelName = child.Find("Name").GetComponent<TextMeshProUGUI>().text.ToLower();
            child.gameObject.SetActive(panelName.Contains(searchText) && IsPanelVisible(child.Find("Downloaded").gameObject.activeSelf));
        }
    }

    // Limpiar los paneles existentes del ScrollView
    public void ClearScrollView()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
