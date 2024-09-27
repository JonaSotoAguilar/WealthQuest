using UnityEngine;
using TMPro;  // Importar la librería necesaria para TextMeshPro

public class HUDController : MonoBehaviour
{
    public static HUDController instance;  // Singleton
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI scoreText;

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

    void Start()
    {
        gameObject.SetActive(false); // Inicialmente oculta el HUD
    }

    public void UpdateHUD(Player player)
    {
        turnText.text = "Turno: " + player.playerName;
        moneyText.text = "Dinero: $" + player.money;
        scoreText.text = "Puntaje: " + player.score;
    }
}
