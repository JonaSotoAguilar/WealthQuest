using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;


public class ProfileMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject profilePanel;
    [SerializeField] private GameObject historyPanel;
    [SerializeField] private GameObject bGamesPanel;
    [SerializeField] private GameObject settingsPanel;
    private GameObject activePanel;

    [Header("User")]
    [SerializeField] private ProfileUser profile;
    [SerializeField] private CharactersDatabase characterDB;
    [SerializeField] private IconsDatabase iconDB;

    [Header("Profile")]
    [SerializeField] private TextMeshProUGUI nameUser;
    [SerializeField] private TextMeshProUGUI scoreUser;
    [SerializeField] private TextMeshProUGUI bestScoreUser;
    [SerializeField] private TextMeshProUGUI playedGames;
    [SerializeField] private RawImage iconUser;

    [Header("History")]

    [Header("bGames")]

    [Header("Setting")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button changeName;

    [SerializeField] private RawImage imageIcon;
    [SerializeField] private int iconSelected;

    [SerializeField] private RawImage imageCharacter;
    [SerializeField] private int characterSelected;

    // Interfaz Settings
    public void ActivateInputField()
    {
        changeName.interactable = false;
        nameInput.interactable = true;
        nameInput.ActivateInputField();
        nameInput.Select();
    }

    public void SaveName()
    {
        StartCoroutine(ChangeName());
    }

    public IEnumerator ChangeName()
    {
        yield return null;
        if (nameInput.text == "")
        {
            nameInput.text = profile.NameUser;
        }
        else
        {
            profile.NameUser = nameInput.text;
            profile.SaveNameUser(nameInput.text);
        }
        nameInput.interactable = false;
        changeName.interactable = true;
        changeName.Select();
    }

    public void NextCharacter()
    {
        characterSelected = (characterSelected + 1) % characterDB.Length;
        UpdateCharacter(characterSelected);
    }

    public void PreviousCharacter()
    {
        characterSelected = (characterSelected - 1 + characterDB.Length) % characterDB.Length;
        UpdateCharacter(characterSelected);
    }

    private void UpdateCharacter(int selectedOption)
    {
        Character character = characterDB.GetCharacter(selectedOption);
        imageCharacter.texture = character.characterIcon;
        profile.SaveCharacterUser(selectedOption);
    }

    public void NextIcon()
    {
        iconSelected = (characterSelected + 1) % iconDB.Length;
        UpdateIcon(characterSelected);
    }

    private void UpdateIcon(int selectedOption)
    {
        iconUser.texture = iconDB.GetIcon(selectedOption);
        imageIcon.texture = iconDB.GetIcon(selectedOption);
        profile.SaveIconUser(selectedOption);
    }

    // Paneles de la interfaz
    public void LoadSettings()
    {
        nameUser.text = profile.NameUser;
        nameInput.text = profile.NameUser;

        scoreUser.text = profile.ScoreUser.ToString();
        bestScoreUser.text = profile.BestScoreUser.ToString();
        playedGames.text = profile.PlayedGames.ToString();

        characterSelected = profile.IndexCharacter;
        UpdateCharacter(characterSelected);
        iconSelected = profile.IndexIcon;
        UpdateIcon(iconSelected);
    }

    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
        if (activePanel == null)
        {
            LoadSettings();
            activePanel = profilePanel;
            activePanel.SetActive(visible);
        }
    }

    public void ShowProfile(bool visible)
    {
        if (activePanel != profilePanel)
        {
            LoadSettings();
            activePanel.SetActive(false);
            profilePanel.SetActive(visible);
            activePanel = profilePanel;
        }
    }

    public void ShowHistory(bool visible)
    {
        if (activePanel != historyPanel)
        {
            activePanel.SetActive(false);
            historyPanel.SetActive(visible);
            activePanel = historyPanel;
        }
    }

    public void ShowBGames(bool visible)
    {
        if (activePanel != bGamesPanel)
        {
            activePanel.SetActive(false);
            bGamesPanel.SetActive(visible);
            activePanel = bGamesPanel;
        }
    }

    public void ShowSettings(bool visible)
    {
        if (activePanel != settingsPanel)
        {
            activePanel.SetActive(false);
            settingsPanel.SetActive(visible);
            activePanel = settingsPanel;
        }
    }
}
