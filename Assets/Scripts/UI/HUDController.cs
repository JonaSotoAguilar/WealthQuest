using UnityEngine;
using TMPro;  // Importar la librería necesaria para TextMeshPro

public class HUDController : MonoBehaviour
{


    [SerializeField]
    private TextMeshProUGUI turnText;
    [SerializeField]
    private TextMeshProUGUI moneyText;
    [SerializeField]
    private TextMeshProUGUI scoreText;

    public void UpdateHUD(PlayerData player)
    {
        turnText.text = "Turno: " + player.PlayerName;
        moneyText.text = "Dinero: $" + player.Money;
        scoreText.text = "Puntaje: " + player.Score;
    }

    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
