using UnityEngine;
using System;
using Firebase.Firestore;

[Serializable, FirestoreData]
public class FinishGameData
{
    public int gameID;
    public int years;
    public string timePlayed;
    public string date;
    public string content;
    public int score;

    [FirestoreProperty] public int GameID { get => gameID; set => gameID = value; }
    [FirestoreProperty] public int Years { get => years; set => years = value; }
    [FirestoreProperty] public string TimePlayed { get => timePlayed; set => timePlayed = value; }
    [FirestoreProperty] public string Date { get => date; set => date = value; }
    [FirestoreProperty] public string Content { get => content; set => content = value; }
    [FirestoreProperty] public int Score { get => score; set => score = value; }

    public FinishGameData() { }

    public FinishGameData(int currentYear, TimeSpan timePlayed, string content, int finalScore)
    {
        gameID = PlayerPrefs.GetInt("gameId", 0) + 1;
        years = currentYear;
        this.timePlayed = timePlayed.ToString(@"hh\:mm\:ss");
        date = DateTime.Now.ToString("dd/MM/yyyy");
        this.content = content;
        this.score = finalScore;
    }

}