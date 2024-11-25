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
    [Header("Status")]
    public TimeSpan timePlayed;
    public int yearsToPlay;
    public int currentYear;

    [Header("Players")]
    public List<PlayerData> playersData;
    public int initialPlayerIndex;
    public int turnPlayer;

    [Header("Cards & Questions")]
    public List<QuestionData> allQuestionList;
    public List<QuestionData> questionList;
    public List<ExpenseCard> expenseCards;
    public List<InvestmentCard> investmentCards;
    public List<IncomeCard> incomeCards;
    public List<EventCard> eventCards;
    public TextAsset jsonFile;

    [Header("Asset Bundle Settings")]
    public string defaultBundlePath;
    public string assetBundleDirectory;
    public string currentBundlePath;
    public string bundleName;
    public AssetBundle assetbundle;

    private void OnEnable()
    {
        assetBundleDirectory = Path.Combine(Application.persistentDataPath, "AssetBundles");
    }

    // TODO: Crear una nueva partida


    public void StartGame()
    {
        SceneManager.LoadScene("MultiplayerLocal");
    }

    #region Asset Bundle

    public IEnumerator LoadCardsAndQuestions(string bundle)
    {
        bundleName = bundle;
        if (bundleName == "Default")
            currentBundlePath = defaultBundlePath;
        else
            currentBundlePath = Path.Combine(assetBundleDirectory, bundleName);

        yield return LoadDataFromBundle();
    }

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
            allQuestionList = new List<QuestionData>(questionJSON.questions);
        }
        else
        {
            Debug.LogError("No se encontró el archivo JSON en el Asset Bundle.");
        }
    }

    #endregion

    #region Getters Random

    public QuestionData GetRandomQuestion()
    {
        if (questionList == null || questionList.Count == 0) ResetQuestionList();

        int randomIndex = UnityEngine.Random.Range(0, questionList.Count);
        QuestionData selectedQuestion = questionList[randomIndex];
        return selectedQuestion;
    }

    public void ResetQuestionList()
    {
        questionList = new List<QuestionData>(allQuestionList);
    }

    public void DeleteQuestion(QuestionData question)
    {
        questionList.Remove(question);
    }

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
        PlayerData currentPlayer = playersData[turnPlayer];
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

    #endregion

    #region Initialization
    public void ClearGameData()
    {
        timePlayed = new TimeSpan();
        yearsToPlay = 2;
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
        return playersData.Count > 0;
    }

    #endregion

}
