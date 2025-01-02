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

public class LobbyLocal : MonoBehaviour
{
    private const string SCENE_GAME = "LocalBoard";
    [SerializeField] private GameData gameData;

    [Header("Mode")]
    [SerializeField] private TMP_Dropdown yearDropdown;
    [SerializeField] private TextMeshProUGUI modeText;
    private int mode = 0;

    [Header("Content")]
    [SerializeField] private Content content;
    [SerializeField] private TMP_Dropdown contentDropdown;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject returnButton;

    [Header("Players Panel")]
    [SerializeField] private GameObject bannerDisconnectedPrefab;
    [SerializeField] private GameObject parentBannerPanel;

    [Header("Players Input")]
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private PlayerInput userInput;

    [Header("Characters")]
    [SerializeField] private GameObject bannerPrefab;
    [SerializeField] private List<GameObject> bannersPanel;
    [SerializeField] private List<CharacterSelector> characters;

    #region Initializers

    private void Awake()
    {
        //GameObject[] buttons = { startButton.gameObject, returnButton };
        //MenuAnimation.Instance.SubscribeButtonsToEvents(buttons);
    }

    public void SetMode(int mode)
    {
        this.mode = mode;
        switch (mode)
        {
            case 0:
                modeText.text = "Un Jugador";
                break;
            case 1:
                modeText.text = "Pasar y Jugar";
                break;
            case 2:
                modeText.text = "Multi Mando";
                break;
        }
    }

    public void OnEnable()
    {
        yearDropdown.value = 0;
        PopulateContentDropdown();
        if (!gameData.DataExists()) NewGameData();
        else LoadGameData();

        if (mode == 0) SingleMode();
        else if (mode == 1) PassMode();
        else if (mode == 2) MultiMode();
    }

    private void SingleMode()
    {
        for (int i = 1; i < bannersPanel.Count; i++)
        {
            bannersPanel[i].SetActive(false);
        }
    }

    private void PassMode()
    {
        for (int i = 1; i < bannersPanel.Count; i++)
        {
            bannersPanel[i].SetActive(true);
            ActiveButtonAdd(i);
        }
    }

    private void ActiveButtonAdd(int i)
    {
        var button = bannersPanel[i].transform.Find("AddPlayer").GetComponent<Button>();
        button.gameObject.SetActive(true);
        button.onClick.AddListener(AddBanner);
    }

    private void MultiMode()
    {
        playerInputManager.EnableJoining();
        playerInputManager.onPlayerJoined += OnPlayerJoined;
        for (int i = 1; i < bannersPanel.Count; i++)
        {
            bannersPanel[i].SetActive(true);
            var button = bannersPanel[i].transform.Find("AddPlayer").GetComponent<Button>();
            button.gameObject.SetActive(false);
        }
    }

    public void OnDisable()
    {
        if (mode == 2)
        {
            DisconnectPlayers();
            playerInputManager.onPlayerJoined -= OnPlayerJoined;
        }
    }

    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void NewGameData()
    {
        startButton.interactable = true;
        characters[0].ActiveChanges(true);
        characters[0].UserPlayer();
    }

    public void LoadGameData()
    {
        // Bloquear seleccion de tema
        contentDropdown.value = content.LocalTopicList.IndexOf(gameData.content);
        contentDropdown.interactable = false;
        MainPlayer();
        if (mode != 0) CleanPlayers();
    }

    private void MainPlayer()
    {
        characters[0].UpdateName(gameData.playersData[0].Nickname);
        characters[0].LoadCharacter(gameData.playersData[0].CharacterID);
        characters[0].ActiveChanges(false);
    }

    private void CleanPlayers()
    {
        startButton.interactable = false;
        int maxPlayers = gameData.playersData.Count;
        for (int i = bannersPanel.Count - 1; i >= maxPlayers; i--)
        {
            Destroy(bannersPanel[i]);
            bannersPanel.RemoveAt(i);
        }
    }

    #endregion

    #region Content

    private void PopulateContentDropdown()
    {
        contentDropdown.ClearOptions();
        List<string> options = new List<string>();

        foreach (var topic in content.LocalTopicList)
        {
            string baseName = SaveSystem.ExtractName(topic);
            options.Add(baseName);
        }

        // Agregar las opciones al Dropdown
        contentDropdown.AddOptions(options);
        contentDropdown.value = 0;
        contentDropdown.interactable = options.Count > 0;
    }

    #endregion

    #region Multiplayer

    private void CreateCharacter(int index, GameObject newPanel)
    {
        CharacterSelector character = newPanel.GetComponent<CharacterSelector>();
        characters.Add(character);

        // Revisa si el jugador ya tiene data cargada
        if (gameData.playersData.Count > index)
        {
            character.UpdateName(gameData.playersData[index].Nickname);
            character.LoadCharacter(gameData.playersData[index].CharacterID);
            character.ActiveChanges(false);
        }
    }

    public void DeletePlayer(GameObject panel)
    {
        Debug.Log("Delete Player");
        CharacterSelector characterSelector = panel.GetComponent<CharacterSelector>();

        bannersPanel.Remove(panel);
        characters.Remove(characterSelector);
        Destroy(panel);

        bannersPanel.Add(Instantiate(bannerDisconnectedPrefab, parentBannerPanel.transform));
        if (mode == 1) ActiveButtonAdd(bannersPanel.Count - 1);

        //Ordenar banner
        for (int i = characters.Count; i < bannersPanel.Count; i++)
        {
            var disconnectedPanel = bannersPanel[i];
            disconnectedPanel.name = "PlayerDisconnected_" + i;
            disconnectedPanel.transform.Find("PlayerIndex").GetComponent<TextMeshProUGUI>().text = "Jugador " + (i + 1);
        }

    }

    #endregion

    #region Multiplayer without Inputs

    private void AddBanner()
    {
        if (characters.Count == 4) return;
        int index = characters.Count;
        Instantiate(bannerPrefab, parentBannerPanel.transform);
    }

    #endregion

    #region Multiplayer with Inputs

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        int index = playerInput.playerIndex;
        if (index == 0) return;
        playerInput.actions.FindActionMap("Player").Disable();
        playerInput.SwitchCurrentActionMap("UI");
        CreateBanner(playerInput, index);
    }

    private void EnableStartGame()
    {
        if (!gameData.DataExists()) return;
        if (gameData.playersData.Count == characters.Count)
        {
            startButton.interactable = true;
            if (playerInputManager != null) playerInputManager.DisableJoining();
        }
    }

    private void CreateBanner(PlayerInput playerInput, int index)
    {
        Transform parent = parentBannerPanel.transform;
        int siblingIndex = bannersPanel[index].transform.GetSiblingIndex();
        Destroy(bannersPanel[index]);
        GameObject newPanel = playerInput.gameObject;
        newPanel.name = "PlayerConnected_" + (index + 1);
        newPanel.transform.SetParent(parent);
        newPanel.transform.SetSiblingIndex(siblingIndex);
        bannersPanel[index] = newPanel;
        var button = newPanel.transform.Find("DeletePlayer").GetComponent<Button>();
        button.onClick.AddListener(() => DeletePlayer(newPanel));
        CreateCharacter(index, newPanel);
    }

    public void DisconnectPlayers()
    {
        for (int i = 1; i < bannersPanel.Count; i++)
        {
            var playerPanel = bannersPanel[i];
            if (playerPanel.TryGetComponent(out CharacterSelector character)) characters.Remove(character);
            Destroy(playerPanel);
        }
        bannersPanel.RemoveRange(1, bannersPanel.Count - 1);
        for (int i = 1; i < 4; i++)
        {
            var disconnectedPanel = Instantiate(bannerDisconnectedPrefab, parentBannerPanel.transform);
            disconnectedPanel.name = "PlayerDisconnected_" + i;
            disconnectedPanel.transform.Find("PlayerIndex").GetComponent<TextMeshProUGUI>().text = "Jugador " + (i + 1);
            bannersPanel.Add(disconnectedPanel);
        }
    }

    #endregion

    #region Start Game

    public void StartGame()
    {
        StartCoroutine(InitializeGame());
    }

    private IEnumerator InitializeGame()
    {
        if (mode == 2) SavePlayerInputs();
        if (!gameData.DataExists())
        {
            yield return gameData.LoadContent(contentDropdown.options[contentDropdown.value].text);
            CreateNewGameData();
        }
        SceneManager.LoadScene(SCENE_GAME);
    }

    private void CreateNewGameData()
    {
        gameData.mode = mode;
        int years = 10 + (yearDropdown.value * 5);
        gameData.yearsToPlay = years;
        SavePlayer();
    }

    private void SavePlayer()
    {
        // Jugador principal
        var character = characters[0];
        gameData.SavePlayer(ProfileUser.UID, character.PlayerName, character.Model);

        // Jugadores secundarios
        for (int i = 1; i < characters.Count; i++)
        {
            character = characters[i];
            gameData.SavePlayer($"{i}", character.PlayerName, character.Model);
        }
    }

    public void SavePlayerInputs()
    {
        Debug.Log("Save Player Inputs");
        InputStorage.ClearData();

        // Jugador principal
        var playerInput = userInput;
        var device = playerInput.devices.FirstOrDefault();
        InputStorage.SaveInputStorage(device);

        // Jugadores secundarios
        for (int i = 1; i < characters.Count; i++)
        {
            playerInput = characters[i].GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                device = playerInput.devices.FirstOrDefault();

                if (device != null)
                {
                    InputStorage.SaveInputStorage(device);
                }
                else
                {
                    Debug.LogWarning($"El jugador {i} no tiene un dispositivo asignado.");
                }
            }
        }
    }

    #endregion

}
