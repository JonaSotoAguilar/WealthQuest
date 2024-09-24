using UnityEngine;
using TMPro;  // Importa la librería TextMeshPro

public class HUDManager : MonoBehaviour
{
    public static HUDManager instance;

    public TextMeshProUGUI turnoText;
    public TextMeshProUGUI dineroText;
    public TextMeshProUGUI puntajeText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ActualizarHUD(PlayerStats playerStats)
    {
        turnoText.text = "Turno: " + playerStats.turno;
        dineroText.text = "Dinero: $" + playerStats.dinero;
        puntajeText.text = "Puntaje: " + playerStats.puntaje;
    }
}
