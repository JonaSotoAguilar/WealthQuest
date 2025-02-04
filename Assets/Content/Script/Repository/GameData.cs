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
    public int mode;

    [Space, Header("Status")]
    public int currentYear;
    public int turnPlayer;
    public string timePlayed;
    public List<PlayerData> playersData;

    [Space, Header("Content")]
    public string content;
    public List<Question> allQuestionList;
    public List<Question> questionList;
    public List<ExpenseCard> expenseCards;
    public List<InvestmentCard> investmentCards;
    public List<IncomeCard> incomeCards;
    public List<EventCard> eventCards;

    #region Getters

    public PlayerData GetPlayerData(string uid)
    {
        return playersData.FirstOrDefault(p => p.UID == uid);
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
        yearsToPlay = 2;

        timePlayed = "00:00:00";
        currentYear = 1;
        turnPlayer = 0;
        playersData = new List<PlayerData>();

        content = SaveService.defaultContentName;
        allQuestionList = new List<Question>();
        questionList = new List<Question>();
        expenseCards = new List<ExpenseCard>();
        investmentCards = new List<InvestmentCard>();
        incomeCards = new List<IncomeCard>();
        eventCards = new List<EventCard>();
    }

    public void LoadContent(string newContent)
    {
        content = newContent;
        LoadCards();
        LoadQuestions(content);
    }

    #endregion

    # region Players

    public void SavePlayer(string uid, string username, int model)
    {
        PlayerData data = new PlayerData(uid, username, model, ProfileUser.financeLevel);
        playersData.Add(data);
    }

    #endregion

    #region Methods Cards

    private void LoadCards()
    {
        LoadExpenseCards();
        LoadInvestmentCards();
        LoadIncomeCards();
        LoadEventCards();
    }

    private void LoadExpenseCards()
    {
        ExpenseCard[] loadedCards = Resources.LoadAll<ExpenseCard>("Card/Expense");

        if (loadedCards.Length > 0)
        {
            expenseCards.Clear();
            expenseCards.AddRange(loadedCards);
        }
    }

    private void LoadInvestmentCards()
    {
        InvestmentCard[] loadedCards = Resources.LoadAll<InvestmentCard>("Card/Investment");

        if (loadedCards.Length > 0)
        {
            investmentCards.Clear();
            investmentCards.AddRange(loadedCards);
        }
    }

    public void LoadIncomeCards()
    {
        IncomeCard[] loadedCards = Resources.LoadAll<IncomeCard>("Card/Income");

        if (loadedCards.Length > 0)
        {
            incomeCards.Clear();
            incomeCards.AddRange(loadedCards);
        }
    }

    private void LoadEventCards()
    {
        EventCard[] loadedCards = Resources.LoadAll<EventCard>("Card/Event");

        if (loadedCards.Length > 0)
        {
            eventCards.Clear();
            eventCards.AddRange(loadedCards);
        }
    }

    private void LoadQuestions(string content)
    {
        List<Question> questionList = ContentDatabase.GetContent(content).questions;

        if (questionList != null && questionList.Count > 0)
        {
            allQuestionList = new List<Question>(questionList);
            ResetQuestionList();
        }
    }

    #endregion

    #region Methods Questions

    public string GetRandomTopicQuestions(int level)
    {
        HashSet<string> topics = new HashSet<string>();

        foreach (Question question in allQuestionList)
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

    public List<Question> GetQuestionsByLevel(int level)
    {
        // Intentar obtener preguntas desde el nivel más alto hacia abajo
        for (int currentLevel = level; currentLevel >= 1; currentLevel--)
        {
            // Filtrar preguntas por tema y nivel actual
            List<Question> questions = questionList.Where(q => q.level == currentLevel).ToList();

            // Si se encuentran preguntas, devolverlas
            if (questions.Count > 0)
            {
                return questions;
            }
        }

        // Si no hay preguntas en ningún nivel, restaurar niveles y volver a intentarlo
        ResetQuestionsByLevel(level);
        return GetQuestionsByLevel(level);
    }

    public void ResetQuestionsByLevel(int level)
    {
        // Recuperar preguntas agotadas para el nivel especificado y los niveles inferiores
        var resetLevels = Enumerable.Range(1, level);
        foreach (var lvl in resetLevels)
        {
            // Restaurar las preguntas de ese nivel (implementación específica)
            RestoreQuestionsForLevel(lvl);
        }
    }


    public void RestoreQuestionsForLevel(int level)
    {
        questionList = new List<Question>(allQuestionList.FindAll(q => q.level <= level));
    }

    public void ResetQuestionList()
    {
        questionList = new List<Question>(allQuestionList);
    }

    public void DeleteQuestion(Question question)
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
