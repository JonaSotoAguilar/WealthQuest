using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager instance;  // Singleton
    private HUDController hud;
    private QuestionPanelController questionPanel;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // Mantener el HUDManager al cambiar de escena
        }
        else
        {
            Destroy(gameObject);  // Si ya existe una instancia, destruir este objeto
        }
        hud = GetComponentInChildren<HUDController>(); // Inicializar el HUDController
        questionPanel = GetComponentInChildren<QuestionPanelController>(); // Inicializar el QuestionPanelController
        questionPanel.ShowPanel(false); // Ocultar el panel de pregunta al inicio
    }

    // Actualizar el HUD
    public void UpdateHUD(PlayerController player)
    {
        hud.UpdateHUD(player);
    }
}
