using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class ContentMenu : MonoBehaviour
{
    [SerializeField] private Topics topics;
    [SerializeField] private GameObject topicPrefab;
    [SerializeField] private Transform container;
    [SerializeField] private TMP_InputField searchInput;

    [Header("Buttons")]
    [SerializeField] private GameObject filterButton;
    [SerializeField] private GameObject rechargeButton;
    [SerializeField] private GameObject returnButton;

    private enum FilterMode { All, NotDownloaded, Downloaded }
    private FilterMode currentFilterMode = FilterMode.All;

    #region Initialization

    private void Awake()
    {
        Color color = new Color(1f, 168 / 255f, 65 / 255f, 1f);
        GameObject[] buttons = new GameObject[] { filterButton, rechargeButton, returnButton };
        MenuAnimation.Instance.SubscribeButtonsToEvents(buttons, color);
    }

    private void Start()
    {
        ClearScrollView();
        StartCoroutine(InitializeAndPopulateScrollView());
        searchInput.onValueChanged.AddListener(delegate { FilterBySearch(); });
    }

    private void OnEnable()
    {
        MenuAnimation.Instance.SelectObject(rechargeButton);
    }

    #endregion

    public void InitScrollView()
    {
        ClearScrollView();
        StartCoroutine(InitializeAndPopulateScrollView());
    }

    private IEnumerator InitializeAndPopulateScrollView()
    {
        yield return StartCoroutine(topics.InicializateRemoteTopics((bundleName) =>
        {
            CreateTopicPanel(bundleName);
        }));
    }

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

        downloadButton.GetComponent<Button>().onClick.AddListener(() =>
             StartCoroutine(DownloadBundle(bundleName, newPanel)));
        deleteButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            DeleteLocalTopic(bundleName, newPanel);
        });

        string searchText = searchInput.text.ToLower();
        string panelName = bundleName.ToLower();
        bool isVisible = panelName.Contains(searchText) && IsPanelVisible(isDownloaded);
        newPanel.SetActive(isVisible);
    }

    private IEnumerator DownloadBundle(string bundleName, GameObject topicPanel)
    {
        AudioManager.Instance?.PlaySoundButtonPress();
        topicPanel.transform.Find("Download").GetComponent<Button>().interactable = false;
        yield return StartCoroutine(topics.DownloadAssetBundle(bundleName));

        topicPanel.transform.Find("Download").gameObject.SetActive(false);
        topicPanel.transform.Find("Downloaded").gameObject.SetActive(true);
        topicPanel.transform.Find("Delete").gameObject.SetActive(true);

        RefreshPanels();
        FilterBySearch();
    }

    private void DeleteLocalTopic(string bundleName, GameObject topicPanel)
    {
        AudioManager.Instance?.PlaySoundButtonPress();
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
    private void RefreshPanels()
    {
        foreach (Transform child in container)
        {
            bool isDownloaded = child.Find("Downloaded").gameObject.activeSelf;
            child.gameObject.SetActive(IsPanelVisible(isDownloaded));
        }
    }

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

    private void FilterBySearch()
    {
        string searchText = searchInput.text.ToLower();

        foreach (Transform child in container)
        {
            string panelName = child.Find("Name").GetComponent<TextMeshProUGUI>().text.ToLower();
            child.gameObject.SetActive(panelName.Contains(searchText) && IsPanelVisible(child.Find("Downloaded").gameObject.activeSelf));
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
}
