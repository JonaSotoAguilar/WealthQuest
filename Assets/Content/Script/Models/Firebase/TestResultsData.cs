using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class TestResultsData
{
    private string date;
    private int numCorrectAnswers = 0;
    private List<int> answers = new List<int>();

    [FirestoreProperty] public string Date { get => date; set => date = value; }
    [FirestoreProperty] public int NumCorrectAnswers { get => numCorrectAnswers; set => numCorrectAnswers = value; }
    [FirestoreProperty] public List<int> Answers { get => answers; set => answers = value; }

    public TestResultsData() { }
}