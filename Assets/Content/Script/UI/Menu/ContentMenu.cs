using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using SFB;
using System.IO;

public class ContentMenu : MonoBehaviour
{
    [Header("Content")]
    [SerializeField] private Content content;
    [SerializeField] private CreateContent createContent;
    [SerializeField] private GameObject topicPrefab;
    [SerializeField] private Transform container;
    [SerializeField] private TMP_InputField searchInput;

    [Header("Filter")]
    [SerializeField] private Texture[] filterIcons;
    [SerializeField] private RawImage filterIcon;

    private enum FilterMode { All, Local, Remote, Update }
    private FilterMode currentFilterMode = FilterMode.All;

    #region Initialization

    private void Start()
    {
        searchInput.onValueChanged.AddListener(delegate { FilterBySearch(); });
    }

    private void OnEnable()
    {
        InitScrollView();
    }

    #endregion

    public void InitScrollView()
    {
        ClearScrollView();
        StartCoroutine(InitializeContent());
    }

    private IEnumerator InitializeContent()
    {
        yield return content.InitializeContent();

        foreach (string bundleName in content.RemoteTopicUpdateList)
        {
            CreateContentPanel(bundleName, true, true);
        }

        foreach (string bundleName in content.RemoteTopicList)
        {
            CreateContentPanel(bundleName, false);
        }

        foreach (string bundleName in content.LocalTopicList)
        {
            CreateContentPanel(bundleName);
        }
    }

    private void CreateContentPanel(string bundleName, bool isLocal = true, bool isUpdate = false)
    {
        GameObject newPanel = Instantiate(topicPrefab, container);

        string name = SaveSystem.ExtractName(bundleName);
        int version = SaveSystem.ExtractVersion(bundleName);
        newPanel.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = name;
        newPanel.transform.Find("Version").GetComponent<TextMeshProUGUI>().text = version + ".0";

        GameObject downloadButton = newPanel.transform.Find("Download").gameObject;
        GameObject downloadedButton = newPanel.transform.Find("Downloaded").gameObject;
        GameObject deleteButton = newPanel.transform.Find("Delete").gameObject;
        GameObject updateButton = newPanel.transform.Find("Update").gameObject;

        GameObject changeButton = newPanel.transform.Find("Change").gameObject;
        GameObject exportButton = newPanel.transform.Find("Export").gameObject;

        if (isUpdate)
        {
            deleteButton.SetActive(true);
            downloadButton.SetActive(false);
            downloadedButton.SetActive(false);
            updateButton.SetActive(true);
            changeButton.SetActive(false);
            exportButton.SetActive(false);
        }
        else
        {
            if (name != SaveSystem.defaultContentName) deleteButton.SetActive(isLocal);
            downloadButton.SetActive(!isLocal);
            downloadedButton.SetActive(isLocal);
            updateButton.SetActive(false);
            changeButton.SetActive(isLocal);
            exportButton.SetActive(isLocal);
        }

        downloadButton.GetComponent<Button>().onClick.AddListener(() =>
             StartCoroutine(DownloadBundle(bundleName, newPanel)));
        deleteButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            DeleteLocalTopic(bundleName, newPanel);
        });
        updateButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            StartCoroutine(UpdateContent(bundleName, newPanel));
        });
        changeButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            createContent.ChangeContent(name, version);
        });
        exportButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            SaveSystem.ExportContentFile(name);
            MenuManager.Instance.OpenMessagePopup("Contenido exportado con éxito en Descargas.");
        });

        ShowContent(isLocal, isUpdate, newPanel, searchInput.text.ToLower(), name.ToLower());
    }

    private void ShowContent(bool isLocal, bool isUpdate, GameObject newPanel, string searchText, string panelName)
    {
        bool isVisible = panelName.Contains(searchText) && IsPanelVisible(isLocal, isUpdate);
        newPanel.SetActive(isVisible);
    }

    private IEnumerator DownloadBundle(string contentName, GameObject contentPanel)
    {
        contentPanel.transform.Find("Download").GetComponent<Button>().interactable = false;
        yield return StartCoroutine(content.DownloadContent(contentName));

        contentPanel.transform.Find("Download").gameObject.SetActive(false);
        contentPanel.transform.Find("Downloaded").gameObject.SetActive(true);
        contentPanel.transform.Find("Delete").gameObject.SetActive(true);

        contentPanel.transform.Find("Change").gameObject.SetActive(true);
        contentPanel.transform.Find("Export").gameObject.SetActive(true);

        ShowContent(true, false, contentPanel, searchInput.text.ToLower(), contentPanel.name.ToLower());
    }

    private IEnumerator UpdateContent(string contentName, GameObject contentPanel)
    {
        contentPanel.transform.Find("Update").GetComponent<Button>().interactable = false;
        yield return StartCoroutine(content.UpdateContent(contentName));

        contentPanel.transform.Find("Update").gameObject.SetActive(false);
        contentPanel.transform.Find("Downloaded").gameObject.SetActive(true);
        contentPanel.transform.Find("Delete").gameObject.SetActive(true);

        contentPanel.transform.Find("Change").gameObject.SetActive(true);
        contentPanel.transform.Find("Export").gameObject.SetActive(true);

        ShowContent(true, false, contentPanel, searchInput.text.ToLower(), contentPanel.name.ToLower());
    }

    private void DeleteLocalTopic(string contentName, GameObject contentPanel)
    {
        bool success = content.DeleteLocalContent(contentName);

        if (success)
        {
            contentPanel.transform.Find("Download").gameObject.SetActive(true);
            contentPanel.transform.Find("Download").GetComponent<Button>().interactable = true;
            contentPanel.transform.Find("Downloaded").gameObject.SetActive(false);
            contentPanel.transform.Find("Delete").gameObject.SetActive(false);
            contentPanel.transform.Find("Update").gameObject.SetActive(false);

            contentPanel.transform.Find("Change").gameObject.SetActive(false);
            contentPanel.transform.Find("Export").gameObject.SetActive(false);

            RefreshPanels();
            FilterBySearch();
        }
        else
        {
            // Elimado de local pero no existe en remoto
            Destroy(contentPanel);
        }
    }

    public void ToggleFilterMode()
    {
        currentFilterMode = (FilterMode)(((int)currentFilterMode + 1) % 4);
        filterIcon.texture = filterIcons[(int)currentFilterMode];
        RefreshPanels();
        FilterBySearch();
    }
    private void RefreshPanels()
    {
        foreach (Transform child in container)
        {
            bool isLocal = child.Find("Downloaded").gameObject.activeSelf;
            bool isUpdate = child.Find("Update").gameObject.activeSelf;
            child.gameObject.SetActive(IsPanelVisible(isLocal, isUpdate));
        }
    }

    private bool IsPanelVisible(bool isDownloaded, bool isUpdate)
    {
        switch (currentFilterMode)
        {
            case FilterMode.All:
                return true;
            case FilterMode.Remote:
                if (isUpdate) return false;
                return !isDownloaded;
            case FilterMode.Local:
                if (isUpdate) return false;
                return isDownloaded;
            case FilterMode.Update:
                return isUpdate;
            default:
                return true;
        }
    }

    private void FilterBySearch()
    {
        string searchText = searchInput.text.ToLower();

        foreach (Transform child in container)
        {
            string panelName = child.Find("Name").GetComponent<TextMeshProUGUI>().text.ToLower();
            bool isLocal = child.Find("Downloaded").gameObject.activeSelf;
            bool isUpdate = child.Find("Update").gameObject.activeSelf;
            child.gameObject.SetActive(panelName.Contains(searchText) && IsPanelVisible(isLocal, isUpdate));
        }
    }

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

    public void ImportContent()
    {
        // Definir múltiples extensiones válidas usando ExtensionFilter
        var extensions = new[]
        {
            new ExtensionFilter("Content or JSON Files", "content", "json")
        };

        // Abrir el cuadro de diálogo para seleccionar un archivo
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select a Content File", "", extensions, false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string selectedFilePath = paths[0];

            if (selectedFilePath.EndsWith(".json"))
            {
                ImportJson(selectedFilePath);
            }
            else if (selectedFilePath.EndsWith(".content"))
            {
                ImportContentFile(selectedFilePath);
            }
            else
            {
                Debug.LogError("Formato de archivo no soportado.");
            }
        }
        else
        {
            Debug.Log("Selección de archivo cancelada.");
        }
    }

    private void ImportJson(string filePath)
    {
        try
        {
            // Leer el archivo JSON
            string jsonContent = File.ReadAllText(filePath);

            // Si el JSON representa un array, envuélvelo
            string wrappedJson = $"{{\"questions\":{jsonContent}}}";

            // Deserializar el JSON en un objeto QuestionList
            QuestionList questionList = JsonUtility.FromJson<QuestionList>(wrappedJson);

            if (questionList != null && questionList.questions != null && questionList.questions.Count > 0)
            {
                Debug.Log($"Importadas {questionList.questions.Count} preguntas desde JSON.");

                // Guardar el archivo como .content
                string name = Path.GetFileNameWithoutExtension(filePath);
                StartCoroutine(SaveSystem.SaveContent(questionList, name));
            }
            else
            {
                Debug.LogWarning("El JSON no contiene preguntas o el formato no es válido.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error al importar el archivo JSON: {ex.Message}");
        }
    }

    private void ImportContentFile(string filePath)
    {
        // Ruta de destino
        string contentDirectory = Path.Combine(Application.persistentDataPath, "Content");
        if (!Directory.Exists(contentDirectory))
        {
            Directory.CreateDirectory(contentDirectory);
        }

        string destinationPath = Path.Combine(contentDirectory, Path.GetFileName(filePath));

        // Copiar el archivo seleccionado a la ubicación de destino
        File.Copy(filePath, destinationPath, overwrite: true);

        Debug.Log($"Archivo .content importado y copiado a: {destinationPath}");
        InitScrollView();
        MenuManager.Instance.OpenMessagePopup("Contenido importado con éxito.");
    }

    public void OpenContentFolder()
    {
        string contentDirectory = Path.Combine(Application.persistentDataPath, "Content");

        // Asegurarse de que la carpeta exista
        if (!Directory.Exists(contentDirectory))
        {
            Debug.Log($"La carpeta no existe. Creándola en: {contentDirectory}");
            Directory.CreateDirectory(contentDirectory);
        }

        // Abrir la carpeta en el sistema operativo
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
        {
            FileName = contentDirectory,
            UseShellExecute = true, // Necesario para abrir carpetas en Unity
            Verb = "open" // Comando para abrir la carpeta
        });

        Debug.Log($"Abriendo la carpeta: {contentDirectory}");
    }

}
