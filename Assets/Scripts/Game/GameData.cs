using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    // Si no necesitas ajustar estos valores desde el Inspector, puedes eliminar el campo y usar solo la propiedad.
    [SerializeField]
    private int gameID; // ID del juego. Considera usar solo la propiedad si no necesitas serializarlo.

    private GameState gameState;
    private int turnPlayer;

    [SerializeField]
    private int numPlayers = 2; // Número de jugadores. Serializado para ajustar desde el Inspector.

    private List<PlayerController> players;

    // Guardar el juego
    private void SaveGame()
    {
        // Implementación de guardado
    }

    // Cargar el juego
    private void LoadGame()
    {
        // Implementación de carga
    }
}
