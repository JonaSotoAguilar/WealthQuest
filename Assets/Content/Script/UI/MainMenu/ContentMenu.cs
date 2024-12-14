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

    [Header("Buttons")]
    [SerializeField] private GameObject filterButton;
    [SerializeField] private GameObject rechargeButton;
    [SerializeField] private GameObject returnButton;
    [SerializeField] private GameObject createContentButton;
    [SerializeField] private GameObject importContentButton;
    [SerializeField] private GameObject fileContentButton;

    [Header("Filter")]
    [SerializeField] private Texture[] filterIcons;
    [SerializeField] private RawImage filterIcon;
    private enum FilterMode { All, Local, Remote, Update }
    private FilterMode currentFilterMode = FilterMode.All;

    #region Initialization

    private void Awake()
    {
        GameObject[] buttons = new GameObject[] { filterButton, rechargeButton, returnButton, createContentButton, importContentButton, fileContentButton };
        MenuAnimation.Instance.SubscribeButtonsToEvents(buttons);
    }

    private void Start()
    {
        searchInput.onValueChanged.AddListener(delegate { FilterBySearch(); });
    }

    private void OnEnable()
    {
        MenuAnimation.Instance.SelectObject(rechargeButton);
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

        string name = content.ExtractName(bundleName);
        int version = content.ExtractVersion(bundleName);
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
            downloadButton.SetActive(false);
            downloadedButton.SetActive(false);
            deleteButton.SetActive(true);
            updateButton.SetActive(true);
            changeButton.SetActive(false);
            exportButton.SetActive(false);
        }
        else
        {
            downloadButton.SetActive(!isLocal);
            downloadedButton.SetActive(isLocal);
            deleteButton.SetActive(isLocal);
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
            AudioManager.Instance?.PlaySoundButtonPress();
            gameObject.SetActive(false);
            createContent.ChangeContent(name, version);
        });
        exportButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlaySoundButtonPress();
            SaveSystem.ExportContentFile(name);
            Popup.Instance.StartCoroutine(Popup.Instance.SuccessExportContent());
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
        AudioManager.Instance?.PlaySoundButtonPress();
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
        AudioManager.Instance?.PlaySoundButtonPress();
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
        AudioManager.Instance?.PlaySoundButtonPress();

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
        // Abrir el cuadro de diálogo para seleccionar un archivo
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select a Content File", "", "content", false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string selectedFilePath = paths[0];

            // Ruta de destino
            string contentDirectory = Path.Combine(Application.persistentDataPath, "Content");
            if (!Directory.Exists(contentDirectory))
            {
                Directory.CreateDirectory(contentDirectory);
            }

            string destinationPath = Path.Combine(contentDirectory, Path.GetFileName(selectedFilePath));

            File.Copy(selectedFilePath, destinationPath, overwrite: true);
            InitScrollView();
            Popup.Instance.StartCoroutine(Popup.Instance.SuccessImportContent());
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
