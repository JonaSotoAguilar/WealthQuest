using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[System.Serializable]
[FirestoreData]
public class TestData
{
    private List<QuestionData> questions = new List<QuestionData>();

    [FirestoreProperty] public List<QuestionData> Questions { get => questions; set => questions = value; }

    public TestData() { }
}