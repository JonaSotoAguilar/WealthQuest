using UnityEngine;
using TMPro;  // Importar la librería necesaria para TextMeshPro

public class HUDManager : MonoBehaviour
{
    public static HUDManager instance;  // Singleton
    public TextMeshProUGUI turnoText;
    public TextMeshProUGUI dineroText;
    public TextMeshProUGUI puntajeText;

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
    }

    public void ActualizarHUD(Player player)
    {
        turnoText.text = "Turno: " + player.playerName;
        dineroText.text = "Dinero: $" + player.money;
        puntajeText.text = "Puntaje: " + player.score;
    }
}
