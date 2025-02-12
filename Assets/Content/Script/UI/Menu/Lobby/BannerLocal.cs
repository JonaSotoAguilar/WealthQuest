using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class BannerLocal : MonoBehaviour
{
    [Header("Character")]
    [SerializeField] private CharactersDatabase characterDB;
    [SerializeField] private Button nextCharacter;
    [SerializeField] private Button previousCharacter;
    [SerializeField] private Image characterSprite;
    private int characterSelected = 0;

    [Header("Name")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button changeName;
    [SerializeField] private string playerName;

    [Header("Status")]
    [SerializeField] private GameObject connectedText;

    public string PlayerName { get => playerName; set => playerName = value; }
    public int Model { get => characterSelected; }

    public void Awake()
    {
        playerName = "Jugador";
        nameInput.text = playerName;
    }

    public void UserPlayer()
    {
        playerName = ProfileUser.username;
        characterSelected = 0;
        characterSprite.sprite = characterDB.GetCharacter(characterSelected).characterIcon;
        nameInput.text = playerName;
        changeName.gameObject.SetActive(false);
    }

    public void ActiveChanges(bool active)
    {
        nameInput.interactable = active;
        changeName.gameObject.SetActive(active);
        nextCharacter.gameObject.SetActive(active);
        previousCharacter.gameObject.SetActive(active);
        connectedText.SetActive(!active);
    }

    #region Character Selection

    public void NextCharacter()
    {
        characterSelected = (characterSelected + 1) % characterDB.Length;
        characterSprite.sprite = characterDB.GetCharacter(characterSelected).characterIcon;
    }

    public void PreviousCharacter()
    {
        characterSelected = (characterSelected - 1 + characterDB.Length) % characterDB.Length;
        characterSprite.sprite = characterDB.GetCharacter(characterSelected).characterIcon;
    }

    public void LoadCharacter(int selected)
    {
        characterSelected = selected;
        characterSprite.sprite = characterDB.GetCharacter(selected).characterIcon;
    }

    #endregion

    #region Name Selection

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

    #endregion
}
