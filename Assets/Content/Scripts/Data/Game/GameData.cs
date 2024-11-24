using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using System;

[CreateAssetMenu(fileName = "GameData", menuName = "Game/GameData", order = 1)]
public class GameData : ScriptableObject
{
    [Header("Game State")]
    [SerializeField] public TimeSpan timePlayed;
    [SerializeField] public int yearsToPlay;
    [SerializeField] public int currentYear;

    [Header("Players")]
    [SerializeField] public List<PlayerData> playersData;
    [SerializeField] public int initialPlayerIndex;
    [SerializeField] public int indexTurn;
    [SerializeField] public string turnPlayer;

    [Header("Asset Bundle Settings")]
    [SerializeField] public string defaultBundlePath = "Assets/Bundles/DefaultBundle/defaultbundle";
    [SerializeField] public string assetBundleDirectory;
    [SerializeField] public string currentBundlePath;
    [SerializeField] public string bundleName;
    public AssetBundle assetbundle;

    [Header("Cards & Questions")]
    [SerializeField] public List<QuestionData> allQuestions;
    [SerializeField] public List<QuestionData> questions;
    [SerializeField] public List<ExpenseCard> expenseCards;
    [SerializeField] public List<InvestmentCard> investmentCards;
    [SerializeField] public List<IncomeCard> incomeCards;
    [SerializeField] public List<EventCard> eventCards;
    [SerializeField] public TextAsset jsonFile;

    private void OnEnable()
    {
        assetBundleDirectory = Path.Combine(Application.persistentDataPath, "AssetBundles");
    }

    // TODO: Crear una nueva partida
    public IEnumerator LoadCardsAndQuestions(string bundle)
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
            questions = new List<QuestionData>(questionJSON.questions);
            allQuestions = new List<QuestionData>(questions);
        }
        else
        {
            Debug.LogError("No se encontró el archivo JSON en el Asset Bundle.");
        }
    }

    // TODO: Funiones para obtener tarjetas y preguntas aleatorias
    public QuestionData GetRandomQuestion()
    {
        if (questions.Count == 0) ResetQuestions();

        if (questions != null && questions.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, questions.Count);
            QuestionData selectedQuestion = questions[randomIndex];
            return selectedQuestion;
        }
        else
        {
            Debug.LogError("No hay preguntas disponibles.");
            return null;
        }
    }

    public QuestionData GetQuestionData(int index)
    {
        if (questions.Count == 0) ResetQuestions();

        if (questions != null && questions.Count > 0)
        {
            QuestionData selectedQuestion = questions[index];
            return selectedQuestion;
        }
        else
        {
            Debug.LogError("No hay preguntas disponibles.");
            return null;
        }
    }

    public void ResetQuestions() => questions = new List<QuestionData>(allQuestions);

    public List<ExpenseCard> GetRandomExpenseCards(int count)
    {
        List<ExpenseCard> selectedCards = new List<ExpenseCard>();
        if (expenseCards != null && expenseCards.Count > 0)
        {
            List<ExpenseCard> availableCards = new List<ExpenseCard>(expenseCards);
            for (int i = 0; i < count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, availableCards.Count);
                selectedCards.Add(availableCards[randomIndex]);
                availableCards.RemoveAt(randomIndex);
            }
        }

        return selectedCards;
    }

    public List<InvestmentCard> GetRandomInvestmentCards(int count)
    {
        PlayerData currentPlayer = playersData[indexTurn];
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
            int randomIndex = UnityEngine.Random.Range(0, availableCards.Count);
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
            int randomIndex = UnityEngine.Random.Range(0, availableCards.Count);
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
            int randomIndex = UnityEngine.Random.Range(0, availableCards.Count);
            selectedCards.Add(availableCards[randomIndex]);
            availableCards.RemoveAt(randomIndex);
        }


        return selectedCards;
    }

    //TODO: Limpiar los datos de los jugadores
    public void ClearGameData()
    {
        timePlayed = new TimeSpan();
        yearsToPlay = 2;
        currentYear = 1;

        playersData = new List<PlayerData>();
        initialPlayerIndex = 0;
        indexTurn = 0;
        turnPlayer = "";

        allQuestions = new List<QuestionData>();
        questions = new List<QuestionData>();
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
        return playersData.Count > 0;
    }
}
