using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LobbyLocal : MonoBehaviour
{
    private const string SCENE_GAME = "LocalBoard";

    [Header("Game Data")]
    [SerializeField] private GameData gameData;

    [Space, Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI modeText;
    [SerializeField] private TextMeshProUGUI readyText;
    [SerializeField] private TMP_Dropdown yearDropdown;
    [SerializeField] private TMP_Dropdown contentDropdown;

    [Space, Header("Game Actions")]
    [SerializeField] private Button startButton;

    [Space, Header("Banners")]
    [SerializeField] private GameObject bannerPrefab;
    [SerializeField] private GameObject bannerDisconnectedPrefab;
    [SerializeField] private GameObject parentBannerPanel;
    [SerializeField] private List<GameObject> bannersPanel;
    [SerializeField] private List<BannerLocal> characters;

    [Space, Header("Inputs")]
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private PlayerInput userInput;

    // Variable control
    private int mode = 0;
    private bool newGame = true;

    #region Initializers

    public void OnEnable()
    {
        PopulateContentDropdown();
        if (gameData.DataExists()) LoadGameData();
        else NewGameData();
    }

    public void OnDisable()
    {
        DisconnectPlayers();
        if (mode == 2) playerInputManager.onPlayerJoined -= OnPlayerJoined;
    }

    public void SetMode(int mode)
    {
        this.mode = mode;
        switch (mode)
        {
            case 0:
                SingleMode();
                break;
            case 1:
                PassMode();
                break;
            case 2:
                MultiMode();
                break;
        }
    }

    private void SingleMode()
    {
        readyText.text = "¡Empieza la partida cuando estés listo!";
        modeText.text = "Un Jugador";
        startButton.interactable = true;
        for (int i = 1; i < bannersPanel.Count; i++)
        {
            bannersPanel[i].SetActive(false);
        }
    }

    private void PassMode()
    {
        readyText.text = "¡Se requieren 2 jugadores para empezar!";
        modeText.text = "Pasar y Jugar";
        startButton.interactable = false;
        for (int i = 1; i < bannersPanel.Count; i++)
        {
            bannersPanel[i].SetActive(true);
            ActiveButtonAdd(i);
        }
    }

    private void MultiMode()
    {
        readyText.text = "¡Se requieren 2 jugadores para empezar, conecta un mando!";
        modeText.text = "Multi Mando";
        startButton.interactable = false;
        playerInputManager.EnableJoining();
        playerInputManager.onPlayerJoined += OnPlayerJoined;
        for (int i = 1; i < bannersPanel.Count; i++)
        {
            bannersPanel[i].SetActive(true);
            var button = bannersPanel[i].transform.Find("AddPlayer").GetComponent<Button>();
            button.gameObject.SetActive(false);
        }
    }

    private void ActiveButtonAdd(int i)
    {
        var button = bannersPanel[i].transform.Find("AddPlayer").GetComponent<Button>();
        button.gameObject.SetActive(true);
        button.onClick.AddListener(AddBanner);
    }

    #endregion

    #region Game Data

    private void NewGameData()
    {
        newGame = true;
        yearDropdown.value = 0;
        yearDropdown.interactable = true;
        characters[0].ActiveChanges(true);
        characters[0].UserPlayer();
    }

    private void LoadGameData()
    {
        newGame = false;
        // Bloquear seleccion de tema
        contentDropdown.value = ContentDatabase.localContentList.IndexOf(gameData.content);
        contentDropdown.interactable = false;
        // Bloquear seleccion de años
        yearDropdown.value = (gameData.yearsToPlay - 10) / 5;
        yearDropdown.interactable = false;
        // Cargar jugadores 
        if (mode != 0) CreatePlayers();
        else LoadMainPlayer();
    }

    private void CreatePlayers()
    {
        LoadMainPlayer();
        int maxPlayers = gameData.playersData.Count;
        for (int i = bannersPanel.Count - 1; i >= maxPlayers; i--)
        {
            Destroy(bannersPanel[i]);
            bannersPanel.RemoveAt(i);
        }
        if (mode == 1)
        {
            startButton.interactable = true;
            readyText.text = "¡Empieza la partida cuando estén todos listos!";
            for (int i = 1; i < maxPlayers; i++)
                AddBanner();
        }
        else
        {
            readyText.text = "¡Esperando a que los jugadores conecten sus mandos!";
        }
    }

    private void LoadMainPlayer()
    {
        characters[0].UpdateName(gameData.playersData[0].Nickname);
        characters[0].LoadCharacter(gameData.playersData[0].CharacterID);
        characters[0].ActiveChanges(false);
    }

    #endregion

    #region Content

    private void PopulateContentDropdown()
    {
        contentDropdown.ClearOptions();
        List<string> options = new List<string>();

        foreach (var content in ContentDatabase.localContentList)
        {
            string baseName = SaveService.ExtractNameContent(content);
            options.Add(baseName);
        }

        // Agregar las opciones al Dropdown
        contentDropdown.AddOptions(options);
        contentDropdown.value = 0;
        contentDropdown.interactable = options.Count > 0;
    }

    #endregion

    #region Multiplayer 

    private void AddBanner()
    {
        if (characters.Count >= 4) return;
        Instantiate(bannerPrefab, parentBannerPanel.transform);
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        int index = playerInput.playerIndex;
        if (index == 0 || index >= bannersPanel.Count) return;
        playerInput.actions.FindActionMap("Player").Disable();
        playerInput.SwitchCurrentActionMap("UI");
        CreateBanner(playerInput, index);
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
        if (mode == 1)
        {
            button.onClick.AddListener(() => DeletePlayer(newPanel));
            button.gameObject.SetActive(true);
        }
        else
        {
            button.gameObject.SetActive(false);
        }

        CreateCharacter(index, newPanel);
    }

    private void CreateCharacter(int index, GameObject newPanel)
    {
        BannerLocal character = newPanel.GetComponent<BannerLocal>();
        characters.Add(character);

        // Revisa si el jugador ya tiene data cargada
        if (gameData.playersData.Count > index)
        {
            character.UpdateName(gameData.playersData[index].Nickname);
            character.LoadCharacter(gameData.playersData[index].CharacterID);
            character.ActiveChanges(false);

            var button = newPanel.transform.Find("DeletePlayer").GetComponent<Button>();
            button.gameObject.SetActive(false);
        }

        EnableStartGame();
    }

    public void DeletePlayer(GameObject panel)
    {
        BannerLocal characterSelector = panel.GetComponent<BannerLocal>();

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

        if (characters.Count < 2)
        {
            readyText.text = "¡Se requieren 2 jugadores para empezar!";
            startButton.interactable = false;
        }
    }

    public void DisconnectPlayers()
    {
        for (int i = 1; i < bannersPanel.Count; i++)
        {
            var playerPanel = bannersPanel[i];
            if (playerPanel.TryGetComponent(out BannerLocal character)) characters.Remove(character);
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

    private void EnableStartGame()
    {
        if (!newGame && mode == 2 && gameData.playersData.Count == characters.Count)
        {
            startButton.interactable = true;
            readyText.text = "¡Empieza la partida cuando estén todos listos!";
            if (playerInputManager != null) playerInputManager.DisableJoining();
        }
        else if (newGame && characters.Count == 2)
        {
            startButton.interactable = true;
            readyText.text = "¡Empieza la partida cuando estén todos listos!";
        }
    }

    public void StartGame()
    {
        if (mode == 2) SavePlayerInputs();
        if (newGame)
        {
            gameData.LoadContent(contentDropdown.options[contentDropdown.value].text);
            CreateNewGameData();
        }
        SceneTransition.Instance.LoadScene(SCENE_GAME);
    }

    private void CreateNewGameData()
    {
        gameData.mode = mode;
        int years = 10 + (yearDropdown.value * 5);
        gameData.yearsToPlay = 2;
        SavePlayer();
    }

    private void SavePlayer()
    {
        // Jugador principal
        var character = characters[0];
        gameData.SavePlayer(ProfileUser.uid, character.PlayerName, character.Model);

        // Jugadores secundarios
        for (int i = 1; i < characters.Count; i++)
        {
            character = characters[i];
            gameData.SavePlayer($"{i}", character.PlayerName, character.Model);
        }
    }

    public void SavePlayerInputs()
    {
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
