using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance;

    [Space]
    [Header("Login")]
    [SerializeField] private TMP_InputField emailLoginField;
    [SerializeField] private TMP_InputField passwordLoginField;
    public Button loginButton;
    public TextMeshProUGUI warningLoginText;

    // Registration Variables
    [Space]
    [Header("Registration")]
    [SerializeField] private TMP_InputField nameRegisterField;
    [SerializeField] private TMP_InputField emailRegisterField;
    [SerializeField] private TMP_InputField passwordRegisterField;
    [SerializeField] private TMP_InputField confirmPasswordRegisterField;
    public Button registerButton;
    public TextMeshProUGUI warningRegisterText;

    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registrationPanel;

    private void Awake()
    {
        CreateInstance();
        SetupListeners();
    }

    private void CreateInstance()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void SetupListeners()
    {
        // Login field listeners
        emailLoginField.onValueChanged.AddListener((_) => ValidateLoginFields());
        passwordLoginField.onValueChanged.AddListener((_) => ValidateLoginFields());

        // Registration field listeners
        nameRegisterField.onValueChanged.AddListener((_) => ValidateRegistrationFields());
        emailRegisterField.onValueChanged.AddListener((_) => ValidateRegistrationFields());
        passwordRegisterField.onValueChanged.AddListener((_) => ValidateRegistrationFields());
        confirmPasswordRegisterField.onValueChanged.AddListener((_) => ValidateRegistrationFields());

        // Disable buttons initially
        loginButton.interactable = false;
        registerButton.interactable = false;
    }

    private void ValidateLoginFields()
    {
        // Check if email and password are filled
        bool isEmailValid = !string.IsNullOrEmpty(emailLoginField.text) && IsValidEmail(emailLoginField.text);
        bool isPasswordValid = !string.IsNullOrEmpty(passwordLoginField.text);

        loginButton.interactable = isEmailValid && isPasswordValid;

        if (Application.internetReachability == NetworkReachability.NotReachable)
            warningRegisterText.text = "Se requiere conexión a internet para iniciar sesión.";
        else if (!isEmailValid)
            warningLoginText.text = "Formato de correo electrónico no válido.";
        else if (!isPasswordValid)
            warningLoginText.text = "La contraseña es obligatoria.";
        else
            warningLoginText.text = "";
    }

    private void ValidateRegistrationFields()
    {
        // Check if all fields are filled
        bool isNameValid = !string.IsNullOrEmpty(nameRegisterField.text);
        bool isEmailValid = !string.IsNullOrEmpty(emailRegisterField.text) && IsValidEmail(emailRegisterField.text);
        bool isPasswordValid = !string.IsNullOrEmpty(passwordRegisterField.text) && passwordRegisterField.text.Length >= 6;
        bool doPasswordsMatch = passwordRegisterField.text == confirmPasswordRegisterField.text;

        registerButton.interactable = isNameValid && isEmailValid && isPasswordValid && doPasswordsMatch;

        if (Application.internetReachability == NetworkReachability.NotReachable)
            warningRegisterText.text = "Se requiere conexión a internet para registrarte.";
        else if (!isNameValid)
            warningRegisterText.text = "El nombre es obligatorio.";
        else if (!isEmailValid)
            warningRegisterText.text = "Se requiere correo electrónico";
        else if (!isPasswordValid)
            warningRegisterText.text = "La contraseña debe tener al menos 6 caracteres.";
        else if (!doPasswordsMatch)
            warningRegisterText.text = "Las contraseñas no coinciden.";
        else
            warningRegisterText.text = "";
    }

    private bool IsValidEmail(string email)
    {
        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern);
    }

    public void OpenLoginPanel()
    {
        ResetLoginFields();
        loginPanel.SetActive(true);
        registrationPanel.SetActive(false);
    }

    private void ResetLoginFields()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
        warningLoginText.text = "";
    }

    public void OpenRegistrationPanel()
    {
        ResetRegistrationFields();
        registrationPanel.SetActive(true);
        loginPanel.SetActive(false);
    }

    private void ResetRegistrationFields()
    {
        nameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        confirmPasswordRegisterField.text = "";
        warningRegisterText.text = "";
    }
}
