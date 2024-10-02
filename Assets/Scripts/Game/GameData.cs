using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; } // Instancia única de GameData
    private int gameID; // ID del juego. Considera usar solo la propiedad si no necesitas serializarlo.

    [SerializeField] private int numPlayers = 1; // Número de jugadores. Serializado para ajustar desde el Inspector.
    private GameState gameState = GameState.EnCurso;
    private int turnPlayer = 0;
    private PlayerData[] players;
    private List<QuestionData> questionList;

    public int NumPlayers { get => numPlayers; } // Propiedad solo de lectura.
    public GameState GameState { get => gameState; set => gameState = value; }
    public int TurnPlayer { get => turnPlayer; set => turnPlayer = value; }
    public PlayerData[] Players { get => players; set => players = value; }
    public List<QuestionData> QuestionList { get => questionList; set => questionList = value; }


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
