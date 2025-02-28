using System.Collections;
using System.IO;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContentMenu : MonoBehaviour
{
    [Header("Content")]
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

    public void InitScrollView()
    {
        ClearScrollView();
        StartCoroutine(InitializeContent());
    }

    private IEnumerator InitializeContent()
    {
        yield return ContentDatabase.InitializeContent();

        foreach (string bundleName in ContentDatabase.localContentList)
        {
            CreateContentPanel(bundleName);
        }

        foreach (string bundleName in ContentDatabase.updateContentList)
        {
            CreateContentPanel(bundleName, true, true);
        }

        foreach (string bundleName in ContentDatabase.remoteContentList)
        {
            CreateContentPanel(bundleName, false);
        }
    }

    public void OpenCreateMenu()
    {
        createContent.NewContent();
        MenuManager.Instance.OpenCreateContentMenu();
    }

    #endregion

    #region Content Panel

    private void CreateContentPanel(string bundleName, bool isLocal = true, bool isUpdate = false)
    {
        GameObject newPanel = Instantiate(topicPrefab, container);

        string name = SaveService.ExtractNameContent(bundleName);
        int version = SaveService.ExtractVersionContent(bundleName);
        Content content = ContentDatabase.GetContent(name, version);

        newPanel.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = name;
        newPanel.transform.Find("Version").GetComponent<TextMeshProUGUI>().text = version + ".0";

        GameObject downloadButton = newPanel.transform.Find("Download").gameObject;
        GameObject downloadedButton = newPanel.transform.Find("Downloaded").gameObject;
        GameObject deleteButton = newPanel.transform.Find("Delete").gameObject;
        GameObject updateButton = newPanel.transform.Find("Update").gameObject;

        GameObject changeButton = newPanel.transform.Find("Change").gameObject;
        GameObject exportButton = newPanel.transform.Find("Export").gameObject;

        changeButton.SetActive(false);
        if (isUpdate)
        {
            if (content != null && content.name == SaveService.defaultContentName)
                deleteButton.SetActive(false);
            else
                deleteButton.SetActive(true);
            downloadButton.SetActive(false);
            downloadedButton.SetActive(false);
            updateButton.SetActive(true);
            exportButton.SetActive(false);
        }
        else
        {
            if (content != null && content.name == SaveService.defaultContentName)
                deleteButton.SetActive(false);
            else
                deleteButton.SetActive(isLocal);
            if (content != null && ProfileUser.uid == content.uidAuthor)
                changeButton.SetActive(isLocal);
            downloadButton.SetActive(!isLocal);
            downloadedButton.SetActive(isLocal);
            updateButton.SetActive(false);
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


        if (content != null && ProfileUser.uid == content.uidAuthor)
        {
            changeButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                createContent.UpdateContent(content);
                MenuManager.Instance.OpenCreateContentMenu();
            });
        }

        exportButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            SaveService.ExportContentFile(name);
            MenuManager.Instance.OpenMessagePopup("Contenido exportado con éxito en Descargas.");
        });

        ShowContent(isLocal, isUpdate, newPanel, searchInput.text.ToLower(), name.ToLower());
    }

    private void ShowContent(bool isLocal, bool isUpdate, GameObject newPanel, string searchText, string panelName)
    {
        bool isVisible = panelName.Contains(searchText) && IsPanelVisible(isLocal, isUpdate);
        newPanel.SetActive(isVisible);
    }

    #endregion

    #region Content Management

    private IEnumerator DownloadBundle(string contentName, GameObject contentPanel)
    {
        contentPanel.transform.Find("Download").GetComponent<Button>().interactable = false;
        yield return StartCoroutine(ContentDatabase.DownloadContent(contentName));

        string name = SaveService.ExtractNameContent(contentName);
        int version = SaveService.ExtractVersionContent(contentName);
        Content content = ContentDatabase.GetContent(name, version);

        contentPanel.transform.Find("Download").gameObject.SetActive(false);
        contentPanel.transform.Find("Downloaded").gameObject.SetActive(true);
        contentPanel.transform.Find("Delete").gameObject.SetActive(true);
        contentPanel.transform.Find("Export").gameObject.SetActive(true);

        if (content != null && ProfileUser.uid == content.uidAuthor)
            contentPanel.transform.Find("Change").gameObject.SetActive(true);
        else
            contentPanel.transform.Find("Change").gameObject.SetActive(false);

        ShowContent(true, false, contentPanel, searchInput.text.ToLower(), contentPanel.name.ToLower());
    }

    private IEnumerator UpdateContent(string contentName, GameObject contentPanel)
    {
        contentPanel.transform.Find("Update").GetComponent<Button>().interactable = false;
        yield return StartCoroutine(ContentDatabase.DownloadUpdateContent(contentName));

        string name = SaveService.ExtractNameContent(contentName);
        int version = SaveService.ExtractVersionContent(contentName);
        Content content = ContentDatabase.GetContent(name, version);

        contentPanel.transform.Find("Update").gameObject.SetActive(false);
        contentPanel.transform.Find("Downloaded").gameObject.SetActive(true);
        contentPanel.transform.Find("Delete").gameObject.SetActive(true);
        contentPanel.transform.Find("Export").gameObject.SetActive(true);
        contentPanel.transform.Find("Change").gameObject.SetActive(false);

        if (content != null && ProfileUser.uid == content.uidAuthor)
            contentPanel.transform.Find("Change").gameObject.SetActive(true);
        else
            contentPanel.transform.Find("Change").gameObject.SetActive(false);

        ShowContent(true, false, contentPanel, searchInput.text.ToLower(), contentPanel.name.ToLower());
    }

    private void DeleteLocalTopic(string contentName, GameObject contentPanel)
    {
        bool success = ContentDatabase.DeleteLocalContent(contentName);

        if (success)
        {
            Destroy(contentPanel);
            InitScrollView();
        }
        else
        {
            Debug.LogError("Error al eliminar el contenido local.");
        }
    }

    #endregion

    #region Filter

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

    #endregion  

    #region Import/Export

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
                ContentDatabase.ImportJson(selectedFilePath);
            }
            else if (selectedFilePath.EndsWith(".content"))
            {
                ContentDatabase.ImportContentFile(selectedFilePath);
            }
            else
            {
                Debug.LogError("Formato de archivo no soportado.");
            }
            InitScrollView();
        }
        else
        {
            Debug.Log("Selección de archivo cancelada.");
        }
    }

    public void OpenContentFolder()
    {
        string contentDirectory = Path.Combine(Application.persistentDataPath, "Content");

        // Asegurarse de que la carpeta exista
        SaveService.ExistsDirectoryContent();

        // Abrir la carpeta en el sistema operativo
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
        {
            FileName = contentDirectory,
            UseShellExecute = true,
            Verb = "open"
        });

        Debug.Log($"Abriendo la carpeta: {contentDirectory}");
    }

    #endregion

}
