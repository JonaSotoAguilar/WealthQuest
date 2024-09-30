using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameData : MonoBehaviour
{
    // Si no necesitas ajustar estos valores desde el Inspector, puedes eliminar el campo y usar solo la propiedad.
    [SerializeField]
    private int gameID; // ID del juego. Considera usar solo la propiedad si no necesitas serializarlo.

    [SerializeField]
    private int numPlayers = 1; // Número de jugadores. Serializado para ajustar desde el Inspector.
    public int NumPlayers { get => numPlayers; } // Propiedad solo de lectura.

    private GameState gameState;
    public GameState GameState { get => gameState; set => gameState = value; }

    private int turnPlayer;
    public int TurnPlayer { get => turnPlayer; set => turnPlayer = value; }

    private PlayerData[] players;
    public PlayerData[] Players { get => players; set => players = value; }

    // Inicialización de los datos del juego
    void Awake()
    {
        turnPlayer = 0;
        gameState = GameState.EnCurso;
    }

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
