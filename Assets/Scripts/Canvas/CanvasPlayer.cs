using UnityEngine;

public class CanvasPlayer : MonoBehaviour
{
    private QuestionPanel questionPanel;
    private CardsPanel cardsPanel;

    public QuestionPanel QuestionPanel { get => questionPanel; }
    public CardsPanel CardsPanel { get => cardsPanel; }

    private void Awake()
    {
        questionPanel = GetComponentInChildren<QuestionPanel>(); // Inicializar el QuestionPanelController
        cardsPanel = GetComponentInChildren<CardsPanel>(); // Inicializar el CardsPanelController
        questionPanel.ShowPanel(false); // Ocultar el panel de pregunta al inicio
        cardsPanel.ShowPanel(false); // Ocultar el panel de cartas al inicio
    }
}
