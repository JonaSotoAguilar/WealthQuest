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

    public void UpdateHUD(PlayerController player)
    {
        turnText.text = "Turno: " + player.GetPlayerName();
        moneyText.text = "Dinero: $" + player.GetMoney();
        scoreText.text = "Puntaje: " + player.GetScore();
    }

    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
