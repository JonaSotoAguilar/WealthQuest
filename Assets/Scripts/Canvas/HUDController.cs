using UnityEngine;
using TMPro;  // Importar la librería necesaria para TextMeshPro

public class HUDController : MonoBehaviour
{


    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI moneyText;

    public void UpdatePlayer(PlayerData player)
    {
        turnText.text = "Turno: " + player.PlayerName;
        scoreText.text = "Puntaje: " + player.ScoreKFP;
        moneyText.text = "Dinero: $" + player.Money;
    }

    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
