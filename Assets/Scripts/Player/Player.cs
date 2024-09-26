using UnityEngine;

[System.Serializable]
public class Player : MonoBehaviour
{
    public string playerName;
    public int money;
    public int score;
    public GameState playerState = GameState.EnCurso; // Estado del jugador, inicializado a EN CURSO

    // Constructor for initializing the player if necessary
    public Player(string initialName = "Jona", int initialMoney = 0, int initialScore = 0)
    {
        playerName = initialName;
        money = initialMoney;
        score = initialScore;
        playerState = GameState.EnCurso;
    }

    public void ModifyMoney(int amount)
    {
        money += amount;
    }

    public void ModifyScore(int points)
    {
        score += points;
    }
}
