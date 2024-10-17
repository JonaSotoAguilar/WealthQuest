using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; } // Instancia única de GameData
    private int gameID; // ID del juego. Considera usar solo la propiedad si no necesitas serializarlo.

    [Header("Game State")]
    [SerializeField] private PlayerData[] players;
    [SerializeField] private GameState gameState;
    [SerializeField] private int turnPlayer;

    [Header("Cards & Questions")]
    [SerializeField] private TextAsset jsonFile; // Esto ya se cargará desde el AssetBundle
    [SerializeField] private List<QuestionData> questionList;
    [SerializeField] private List<ExpenseCard> expenseCards;
    [SerializeField] private List<InvestmentCard> investmentCards;
    [SerializeField] private List<IncomeCard> incomeCards;
    [SerializeField] private List<EventCard> eventCards;

    [Header("Asset Bundle Settings")]
    private string defaultBundlePath = "Assets/Bundles/DefaultBundle/defaultbundle";    // Ruta del DefaultBundle
    private string assetBundleDirectory;                                                // Ruta a la carpeta de Asset Bundles
    private string currentBundlePath;                                                   // La ruta del Asset Bundle seleccionado

    public GameState GameState { get => gameState; set => gameState = value; }
    public int TurnPlayer { get => turnPlayer; set => turnPlayer = value; }
    public PlayerData[] Players { get => players; set => players = value; }
    public List<QuestionData> QuestionList { get => questionList; set => questionList = value; }
    public List<ExpenseCard> ExpenseCards { get => expenseCards; set => expenseCards = value; }
    public List<InvestmentCard> InvestmentCards { get => investmentCards; set => investmentCards = value; }

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
        assetBundleDirectory = Path.Combine(Application.persistentDataPath, "AssetBundles");
    }

    public IEnumerator NewGame(string bundleName)
    {
        players = players.Where(p => p != null).ToArray();

        // Verificar si se ha seleccionado el bundle "Default"
        if (bundleName == "Default")
            currentBundlePath = defaultBundlePath;
        else
            currentBundlePath = Path.Combine(assetBundleDirectory, bundleName);

        yield return StartCoroutine(LoadDataFromBundle(currentBundlePath));
        SceneManager.LoadScene("MultiplayerLocal");
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

    // Método para cargar los datos desde el Asset Bundle
    private IEnumerator LoadDataFromBundle(string bundlePath)
    {
        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
        yield return bundleRequest;

        AssetBundle bundle = bundleRequest.assetBundle;
        if (bundle == null)
        {
            Debug.LogError("Error al cargar el Asset Bundle: " + bundlePath);
            yield break;
        }

        // Cargar todas las ExpenseCards
        AssetBundleRequest loadExpenseCardsRequest = bundle.LoadAllAssetsAsync<ExpenseCard>();
        yield return loadExpenseCardsRequest;
        expenseCards = loadExpenseCardsRequest.allAssets.OfType<ExpenseCard>().ToList();

        // Cargar todas las InvestmentCards
        AssetBundleRequest loadInvestmentCardsRequest = bundle.LoadAllAssetsAsync<InvestmentCard>();
        yield return loadInvestmentCardsRequest;
        investmentCards = loadInvestmentCardsRequest.allAssets.OfType<InvestmentCard>().ToList();

        // Cargar todas las IncomeCards
        AssetBundleRequest loadIncomeCardsRequest = bundle.LoadAllAssetsAsync<IncomeCard>();
        yield return loadIncomeCardsRequest;
        incomeCards = loadIncomeCardsRequest.allAssets.OfType<IncomeCard>().ToList();

        // Cargar todas las EventCards
        AssetBundleRequest loadEventCardsRequest = bundle.LoadAllAssetsAsync<EventCard>();
        yield return loadEventCardsRequest;
        eventCards = loadEventCardsRequest.allAssets.OfType<EventCard>().ToList();

        // Cargar el archivo JSON de las preguntas
        AssetBundleRequest loadJsonRequest = bundle.LoadAssetAsync<TextAsset>("Questions");
        yield return loadJsonRequest;
        jsonFile = loadJsonRequest.asset as TextAsset;
        if (jsonFile != null)
        {
            LoadQuestionListFromJson(jsonFile);
        }
        else
        {
            Debug.LogError("No se encontró el archivo JSON en el Asset Bundle.");
        }

        bundle.Unload(false); // Descargar el Asset Bundle de la memoria
    }

    // Método para cargar las preguntas desde el archivo JSON
    private void LoadQuestionListFromJson(TextAsset json)
    {
        QuestionList questionJSON = JsonUtility.FromJson<QuestionList>(json.text);
        questionList = new List<QuestionData>(questionJSON.questions);
    }

    // Selecciona una pregunta aleatoria de la lista de preguntas
    public QuestionData GetRandomQuestion()
    {
        if (questionList != null && questionList.Count > 0)
        {
            int randomIndex = Random.Range(0, questionList.Count);
            QuestionData selectedQuestion = questionList[randomIndex];
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

    // Selecciona tarjetas aleatorias de la lista de tarjetas de inversión y las retorna sin eliminarlas
    public List<InvestmentCard> GetRandomInvestmentCards(int count)
    {
        List<InvestmentCard> selectedCards = new List<InvestmentCard>();

        // Obtiene cartas aleatorias de la lista de cartas de inversión, asegurándose de que sean diferentes
        if (investmentCards != null && investmentCards.Count > 0)
        {
            List<InvestmentCard> availableCards = new List<InvestmentCard>(investmentCards);
            for (int i = 0; i < count; i++)
            {
                if (availableCards.Count == 0)
                {
                    Debug.LogWarning("No hay suficientes tarjetas de inversión disponibles para seleccionar la cantidad solicitada.");
                    break;
                }
                int randomIndex = Random.Range(0, availableCards.Count);
                selectedCards.Add(availableCards[randomIndex]);
                availableCards.RemoveAt(randomIndex); // Asegúrate de que las tarjetas seleccionadas sean diferentes
            }
        }
        else
        {
            Debug.LogError("La lista de tarjetas de inversión está vacía o no existe.");
        }

        return selectedCards;
    }

    // Selecciona tarjetas aleatorias de la lista de tarjetas de ingreso y las retorna sin eliminarlas
    public List<IncomeCard> GetRandomIncomeCards(int count)
    {
        List<IncomeCard> selectedCards = new List<IncomeCard>();

        if (incomeCards != null && incomeCards.Count > 0)
        {
            List<IncomeCard> availableCards = new List<IncomeCard>(incomeCards);
            for (int i = 0; i < count; i++)
            {
                if (availableCards.Count == 0)
                {
                    Debug.LogWarning("No hay suficientes tarjetas de ingreso disponibles para seleccionar la cantidad solicitada.");
                    break;
                }
                int randomIndex = Random.Range(0, availableCards.Count);
                selectedCards.Add(availableCards[randomIndex]);
                availableCards.RemoveAt(randomIndex); // Asegúrate de que las tarjetas seleccionadas sean diferentes
            }
        }
        else
        {
            Debug.LogError("La lista de tarjetas de ingreso está vacía o no existe.");
        }

        return selectedCards;
    }

    // Selecciona tarjetas aleatorias de la lista de tarjetas de evento y las retorna sin eliminarlas
    public List<EventCard> GetRandomEventCards(int count)
    {
        List<EventCard> selectedCards = new List<EventCard>();

        if (eventCards != null && eventCards.Count > 0)
        {
            List<EventCard> availableCards = new List<EventCard>(eventCards);
            for (int i = 0; i < count; i++)
            {
                if (availableCards.Count == 0)
                {
                    Debug.LogWarning("No hay suficientes tarjetas de evento disponibles para seleccionar la cantidad solicitada.");
                    break;
                }
                int randomIndex = Random.Range(0, availableCards.Count);
                selectedCards.Add(availableCards[randomIndex]);
                availableCards.RemoveAt(randomIndex); // Asegúrate de que las tarjetas seleccionadas sean diferentes
            }
        }
        else
        {
            Debug.LogError("La lista de tarjetas de evento está vacía o no existe.");
        }

        return selectedCards;
    }


}
