using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance;

    [Space, Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public Button loginButton;
    public TextMeshProUGUI warningLoginText;

    // Registration Variables
    [Space, Header("Registration")]
    public TMP_InputField nameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField confirmPasswordRegisterField;
    public Button registerButton;
    public TextMeshProUGUI warningRegisterText;
    public TMP_Dropdown genderDropdown;
    public TMP_InputField birthDatePicker;
    public TMP_Dropdown roleDropdown;
    public TextMeshProUGUI roleText;

    [Space, Header("Forgot Password")]
    public TMP_InputField emailForgotField;
    public TextMeshProUGUI warningForgotText;

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
        emailLoginField.onValueChanged.AddListener((value) =>
        {
            emailLoginField.text = value.ToLower();
            ValidateLoginFields();
        });
        passwordLoginField.onValueChanged.AddListener((_) => ValidateLoginFields());

        // Registration field listeners
        nameRegisterField.onValueChanged.AddListener((_) => ValidateRegistrationFields());
        emailRegisterField.onValueChanged.AddListener((value) =>
        {
            emailRegisterField.text = value.ToLower();
            ValidateRegistrationFields();
        });
        passwordRegisterField.onValueChanged.AddListener((_) => ValidateRegistrationFields());
        confirmPasswordRegisterField.onValueChanged.AddListener((_) => ValidateRegistrationFields());
        birthDatePicker.onValueChanged.AddListener((_) => ValidateRegistrationFields());

        // Forgot field listeners
        emailForgotField.onValueChanged.AddListener((value) =>
        {
            emailForgotField.text = value.ToLower();
            ValidateForgotFields();
        });

        // Dropdown listeners
        roleDropdown.onValueChanged.AddListener(UpdateRoleMessage);

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
        bool isBirthDateValid = !string.IsNullOrEmpty(birthDatePicker.text);

        registerButton.interactable = isNameValid && isEmailValid && isPasswordValid && doPasswordsMatch && isBirthDateValid;

        if (Application.internetReachability == NetworkReachability.NotReachable)
            warningRegisterText.text = "Se requiere conexión a internet para registrarte.";
        else if (!isNameValid)
            warningRegisterText.text = "El nombre es obligatorio.";
        else if (!isEmailValid)
            warningRegisterText.text = "Se requiere correo electrónico válido.";
        else if (!isPasswordValid)
            warningRegisterText.text = "La contraseña debe tener al menos 6 caracteres.";
        else if (!doPasswordsMatch)
            warningRegisterText.text = "Las contraseñas no coinciden.";
        else if (!isBirthDateValid)
            warningRegisterText.text = "La fecha de nacimiento es obligatoria.";
        else
            warningRegisterText.text = "";
    }

    private void ValidateForgotFields()
    {
        // Check if email is filled
        bool isEmailValid = !string.IsNullOrEmpty(emailForgotField.text) && IsValidEmail(emailForgotField.text);

        if (Application.internetReachability == NetworkReachability.NotReachable)
            warningForgotText.text = "Se requiere conexión a internet para recuperar tu contraseña.";
        else if (!isEmailValid)
            warningForgotText.text = "Formato de correo electrónico no válido.";
        else
            warningForgotText.text = "";
    }

    private bool IsValidEmail(string email)
    {
        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern);
    }

    public void ResetLoginFields()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
        warningLoginText.text = "";
    }

    public void ResetRegistrationFields()
    {
        nameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        confirmPasswordRegisterField.text = "";
        warningRegisterText.text = "";
        birthDatePicker.text = "";
        genderDropdown.value = 0;
        roleDropdown.value = 0;
    }

    private void UpdateRoleMessage(int roleIndex)
    {
        switch (roleIndex)
        {
            case 0:
                roleText.text = "Acceso a funciones básicas del juego.";
                break;
            case 1:
                roleText.text = "Acceso a funciones básicas del juego y herramientas de creación.";
                break;
            default:
                roleText.text = "Selecciona un rol válido.";
                break;
        }
    }

    public void ResetForgotFields()
    {
        emailForgotField.text = "";
        warningForgotText.text = "";
    }
}
