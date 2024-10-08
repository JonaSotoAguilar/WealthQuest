using UnityEngine;

public class MultiplayerRoom : MonoBehaviour
{
    [SerializeField] private MultiplayerLocal multiplayerLocal;     // Referencia al script de MultiplayerLocal
    [SerializeField] private TMPro.TextMeshProUGUI[] playerNames;   // Lista de nombres de los jugadores
    [SerializeField] private GameObject[] playerPanels;             // Lista de paneles para los jugadores
    [SerializeField] private GameObject connectedPanelPrefab;       // Prefab del panel "conectado"

    private void Start()
    {
        multiplayerLocal.OnPlayerJoinedEvent += ReplacePanelWithConnected;
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

    public void UpdatePlayerNames()
    {
        for (int i = 0; i < playerNames.Length; i++)
        {
            multiplayerLocal.UpdatePlayerName(i, playerNames[i].text);
        }
    }

    public void StartGame()
    {
        multiplayerLocal.OnPlayerJoinedEvent -= ReplacePanelWithConnected;
        //UpdatePlayerNames();
        multiplayerLocal.UpdateActionMap();
        GameData.Instance.NewGame();
    }
}
