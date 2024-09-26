using UnityEngine;

public class GameData : MonoBehaviour
{

    public Player[] players;
    public int currentPlayer = 0;
    public GameState gameState = GameState.EnCurso;

    public void InitializePlayers(int numberOfPlayers)
    {
        players = new Player[numberOfPlayers];

        for (int i = 0; i < numberOfPlayers; i++)
        {
            GameObject playerObject = GameObject.Find("Player_" + (i + 1));
            if (playerObject != null)
            {
                players[i] = playerObject.GetComponent<Player>(); // Obtener el componente Player del GameObject
            }
            else
            {
                Debug.LogError("Player_" + (i + 1) + " no se encontró en la escena.");
            }
        }

        // Comprobar si HUDManager.instance está correctamente inicializado
        if (HUDManager.instance != null)
        {
            HUDManager.instance.ActualizarHUD(players[currentPlayer]);
        }
        else
        {
            Debug.LogError("HUDManager.instance no ha sido inicializado.");
        }
    }

    public void UpdateTurn()
    {
        if (gameState == GameState.Finalizado)
        {
            Debug.Log("El juego ha terminado. No se puede actualizar el turno.");
            return;
        }

        // Avanzar al siguiente jugador que esté en EN CURSO
        do
        {
            currentPlayer = (currentPlayer + 1) % players.Length;
        } while (players[currentPlayer].playerState == GameState.Finalizado && !CheckIfAllPlayersFinished());

        // Si todos los jugadores han terminado, el juego pasa a FINALIZADO
        if (CheckIfAllPlayersFinished())
        {
            gameState = GameState.Finalizado;
            Debug.Log("Todos los jugadores han terminado. El juego ha finalizado.");
            return;  // No continuar con la actualización del HUD si el juego ha finalizado
        }

        // Actualizar el HUD para el jugador en curso
        HUDManager.instance.ActualizarHUD(players[currentPlayer]);
    }

    private bool CheckIfAllPlayersFinished()
    {
        foreach (Player player in players)
        {
            if (player.playerState == GameState.EnCurso)
            {
                return false; // Si hay algún jugador en curso, el juego no ha terminado
            }
        }
        return true; // Todos los jugadores están en estado FINALIZADO
    }

    public void ShowStats()
    {
        Debug.Log("Game State: " + gameState);
        foreach (Player player in players)
        {
            Debug.Log("Name: " + player.playerName + ", Money: " + player.money + ", Score: " + player.score + ", State: " + player.playerState);
        }
    }
}