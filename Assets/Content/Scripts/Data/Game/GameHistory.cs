using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;


[System.Serializable]
public class FinishGameData
{
    public int gameID;
    public int years;
    public TimeSpan timePlayed;
    public string date;
    public string bundleName;
    public int score;
    public List<PlayerData> playersData = new List<PlayerData>();

    public FinishGameData(GameData gameData)
    {
        gameID = PlayerPrefs.GetInt("gameId", 0) + 1;
        years = gameData.CurrentYear;
        timePlayed = gameData.TimePlayed;
        date = DateTime.Now.ToString("dd/MM/yyyy");
        bundleName = gameData.BundleName;
        score = gameData.PlayersData[0].ScoreKFP / years;
        playersData = gameData.PlayersData;
        PlayerPrefs.SetInt("gameId", gameID);
    }
}

[CreateAssetMenu(fileName = "GameHistory", menuName = "Game/GameHistory")]
public class GameHistory : ScriptableObject
{
    public List<FinishGameData> finishGameData = new List<FinishGameData>();

    public void ClearHistory()
    {
        finishGameData.Clear();
    }

    public IEnumerator GetGames()
    {
        yield return SaveSystem.LoadHistory(this);
    }

}
