using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class LocalMultiRoom : MonoBehaviour
{
    [SerializeField] private MultiplayerLocal multiplayerLocal;
    [SerializeField] private Topics topics;
    [SerializeField] private GameObject[] playerPanels;
    [SerializeField] private GameObject connectedPanelPrefab;
    [SerializeField] private TMP_Dropdown bundleDropdown;
    private TextMeshProUGUI[] playerNames;
    private string assetBundleDirectory;
    private string selectedBundle;

    private void Start()
    {
        assetBundleDirectory = Path.Combine(Application.persistentDataPath, "AssetBundles");
        multiplayerLocal.OnPlayerJoinedEvent += ReplacePanelWithConnected;
        PopulateBundleDropdown();
    }

    // Llenar el TMP_Dropdown con los Asset Bundles disponibles
    private void PopulateBundleDropdown()
    {
        // Limpiar las opciones actuales del Dropdown
        bundleDropdown.ClearOptions();

        // Crear una lista para las opciones del Dropdown
        List<string> options = new List<string> { "Default" }; // Agregar "Default" como la primera opción

        // Agregar los bundles locales a las opciones del Dropdown
        options.AddRange(topics.LocalTopicList);

        // Actualizar el Dropdown con las nuevas opciones
        bundleDropdown.AddOptions(options);

        // Seleccionar "Default" por defecto
        selectedBundle = "Default";
        bundleDropdown.value = 0;
    }
    // Método que se ejecuta cuando se selecciona un nuevo bundle en el TMP_Dropdown
    public void OnBundleSelected(int index)
    {
        selectedBundle = bundleDropdown.options[index].text;
        Debug.Log($"Bundle seleccionado: {selectedBundle}");
    }

    // Reemplazar el panel con el prefab de "conectado"
    private void ReplacePanelWithConnected(int index)
    {
        if (playerPanels[index] != null)
        {
            // Guardar la referencia al padre y la posición en la jerarquía
            Transform parent = playerPanels[index].transform.parent;
            int siblingIndex = playerPanels[index].transform.GetSiblingIndex();

            // Destruir el panel anterior
            Destroy(playerPanels[index]);

            // Instanciar el nuevo prefab "conectado" en la misma posición y bajo el mismo padre
            GameObject newPanel = Instantiate(connectedPanelPrefab, parent);

            // Colocar el nuevo panel en la misma posición en la jerarquía
            newPanel.transform.SetSiblingIndex(siblingIndex);

            // Actualizar la referencia en el array de paneles
            playerPanels[index] = newPanel;
        }
    }

    // FIXME: Este método no se está utilizando actualmente
    public void UpdatePlayerNames()
    {
        for (int i = 0; i < playerNames.Length; i++)
        {
            multiplayerLocal.UpdatePlayerName(i, playerNames[i].text);
        }
    }

    public void StartGame()
    {
        if (string.IsNullOrEmpty(selectedBundle))
        {
            Debug.LogWarning("Debe seleccionarse un tema (Asset Bundle) para iniciar el juego.");
            return;
        }

        multiplayerLocal.OnPlayerJoinedEvent -= ReplacePanelWithConnected;
        StartCoroutine(GameData.Instance.NewGame(selectedBundle));
    }

    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
