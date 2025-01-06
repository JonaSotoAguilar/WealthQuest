using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    [Header("Online Game")]
    [SerializeField] private Button joinOnlineButton;
    [SerializeField] private TMP_InputField joinInput;

    private void Start()
    {
        joinOnlineButton.interactable = false;
        joinInput.onValueChanged.AddListener(ValidateInput);
        joinInput.onValidateInput += ValidateCharacter;
    }

    private void ValidateInput(string input)
    {
        joinInput.text = input.ToUpper().Trim();
        joinOnlineButton.interactable = joinInput.text.Length == 6;
    }

    private char ValidateCharacter(string text, int charIndex, char addedChar)
    {
        if (char.IsLetterOrDigit(addedChar))
        {
            return char.ToUpper(addedChar);
        }
        return '\0';
    }


}
