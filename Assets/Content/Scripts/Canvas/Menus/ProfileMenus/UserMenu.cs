using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UserMenu : MonoBehaviour
{

    [Header("Profile")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button changeName;

    [Header("Statistics")]
    [SerializeField] private TextMeshProUGUI xpUser;
    [SerializeField] private TextMeshProUGUI scoreAverageUser;
    [SerializeField] private TextMeshProUGUI bestScoreUser;
    [SerializeField] private TextMeshProUGUI playedGames;

    [Header("Character")]
    [SerializeField] private CharactersDatabase characterDB;
    [SerializeField] private RawImage imageCharacter;
    private int characterSelected;

    private void OnEnable()
    {
        LoadSettings();
    }

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
            nameInput.text = ProfileUser.NameUser;
        }
        else
        {
            ProfileUser.SaveNameUser(nameInput.text);
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
        ProfileUser.SaveCharacterUser(selectedOption);
    }

    // Paneles de la interfaz
    public void LoadSettings()
    {
        ProfileUser.LoadSettings();

        nameInput.text = ProfileUser.NameUser;
        xpUser.text = ProfileUser.XPUser.ToString();
        bestScoreUser.text = ProfileUser.BestScoreUser.ToString();
        playedGames.text = ProfileUser.PlayedGames.ToString();

        characterSelected = ProfileUser.IndexCharacter;
        UpdateCharacter(characterSelected);
    }

}