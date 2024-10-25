using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;


public class CharacterSelector : MonoBehaviour
{
    [SerializeField] private int index;
    [SerializeField] private ProfileUser profile;
    [SerializeField] private Button DeletePlayer;

    [Header("Character")]
    [SerializeField] private CharactersDatabase characterDB;
    [SerializeField] private RawImage imageCharacter;
    [SerializeField] private Button nextCharacter;
    [SerializeField] private Button previousCharacter;
    private int characterSelected;

    [Header("Name")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button changeName;
    [SerializeField] private string playerName;

    public int Index { get => index; set => index = value; }
    public string PlayerName { get => playerName; set => playerName = value; }
    public Character Model { get => characterDB.GetCharacter(characterSelected); }

    void Start()
    {
        if (index == 0) UserPlayer();
        UpdateCharacter(characterSelected);
    }

    private void UserPlayer()
    {
        playerName = profile.NameUser;
        characterSelected = profile.IndexCharacter;
        nameInput.text = playerName;
    }

    public void UpdateIndex(int i)
    {
        index = i;
        if (playerName.Contains("Jugador")) playerName = "Jugador " + (i + 1);
        nameInput.text = playerName;
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

    public void UpdateCharacter(int selectedOption)
    {
        Character character = characterDB.GetCharacter(selectedOption);
        imageCharacter.texture = character.characterIcon;
    }

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

    private IEnumerator ChangeName()
    {
        yield return null;
        if (nameInput.text == "")
        {
            nameInput.text = playerName;
        }
        else
        {
            playerName = nameInput.text;
        }
        nameInput.interactable = false;
        changeName.interactable = true;
        changeName.Select();
    }

    public void UpdateName(string name)
    {
        playerName = name;
        nameInput.text = playerName;
    }

    public void DesactiveChanges()
    {
        nameInput.interactable = false;
        changeName.interactable = false;
        nextCharacter.interactable = false;
        previousCharacter.interactable = false;
        if (DeletePlayer != null) Destroy(DeletePlayer.gameObject);
    }
}
