using System;
using Firebase.Firestore;
using UnityEngine;

[Serializable, FirestoreData]
public class FinishGameData
{
    public string userId;
    public int gameID;
    public int years;
    public string timePlayed;
    public string date;
    public string content;
    public int score;
    public string grade;

    [FirestoreProperty] public string UserId { get => userId; set => userId = value; }
    [FirestoreProperty] public int GameID { get => gameID; set => gameID = value; }
    [FirestoreProperty] public int Years { get => years; set => years = value; }
    [FirestoreProperty] public string TimePlayed { get => timePlayed; set => timePlayed = value; }
    [FirestoreProperty] public string Date { get => date; set => date = value; }
    [FirestoreProperty] public string Content { get => content; set => content = value; }
    [FirestoreProperty] public int Score { get => score; set => score = value; }
    [FirestoreProperty] public string Grade { get => grade; set => grade = value; }

    public FinishGameData() { }

    public FinishGameData(int currentYear, string timePlayed, string content, int finalScore, int level)
    {
        userId = ProfileUser.uid;
        gameID = PlayerPrefs.GetInt("gameId", 0) + 1;
        years = currentYear;
        this.timePlayed = timePlayed;
        date = DateTime.Now.ToString("dd/MM/yyyy");
        this.content = content;
        this.score = finalScore;
        this.grade = ProfileUser.GetGrade(level);
    }

}