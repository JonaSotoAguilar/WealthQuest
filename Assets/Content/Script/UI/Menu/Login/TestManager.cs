using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestManager : MonoBehaviour
{
    [Header("Test")]
    [SerializeField] private GameObject questionTestPrefab;
    [SerializeField] private Transform container;
    [SerializeField] private List<TestQuestion> testQuestions = new List<TestQuestion>();
    [SerializeField] private Button sendButton;

    private int questionsAnswered = 0;

    private void OnEnable()
    {
        sendButton.interactable = false;
        GetTest();
    }

    private void OnDisable()
    {
        ClearTest();
    }

    private async void GetTest()
    {
        TestData test = await FirebaseService.Instance.GetRandomTestAsync();
        SetTest(test);
    }

    private void SetTest(TestData test)
    {
        foreach (QuestionData question in test.Questions)
        {
            int index = test.Questions.IndexOf(question);
            TestQuestion testQuestion = Instantiate(questionTestPrefab, container).GetComponent<TestQuestion>();

            // Suscribirse al evento OnAnswered
            testQuestion.OnAnswered += OnQuestionAnswered;

            testQuestion.SetQuestion(question, index);
            testQuestions.Add(testQuestion);
        }
    }
    private void ClearTest()
    {
        foreach (TestQuestion testQuestion in testQuestions)
        {
            Destroy(testQuestion.gameObject);
        }
        testQuestions.Clear();
    }

    private void OnQuestionAnswered()
    {
        // Incrementar el contador y verificar si todas las preguntas están respondidas
        questionsAnswered++;

        if (questionsAnswered == testQuestions.Count)
        {
            sendButton.interactable = true;
        }
    }

    public void SendTest()
    {
        sendButton.interactable = false;
        TestResultsData testResults = new TestResultsData();
        foreach (TestQuestion testQuestion in testQuestions)
        {
            testResults.Answers.Add(testQuestion.answerIndex);
            if (testQuestion.CheckAnswer()) testResults.NumCorrectAnswers++;
        }
        int newLevelFinace = CalculateLevel(testResults.NumCorrectAnswers);
        if (newLevelFinace > ProfileUser.financeLevel)
        {
            ProfileUser.UpdateFinanceLevel(newLevelFinace);
            FirebaseService.Instance.UpdateProfile(ProfileUser.uid);
        }

        DateTime date = DateTime.Now;
        testResults.Date = date.ToString("dd/MM/yyyy HH:mm:ss");
        FirebaseService.Instance.SaveTestResults(testResults);
        MenuManager.Instance.OpenStartMenu();
    }

    public int CalculateLevel(int correctAnswers)
    {
        if (correctAnswers <= 3)
            return 1;
        else if (correctAnswers <= 4)
            return 2;
        else if (correctAnswers <= 5)
            return 3;
        else
            return 4;
    }

}
