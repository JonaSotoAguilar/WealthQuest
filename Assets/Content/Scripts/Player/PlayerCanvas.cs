using UnityEngine;
using UnityEngine.UI;

public class PlayerCanvas : MonoBehaviour
{
    [SerializeField] private QuestionPanel questionPanel;
    [SerializeField] private CardsPanel cardsPanel;
    [SerializeField] private InvestPanel investPanel;

    public QuestionPanel QuestionPanel { get => questionPanel; }
    public CardsPanel CardsPanel { get => cardsPanel; }

    private void Awake()
    {
        questionPanel = GetComponentInChildren<QuestionPanel>();
        questionPanel.ShowPanel(false);

        cardsPanel = GetComponentInChildren<CardsPanel>();
        investPanel = cardsPanel.GetComponentInChildren<InvestPanel>();
        cardsPanel.InvestPanel = investPanel;
        cardsPanel.CardGrid = cardsPanel.GetComponentInChildren<GridLayoutGroup>().transform;
        investPanel.ShowPanel(false);
        cardsPanel.ShowPanel(false);
    }

    public void ActiveCanvas(bool interactable)
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.enabled = interactable;
    }

}
