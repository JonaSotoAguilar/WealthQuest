using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class TopicsMenu : MonoBehaviour
{
    [SerializeField] private TopicsLoader topicsLoader; // Referencia al script de TopicsLoader
    [SerializeField] private GameObject topicPrefab;    // Prefab del panel de tema
    [SerializeField] private Transform container;       // Contenedor de los paneles

    private void Start()
    {
        StartCoroutine(InitializeAndPopulateScrollView());
    }

    // Inicia la carga de los temas y llena el ScrollView
    private IEnumerator InitializeAndPopulateScrollView()
    {
        yield return StartCoroutine(topicsLoader.InicializateTopics());
        PopulateScrollView(topicsLoader.RemoteTopicList);
    }

    // Llena la vista de desplazamiento con los bundles disponibles
    public void PopulateScrollView(List<string> bundleNames)
    {
        ClearScrollView();

        // Crear un panel por cada nombre de bundle
        foreach (string bundleName in bundleNames)
        {
            GameObject newPanel = Instantiate(topicPrefab, container);

            // Actualiza el nombre del tema en el panel
            newPanel.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = bundleName;

            // Verificar si el bundle ya está descargado localmente
            bool isDownloaded = topicsLoader.LocalTopicList.Contains(bundleName);

            // Obtener el botón de descarga y configurar su visibilidad
            GameObject downloadButton = newPanel.transform.Find("Download").gameObject;
            downloadButton.SetActive(!isDownloaded);
            newPanel.transform.Find("Downloaded").gameObject.SetActive(isDownloaded);

            // Si el botón está activo (no descargado), asignar la funcionalidad de descarga
            if (!isDownloaded)
            {
                downloadButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
                    StartCoroutine(DownloadBundle(bundleName, downloadButton)));
            }
        }
    }

    // Método para descargar un bundle y actualizar la UI
    private IEnumerator DownloadBundle(string bundleName, GameObject downloadButton)
    {
        // Iniciar la descarga del Asset Bundle
        yield return StartCoroutine(topicsLoader.DownloadAssetBundle(bundleName));

        // Ocultar el botón de descarga ya que el asset ahora está disponible localmente
        downloadButton.SetActive(false);
    }

    // Limpiar los paneles existentes del ScrollView
    public void ClearScrollView()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}
