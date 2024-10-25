using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;

[CreateAssetMenu(fileName = "GameData", menuName = "Game/GameData", order = 1)]
public class GameData : ScriptableObject
{
    [Header("Game State")]
    [SerializeField] private int gameID;
    [SerializeField] private string dateGame;
    [SerializeField] private GameState gameState;
    [SerializeField] private int yearsToPlay = 10;
    [SerializeField] private int currentYear = 1;

    [Header("Players")]
    [SerializeField] private List<PlayerData> playersData = new List<PlayerData>();
    [SerializeField] private int initialPlayerIndex = 0;
    [SerializeField] private int turnPlayer;

    [Header("Cards & Questions")]
    [SerializeField] private List<QuestionData> questionList;
    [SerializeField] private List<ExpenseCard> expenseCards;
    [SerializeField] private List<InvestmentCard> investmentCards;
    [SerializeField] private List<IncomeCard> incomeCards;
    [SerializeField] private List<EventCard> eventCards;
    [SerializeField] private TextAsset jsonFile;

    [Header("Asset Bundle Settings")]
    [SerializeField] private string defaultBundlePath = "Assets/Bundles/DefaultBundle/defaultbundle";
    [SerializeField] private string assetBundleDirectory;
    [SerializeField] private string currentBundlePath;
    [SerializeField] private string bundleName;
    private AssetBundle assetbundle;

    // TODO: Getters y Setters
    public GameState GameState { get => gameState; set => gameState = value; }
    public int YearsToPlay { get => yearsToPlay; set => yearsToPlay = value; }
    public int CurrentYear { get => currentYear; set => currentYear = value; }

    public List<PlayerData> PlayersData { get => playersData; set => playersData = value; }
    public int InitialPlayerIndex { get => initialPlayerIndex; set => initialPlayerIndex = value; }
    public int TurnPlayer { get => turnPlayer; set => turnPlayer = value; }

    public List<QuestionData> QuestionList { get => questionList; set => questionList = value; }
    public List<ExpenseCard> ExpenseCards { get => expenseCards; set => expenseCards = value; }
    public List<InvestmentCard> InvestmentCards { get => investmentCards; set => investmentCards = value; }
    public List<IncomeCard> IncomeCards { get => incomeCards; set => incomeCards = value; }
    public List<EventCard> EventCards { get => eventCards; set => eventCards = value; }

    public string BundleName { get => bundleName; set => bundleName = value; }

    private void OnEnable()
    {
        assetBundleDirectory = Path.Combine(Application.persistentDataPath, "AssetBundles");
    }

    // TODO: Crear una nueva partida
    public IEnumerator InitGameData(string bundle)
    {
        bundleName = bundle;
        if (bundleName == "Default")
            currentBundlePath = defaultBundlePath;
        else
            currentBundlePath = Path.Combine(assetBundleDirectory, bundleName);

        yield return LoadDataFromBundle();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("MultiplayerLocal");
    }

    // TODO: Obtener el directorio de los Asset Bundles
    private IEnumerator LoadDataFromBundle()
    {
        yield return LoadBundle();
        if (assetbundle == null)
            yield break;
        yield return LoadQuestionsFromBundle();
        yield return LoadCardsFromBundle();
        assetbundle.Unload(false);
    }

    private IEnumerator LoadBundle()
    {
        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(currentBundlePath);
        yield return bundleRequest;
        assetbundle = bundleRequest.assetBundle;
    }

    private IEnumerator LoadCardsFromBundle()
    {
        yield return LoadExpenseCards(assetbundle);
        yield return LoadInvestmentCards(assetbundle);
        yield return LoadIncomeCards(assetbundle);
        yield return LoadEventCards(assetbundle);
    }

    private IEnumerator LoadExpenseCards(AssetBundle bundle)
    {
        AssetBundleRequest loadExpenseCardsRequest = bundle.LoadAllAssetsAsync<ExpenseCard>();
        yield return loadExpenseCardsRequest;
        if (expenseCards != null)
            expenseCards.AddRange(loadExpenseCardsRequest.allAssets.OfType<ExpenseCard>());
        else
            expenseCards = loadExpenseCardsRequest.allAssets.OfType<ExpenseCard>().ToList();
    }

    private IEnumerator LoadInvestmentCards(AssetBundle bundle)
    {
        AssetBundleRequest loadInvestmentCardsRequest = bundle.LoadAllAssetsAsync<InvestmentCard>();
        yield return loadInvestmentCardsRequest;
        if (investmentCards != null)
            investmentCards.AddRange(loadInvestmentCardsRequest.allAssets.OfType<InvestmentCard>());
        else
            investmentCards = loadInvestmentCardsRequest.allAssets.OfType<InvestmentCard>().ToList();
    }

    private IEnumerator LoadIncomeCards(AssetBundle bundle)
    {
        AssetBundleRequest loadIncomeCardsRequest = bundle.LoadAllAssetsAsync<IncomeCard>();
        yield return loadIncomeCardsRequest;
        if (incomeCards != null)
            incomeCards.AddRange(loadIncomeCardsRequest.allAssets.OfType<IncomeCard>());
        else
            incomeCards = loadIncomeCardsRequest.allAssets.OfType<IncomeCard>().ToList();
    }

    private IEnumerator LoadEventCards(AssetBundle bundle)
    {
        AssetBundleRequest loadEventCardsRequest = bundle.LoadAllAssetsAsync<EventCard>();
        yield return loadEventCardsRequest;
        if (eventCards != null)
            eventCards.AddRange(loadEventCardsRequest.allAssets.OfType<EventCard>());
        else
            eventCards = loadEventCardsRequest.allAssets.OfType<EventCard>().ToList();
    }

    private IEnumerator LoadQuestionsFromBundle()
    {
        AssetBundleRequest loadJsonRequest = assetbundle.LoadAssetAsync<TextAsset>("Questions");
        yield return loadJsonRequest;
        jsonFile = loadJsonRequest.asset as TextAsset;
        if (jsonFile != null)
        {
            QuestionList questionJSON = JsonUtility.FromJson<QuestionList>(jsonFile.text);
            questionList = new List<QuestionData>(questionJSON.questions);
        }
        else
        {
            Debug.LogError("No se encontró el archivo JSON en el Asset Bundle.");
        }
    }

    // TODO: Funiones para obtener tarjetas y preguntas aleatorias
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
            LoadBundle();
            LoadQuestionsFromBundle();
            assetbundle.Unload(false);
            return GetRandomQuestion();
        }
    }

    public List<ExpenseCard> GetRandomExpenseCards(int count)
    {
        List<ExpenseCard> selectedCards = new List<ExpenseCard>();
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

    public List<InvestmentCard> GetRandomInvestmentCards(int count)
    {
        PlayerData currentPlayer = playersData[TurnPlayer];
        List<InvestmentCard> selectedCards = new List<InvestmentCard>();
        List<InvestmentCard> availableCards = investmentCards
            .Where(card => !currentPlayer.Investments.Any(inv => inv.NameInvestment == card.title))
            .ToList();

        if (availableCards.Count == 0)
            availableCards = new List<InvestmentCard>(investmentCards);

        for (int i = 0; i < count; i++)
        {
            if (availableCards.Count == 0)
                break;
            int randomIndex = Random.Range(0, availableCards.Count);
            selectedCards.Add(availableCards[randomIndex]);
            availableCards.RemoveAt(randomIndex);
        }

        return selectedCards;
    }

    public List<IncomeCard> GetRandomIncomeCards(int count)
    {
        List<IncomeCard> selectedCards = new List<IncomeCard>();
        List<IncomeCard> availableCards = new List<IncomeCard>(incomeCards);

        for (int i = 0; i < count; i++)
        {
            if (availableCards.Count == 0)
                break;
            int randomIndex = Random.Range(0, availableCards.Count);
            selectedCards.Add(availableCards[randomIndex]);
            availableCards.RemoveAt(randomIndex);
        }

        return selectedCards;
    }

    public List<EventCard> GetRandomEventCards(int count)
    {
        List<EventCard> selectedCards = new List<EventCard>();
        List<EventCard> availableCards = new List<EventCard>(eventCards);

        for (int i = 0; i < count; i++)
        {
            if (availableCards.Count == 0)
                break;
            int randomIndex = Random.Range(0, availableCards.Count);
            selectedCards.Add(availableCards[randomIndex]);
            availableCards.RemoveAt(randomIndex);
        }


        return selectedCards;
    }

    //TODO: Limpiar los datos de los jugadores
    public void ClearGameData()
    {
        gameID = 0;
        dateGame = "";
        gameState = GameState.EnCurso;
        yearsToPlay = 10;
        currentYear = 1;

        playersData = new List<PlayerData>();
        initialPlayerIndex = 0;
        turnPlayer = 0;

        questionList = new List<QuestionData>();
        expenseCards = new List<ExpenseCard>();
        investmentCards = new List<InvestmentCard>();
        incomeCards = new List<IncomeCard>();
        eventCards = new List<EventCard>();
        jsonFile = null;

        defaultBundlePath = "Assets/Bundles/DefaultBundle/defaultbundle";
        assetBundleDirectory = Path.Combine(Application.persistentDataPath, "AssetBundles");
        currentBundlePath = "";
        bundleName = "";
        assetbundle = null;
    }

    public bool DataExists()
    {
        return PlayersData.Count > 0;
    }
}
