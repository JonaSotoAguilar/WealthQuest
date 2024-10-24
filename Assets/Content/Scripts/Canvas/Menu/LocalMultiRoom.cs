using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Linq;
using System.Collections;

public class LocalMultiRoom : MonoBehaviour
{
    [SerializeField] private GameObject eventSystem;
    [Header("Topics")]
    [SerializeField] private Topics topics;
    [SerializeField] private TMP_Dropdown bundleDropdown;

    [Header("Players Panel")]
    [SerializeField] private GameObject playerDisconnectedPrefab;
    [SerializeField] private GameObject parentPlayerPanel;
    [SerializeField] private List<GameObject> playerPanels;

    [Header("Players Data")]
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private PlayerStorage playerStorage;
    [SerializeField] private PlayerInput userInput;

    [Header("Characters")]
    [SerializeField] private List<CharacterSelector> characters;

    [Header("Player Bundle")]
    private string assetBundleDirectory;
    private string selectedBundle;

    private void Start()
    {
        assetBundleDirectory = Path.Combine(Application.persistentDataPath, "AssetBundles");
        PopulateBundleDropdown();
    }

    private void PopulateBundleDropdown()
    {
        bundleDropdown.ClearOptions();
        List<string> options = new List<string> { "Default" };
        options.AddRange(topics.LocalTopicList);
        bundleDropdown.AddOptions(options);
        selectedBundle = "Default";
        bundleDropdown.value = 0;
    }

    public void OnBundleSelected(int index)
    {
        selectedBundle = bundleDropdown.options[index].text;
    }

    public void OnEnable()
    {
        playerInputManager.EnableJoining();
        playerInputManager.onPlayerJoined += OnPlayerJoined;
    }

    public void OnDisable()
    {
        playerInputManager.onPlayerJoined -= OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        int index = playerInput.playerIndex;

        if (index == 0) return;

        playerInput.actions.FindActionMap("Player").Disable();
        playerInput.SwitchCurrentActionMap("UI");

        // Se crea un nuevo panel para el jugador conectado
        Transform parent = playerPanels[index].transform.parent;
        int siblingIndex = playerPanels[index].transform.GetSiblingIndex();
        Destroy(playerPanels[index]);
        GameObject newPanel = playerInput.gameObject;
        newPanel.name = "PlayerConnected_" + (index + 1);
        newPanel.transform.SetParent(parent);
        newPanel.transform.SetSiblingIndex(siblingIndex);
        playerPanels[index] = newPanel;

        // Obtiene characterSelector y lo añade a la lista de personajes
        CharacterSelector character = newPanel.GetComponent<CharacterSelector>();
        character.UpdateIndex(index);
        characters.Add(character);
    }

    public void DisconnectPlayers()
    {
        var panelsToRemove = playerPanels.Skip(1).ToList();
        for (int i = 1; i < playerPanels.Count; i++)
        {
            var playerPanel = playerPanels[i];
            if (playerPanel.TryGetComponent(out CharacterSelector character)) characters.Remove(character);
            Destroy(playerPanel);
        }
        playerPanels.RemoveRange(1, playerPanels.Count - 1);
        for (int i = 1; i < panelsToRemove.Count + 1; i++)
        {
            var disconnectedPanel = Instantiate(playerDisconnectedPrefab, parentPlayerPanel.transform);
            disconnectedPanel.name = "PlayerDisconnected_" + i;
            playerPanels.Add(disconnectedPanel);
        }
    }

    public void DeletePlayer(CharacterSelector character)
    {
        GameObject player = character.gameObject;

        playerPanels.Remove(player);
        characters.Remove(character);

        Destroy(player);
        playerPanels.Add(Instantiate(playerDisconnectedPrefab, parentPlayerPanel.transform));
        playerPanels[playerPanels.Count - 1].name = "PlayerConnected_" + (playerPanels.Count);
        var newCharacter = Instantiate(character, parentPlayerPanel.transform);
        characters.Add(newCharacter);
        for (int i = 0; i < characters.Count; i++) characters[i].UpdateIndex(i);
    }

    public void StartGame()
    {
        if (string.IsNullOrEmpty(selectedBundle))
        {
            Debug.LogWarning("Debe seleccionarse un tema (Asset Bundle) para iniciar el juego.");
            return;
        }
        SavePlayerInputs();

        StartCoroutine(GameData.Instance.NewGame(selectedBundle));
    }

    public void SavePlayerInputs()
    {
        playerStorage.ClearData();

        // Usuario principal
        var playerInput = userInput;
        var device = playerInput.devices.FirstOrDefault();
        var controlScheme = playerInput.currentControlScheme;
        playerStorage.SavePlayerStorage(0, device, controlScheme, characters[0].PlayerName, characters[0].Model);

        // Jugadores secundarios
        for (int i = 1; i < characters.Count; i++)
        {
            playerInput = characters[i].GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                device = playerInput.devices.FirstOrDefault();
                controlScheme = playerInput.currentControlScheme;

                if (device != null)
                {
                    playerStorage.SavePlayerStorage(characters[i].Index, device, controlScheme, characters[i].PlayerName, characters[i].Model);
                }
                else
                {
                    Debug.LogWarning($"El jugador {i} no tiene un dispositivo asignado.");
                }
            }
        }
    }


    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void ActiveEventSystem(bool active)
    {
        eventSystem.SetActive(active);
    }
}
