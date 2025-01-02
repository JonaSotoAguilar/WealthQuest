using UnityEngine;
using System;

[System.Serializable]
public class FinishGameData
{
    public int gameID;
    public int years;
    public string timePlayed;
    public string date;
    public string content;
    public int score;

    public FinishGameData(int currentYear, TimeSpan timePlayed, string content, int finalScore)
    {
        gameID = PlayerPrefs.GetInt("gameId", 0) + 1;
        years = currentYear;
        this.timePlayed = timePlayed.ToString(@"hh\:mm\:ss");
        date = DateTime.Now.ToString("dd/MM/yyyy");
        this.content = content;
        this.score = finalScore;
        PlayerPrefs.SetInt("gameId", gameID);
    }

}