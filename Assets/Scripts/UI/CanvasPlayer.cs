using UnityEngine;

public class CanvasPlayer : MonoBehaviour
{
    private QuestionController questionPanel;
    public QuestionController QuestionPanel { get => questionPanel; }

    private void Awake()
    {
        questionPanel = GetComponentInChildren<QuestionController>(); // Inicializar el QuestionPanelController
        questionPanel.ShowPanel(false); // Ocultar el panel de pregunta al inicio
    }

    private void ShowQuestion(bool visible)
    {
        questionPanel.ShowPanel(visible);
    }
}
