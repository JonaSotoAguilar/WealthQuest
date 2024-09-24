using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestionPanel : MonoBehaviour
{
    public TextMeshProUGUI questionText;
    public Button[] optionButtons;
    private int correctAnswerIndex;
    private PlayerStats currentPlayerStats;

    void Start()
    {
        // Inicialmente ocultar el panel
        gameObject.SetActive(false);
    }

    public void SetupQuestion(string question, string[] options, int correctIndex, PlayerStats playerStats)
    {
        questionText.text = question;
        correctAnswerIndex = correctIndex;
        currentPlayerStats = playerStats;

        for (int i = 0; i < options.Length; i++)
        {
            optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = options[i];
            int index = i;  // Copia local para evitar problemas con el closure en lambdas
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => Answer(index));
        }

        ShowPanel(true);
    }

    void Answer(int index)
    {
        ShowPanel(false);
        if (index == correctAnswerIndex)
        {
            currentPlayerStats.ModificarPuntaje(10);  // Asume un puntaje fijo por correcta
            HUDManager.instance.ActualizarHUD(currentPlayerStats);
        }
        // Aquí puedes manejar una respuesta incorrecta si es necesario
    }

    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
