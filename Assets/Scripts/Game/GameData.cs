using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; } // Instancia única de GameData
    private int gameID; // ID del juego. Considera usar solo la propiedad si no necesitas serializarlo.

    [Header("Game State")]
    [SerializeField] private PlayerData[] players;
    [SerializeField] private GameState gameState;
    [SerializeField] private int turnPlayer;

    [Header("Cards & Questions")]
    [SerializeField] private List<QuestionData> questionList;
    [SerializeField] private List<ExpenseCard> expenseCards;
    [SerializeField] private TextAsset jsonFile;

    public GameState GameState { get => gameState; set => gameState = value; }
    public int TurnPlayer { get => turnPlayer; set => turnPlayer = value; }
    public PlayerData[] Players { get => players; set => players = value; }
    public List<QuestionData> QuestionList { get => questionList; set => questionList = value; }
    public List<ExpenseCard> ExpenseCards { get => expenseCards; set => expenseCards = value; }

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
        players = players.Where(p => p != null).ToArray();
        LoadQuestionList();
        Addressables.LoadAssetsAsync<ExpenseCard>("ExpenseCards", null).Completed += OnExpenseCardsLoaded;
    }

    // Guardar el juego
    public void SaveGame()
    {
        // Implementación de guardado
    }

    // Cargar el juego
    public void LoadGame()
    {

    }


    private void LoadQuestionList()
    {
        // Ruta completa al archivo JSON en tu proyecto
        QuestionList questionJSON = JsonUtility.FromJson<QuestionList>(jsonFile.text);

        // Convertir el array de preguntas a una lista
        List<QuestionData> questions = new List<QuestionData>(questionJSON.questions);

        // Asignar la lista de preguntas a GameData
        questionList = questions;
    }

    // Selecciona una pregunta aleatoria de la lista de preguntas
    public QuestionData GetRandomQuestion()
    {
        if (questionList != null && questionList.Count > 0)
        {
            int randomIndex = Random.Range(0, questionList.Count);
            QuestionData selectedQuestion = questionList[randomIndex];
            questionList.RemoveAt(randomIndex);

            return selectedQuestion;
        }
        else
        {
            return null;
        }
    }

    // Selecciona tarjetas aleatorias de la lista y las retorna sin eliminarlas
    public List<ExpenseCard> GetRandomExpenseCards(int count)
    {
        List<ExpenseCard> selectedCards = new List<ExpenseCard>();

        // Obtiene 2 cartas aleatorias de la lista de cartas de gasto diferentes
        if (expenseCards != null && expenseCards.Count > 0)
        {
            List<ExpenseCard> availableCards = new List<ExpenseCard>(expenseCards);
            for (int i = 0; i < count; i++)
            {
                int randomIndex = Random.Range(0, availableCards.Count);
                selectedCards.Add(availableCards[randomIndex]);
                availableCards.RemoveAt(randomIndex);
            }
        }

        return selectedCards;
    }

    private void OnExpenseCardsLoaded(AsyncOperationHandle<IList<ExpenseCard>> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            expenseCards = handle.Result.ToList();
            Debug.Log($"Cargadas {expenseCards.Count} ExpenseCards.");
        }
        else
        {
            Debug.LogError("Error cargando las ExpenseCards.");
        }
    }
}
