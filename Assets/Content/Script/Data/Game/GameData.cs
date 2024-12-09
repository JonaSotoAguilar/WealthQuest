using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.IO;
using System;

[CreateAssetMenu(fileName = "GameData", menuName = "Game/GameData", order = 1)]
public class GameData : ScriptableObject
{
    [Header("Initial Info")]
    public int initialPlayerIndex;
    public int yearsToPlay;

    [Header("Status")]
    public TimeSpan timePlayed;
    public int currentYear;
    public int turnPlayer;
    public List<PlayerData> playersData;

    [Header("Cards & Questions")]
    public string topicName;
    public List<QuestionData> allQuestionList;
    public List<QuestionData> questionList;
    public List<ExpenseCard> expenseCards;
    public List<InvestmentCard> investmentCards;
    public List<IncomeCard> incomeCards;
    public List<EventCard> eventCards;

    #region Getters

    public PlayerData GetPlayerData(string uid)
    {
        return playersData.FirstOrDefault(p => p.UID == uid);
    }

    public Card GetCardByName(string name)
    {
        // Busca en todas las listas de cartas
        Card card = expenseCards.OfType<Card>().FirstOrDefault(c => c.name == name) ??
                        investmentCards.OfType<Card>().FirstOrDefault(c => c.name == name) ??
                        incomeCards.OfType<Card>().FirstOrDefault(c => c.name == name) ??
                        eventCards.OfType<Card>().FirstOrDefault(c => c.name == name);
        return card;
    }

    public bool DataExists()
    {
        return playersData.Count > 0;
    }

    #endregion

    #region Initialization

    public void ClearGameData()
    {
        initialPlayerIndex = 0;
        yearsToPlay = 5;

        timePlayed = new TimeSpan();
        currentYear = 1;
        turnPlayer = 0;
        playersData = new List<PlayerData>();

        topicName = "Default";
        allQuestionList = new List<QuestionData>();
        questionList = new List<QuestionData>();
        expenseCards = new List<ExpenseCard>();
        investmentCards = new List<InvestmentCard>();
        incomeCards = new List<IncomeCard>();
        eventCards = new List<EventCard>();
    }

    #endregion

    #region Asset Bundle

    public IEnumerator LoadCardsAndQuestions(string bundle)
    {
        topicName = bundle;
        string currentBundlePath;
        if (topicName == "Default")
            currentBundlePath = Path.Combine(Application.streamingAssetsPath, "AssetBundles/defaultbundle");
        else
        {
            string assetBundleDirectory = Path.Combine(Application.persistentDataPath, "Topics");
            currentBundlePath = Path.Combine(assetBundleDirectory, topicName);
        }

        yield return LoadDataFromBundle(currentBundlePath);
    }

    private IEnumerator LoadDataFromBundle(string currentBundlePath)
    {
        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(currentBundlePath);
        AssetBundle assetbundle = bundleRequest.assetBundle;

        if (assetbundle == null)
            yield break;
        yield return LoadQuestionsFromBundle(assetbundle);
        yield return LoadCardsFromBundle(assetbundle);
        assetbundle.Unload(false);
    }

    private IEnumerator LoadCardsFromBundle(AssetBundle assetbundle)
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
        if (loadExpenseCardsRequest.allAssets.Length > 0)
            expenseCards.AddRange(loadExpenseCardsRequest.allAssets.OfType<ExpenseCard>());
        else
            expenseCards = loadExpenseCardsRequest.allAssets.OfType<ExpenseCard>().ToList();
    }

    private IEnumerator LoadInvestmentCards(AssetBundle bundle)
    {
        AssetBundleRequest loadInvestmentCardsRequest = bundle.LoadAllAssetsAsync<InvestmentCard>();
        yield return loadInvestmentCardsRequest;
        if (loadInvestmentCardsRequest.allAssets.Length > 0)
            investmentCards.AddRange(loadInvestmentCardsRequest.allAssets.OfType<InvestmentCard>());
        else
            investmentCards = loadInvestmentCardsRequest.allAssets.OfType<InvestmentCard>().ToList();
    }

    private IEnumerator LoadIncomeCards(AssetBundle bundle)
    {
        AssetBundleRequest loadIncomeCardsRequest = bundle.LoadAllAssetsAsync<IncomeCard>();
        yield return loadIncomeCardsRequest;
        if (loadIncomeCardsRequest.allAssets.Length > 0)
            incomeCards.AddRange(loadIncomeCardsRequest.allAssets.OfType<IncomeCard>());
        else
            incomeCards = loadIncomeCardsRequest.allAssets.OfType<IncomeCard>().ToList();
    }

    private IEnumerator LoadEventCards(AssetBundle bundle)
    {
        AssetBundleRequest loadEventCardsRequest = bundle.LoadAllAssetsAsync<EventCard>();
        yield return loadEventCardsRequest;
        if (loadEventCardsRequest.allAssets.Length > 0)
            eventCards.AddRange(loadEventCardsRequest.allAssets.OfType<EventCard>());
        else
            eventCards = loadEventCardsRequest.allAssets.OfType<EventCard>().ToList();
    }

    private IEnumerator LoadQuestionsFromBundle(AssetBundle assetbundle)
    {
        AssetBundleRequest loadJsonRequest = assetbundle.LoadAssetAsync<TextAsset>("Questions");
        yield return loadJsonRequest;
        TextAsset jsonFile = loadJsonRequest.asset as TextAsset;
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

    #region Methods Questions

    public string GetRandomTopicQuestions(int level)
    {
        HashSet<string> topics = new HashSet<string>();

        foreach (QuestionData question in allQuestionList)
        {
            if (question.level <= level)
                topics.Add(question.topic);
        }

        if (topics.Count == 0)
        {
            ResetQuestionsByLevel(level);
            return GetRandomTopicQuestions(level);
        }

        List<string> topicList = new List<string>(topics);
        int randomIndex = UnityEngine.Random.Range(0, topicList.Count);

        return topicList[randomIndex];
    }

    public List<QuestionData> GetQuestionsByTopic(string topic, int level)
    {
        List<QuestionData> questions = questionList.Where(q => q.topic == topic && q.level <= level).ToList();

        if (questions.Count == 0)
        {
            ResetQuestionsByLevel(level);
            return GetQuestionsByTopic(topic, level);
        }

        return questions;
    }

    // FIXME: Eliminar
    public QuestionData GetRandomQuestion()
    {
        if (questionList == null || questionList.Count == 0)
            ResetQuestionList();

        int randomIndex = UnityEngine.Random.Range(0, questionList.Count);
        return questionList[randomIndex];
    }

    public void ResetQuestionsByLevel(int level)
    {
        questionList = new List<QuestionData>(allQuestionList.FindAll(q => q.level <= level));
    }

    public void ResetQuestionList()
    {
        questionList = new List<QuestionData>(allQuestionList);
    }

    public void DeleteQuestion(QuestionData question)
    {
        questionList.Remove(question);
    }

    #endregion

    #region Methods Cards

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

}
