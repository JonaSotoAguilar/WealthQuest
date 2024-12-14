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
using UnityEngine.SceneManagement;

public class LobbyLocalMulti : MonoBehaviour
{
    [SerializeField] private GameData gameData;
    [Header("Topics")]
    [SerializeField] private Content content;
    [SerializeField] private TMP_Dropdown bundleDropdown;

    [Header("Players Panel")]
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject playerDisconnectedPrefab;
    [SerializeField] private GameObject parentPlayerPanel;
    [SerializeField] private List<GameObject> playerPanels;
    [SerializeField] private GameObject returnButton;

    [Header("Players Data")]
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private PlayerInput userInput;

    [Header("Characters")]
    [SerializeField] private List<CharacterSelector> characters;

    [Header("Player Bundle")]
    private string assetBundleDirectory;
    private string selectedBundle;

    private void Awake()
    {
        GameObject[] buttons = { startButton.gameObject, returnButton };
        MenuAnimation.Instance.SubscribeButtonsToEvents(buttons);
    }

    private void Start()
    {
        assetBundleDirectory = Path.Combine(Application.persistentDataPath, "AssetBundles");
    }

    public void OnEnable()
    {
        playerInputManager.EnableJoining();
        playerInputManager.onPlayerJoined += OnPlayerJoined;

        PopulateBundleDropdown();
        if (!gameData.DataExists()) DefaultPanel();
        else LoadPanel();
    }

    public void OnDisable()
    {
        playerInputManager.onPlayerJoined -= OnPlayerJoined;
    }

    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void DefaultPanel()
    {
        startButton.interactable = true;
        characters[0].ActiveChanges(true);
        characters[0].UserPlayer();
    }

    public void LoadPanel()
    {
        // Bloquear seleccion de tema
        selectedBundle = gameData.topicName;
        bundleDropdown.value = content.LocalTopicList.IndexOf(gameData.topicName);
        bundleDropdown.interactable = false;
        startButton.interactable = false;
        // Limpiar jugadores exededentes
        int maxPlayers = gameData.playersData.Count;
        for (int i = playerPanels.Count - 1; i >= maxPlayers; i--)
        {
            Destroy(playerPanels[i]);
            playerPanels.RemoveAt(i);
        }
        // Data jugador principal
        characters[0].UpdateName(gameData.playersData[0].Nickname);
        characters[0].LoadCharacter(gameData.playersData[0].CharacterID);
        characters[0].ActiveChanges(false);
    }

    private void PopulateBundleDropdown()
    {
        bundleDropdown.ClearOptions();
        List<string> options = new List<string> { "Default" };
        options.AddRange(content.LocalTopicList);
        bundleDropdown.AddOptions(options);
        selectedBundle = "Default";
        bundleDropdown.value = 0;
        bundleDropdown.interactable = true;
    }

    public void OnBundleSelected(int index)
    {
        selectedBundle = bundleDropdown.options[index].text;
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        int index = playerInput.playerIndex;
        if ((gameData.playersData.Count - 1) == index)
        {
            startButton.interactable = true;
            playerInputManager.DisableJoining();
        }
        if (index == 0) return;

        playerInput.actions.FindActionMap("Player").Disable();
        playerInput.SwitchCurrentActionMap("UI");
        GameObject newPanel = CreatePlayerPanel(playerInput, index);
        CreateCharacter(index, newPanel);
    }
    private GameObject CreatePlayerPanel(PlayerInput playerInput, int index)
    {
        Transform parent = playerPanels[index].transform.parent;
        int siblingIndex = playerPanels[index].transform.GetSiblingIndex();
        Destroy(playerPanels[index]);
        GameObject newPanel = playerInput.gameObject;
        newPanel.name = "PlayerConnected_" + (index + 1);
        newPanel.transform.SetParent(parent);
        newPanel.transform.SetSiblingIndex(siblingIndex);
        playerPanels[index] = newPanel;
        return newPanel;
    }

    private void CreateCharacter(int index, GameObject newPanel)
    {
        CharacterSelector character = newPanel.GetComponent<CharacterSelector>();
        character.UpdateIndex(index);
        characters.Add(character);

        // Revisa si el jugador ya tiene data cargada
        if (gameData.playersData.Count > index)
        {
            character.UpdateName(gameData.playersData[index].Nickname);
            character.LoadCharacter(gameData.playersData[index].CharacterID);
            character.ActiveChanges(false);
        }
    }

    public void DisconnectPlayers()
    {
        for (int i = 1; i < playerPanels.Count; i++)
        {
            var playerPanel = playerPanels[i];
            if (playerPanel.TryGetComponent(out CharacterSelector character)) characters.Remove(character);
            Destroy(playerPanel);
        }
        playerPanels.RemoveRange(1, playerPanels.Count - 1);
        for (int i = 1; i < 4; i++)
        {
            var disconnectedPanel = Instantiate(playerDisconnectedPrefab, parentPlayerPanel.transform);
            disconnectedPanel.name = "PlayerDisconnected_" + i;
            disconnectedPanel.transform.Find("PlayerIndex").GetComponent<TextMeshProUGUI>().text = "Jugador " + (i + 1);
            playerPanels.Add(disconnectedPanel);
        }
    }

    // FIXME: Sin utilizar
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
        StartCoroutine(InitGame());
    }

    private IEnumerator InitGame()
    {
        yield return SavePlayerInputs();
        if (!gameData.DataExists()) yield return gameData.LoadCardsAndQuestions(selectedBundle);
        SceneManager.LoadScene("LocalMulti");
    }

    public IEnumerator SavePlayerInputs()
    {
        PlayerStorage.ClearData();

        // Jugador principal
        var playerInput = userInput;
        var device = playerInput.devices.FirstOrDefault();
        var controlScheme = playerInput.currentControlScheme;
        PlayerStorage.SavePlayerStorage(0, device, controlScheme, characters[0].PlayerName, characters[0].Model);

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
                    PlayerStorage.SavePlayerStorage(characters[i].Index, device, controlScheme, characters[i].PlayerName, characters[i].Model);
                }
                else
                {
                    Debug.LogWarning($"El jugador {i} no tiene un dispositivo asignado.");
                }
            }
        }
        yield return null;
    }

}
