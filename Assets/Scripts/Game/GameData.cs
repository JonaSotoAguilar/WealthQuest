using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; } // Instancia única de GameData
    private int gameID; // ID del juego. Considera usar solo la propiedad si no necesitas serializarlo.

    [SerializeField] private int numPlayers; // Número de jugadores. Serializado para ajustar desde el Inspector.
    [SerializeField] private PlayerData[] players;
    [SerializeField] private GameState gameState;
    [SerializeField] private int turnPlayer;
    [SerializeField] private List<QuestionData> questionList;

    public int NumPlayers { get => numPlayers; } // Propiedad solo de lectura.
    public GameState GameState { get => gameState; set => gameState = value; }
    public int TurnPlayer { get => turnPlayer; set => turnPlayer = value; }
    public PlayerData[] Players { get => players; set => players = value; }
    public List<QuestionData> QuestionList { get => questionList; set => questionList = value; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void NewGame()
    {
        // Reduce el tamanho del array de jugadores quitando los nulos
        players = players.Where(p => p != null).ToArray();
    }

    // Guardar el juego
    private void SaveGame()
    {
        // Implementación de guardado
    }

    // Cargar el juego
    private void LoadGame()
    {
        // Buscar todos los PlayerData en la escena
        var players = FindObjectsByType<PlayerData>(FindObjectsSortMode.None); // Buscar jugadores
        // Recorre los jugadores y desactiva aquellos cuyo índice sea mayor o igual al número de jugadores
        foreach (var player in players)
        {
            int index = player.Index;
            players[index] = player;
        }
    }
}
