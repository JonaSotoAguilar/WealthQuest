using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Método que se llamará cuando el jugador haga clic en el botón Jugar
    public void Play()
    {
        // Cargar la escena del juego llamada "BoardGame"
        SceneManager.LoadScene("BoardGame");
    }
}
