using System;
using UnityEngine;


[System.Serializable]
public static class ProfileUser
{
    private static string nameUser;
    private static int xpUser;
    private static int scoreAverageUser;
    private static int bestScoreUser;
    private static int playedGames;
    private static int indexCharacter;

    [Header("bGames")]
    //[SerializeField] private string user;
    //[SerializeField] private string password;
    //[SerializeField] private string pointsBGames;

    public static string NameUser { get => nameUser; }
    public static int XPUser { get => xpUser; }
    public static int ScoreAverageUser { get => scoreAverageUser; }
    public static int BestScoreUser { get => bestScoreUser; }
    public static int PlayedGames { get => playedGames; }
    public static int IndexCharacter { get => indexCharacter; }

    public static void AddGameData(FinishGameData data)
    {
        PlayerData user = data.playersData[0];
        SaveXPUser(xpUser + data.score);
        SaveAverageScoreUser((scoreAverageUser * playedGames + data.score) / playedGames + 1);
        SavePlayedGames(playedGames + 1);
        if (bestScoreUser < data.score) SaveBestScoreUser(data.score);
    }

    public static void LoadSettings()
    {
        nameUser = PlayerPrefs.GetString("nameUser", "Jugador");
        xpUser = PlayerPrefs.GetInt("xpUser", 0);
        bestScoreUser = PlayerPrefs.GetInt("bestScoreUser", 0);
        playedGames = PlayerPrefs.GetInt("playedGames", 0);
    }

    public static void SaveNameUser(String name)
    {
        nameUser = name;
        PlayerPrefs.SetString("nameUser", nameUser);
    }

    public static void SaveXPUser(int score)
    {
        xpUser = score;
        PlayerPrefs.SetInt("scoreUser", xpUser);
    }

    public static void SaveAverageScoreUser(int average)
    {
        scoreAverageUser = average;
        PlayerPrefs.SetInt("scoreAverageUser", scoreAverageUser);
    }

    public static void SaveBestScoreUser(int bestScore)
    {
        bestScoreUser = bestScore;
        PlayerPrefs.SetInt("bestScoreUser", bestScoreUser);
    }

    public static void SavePlayedGames(int played)
    {
        playedGames = played;
        PlayerPrefs.SetInt("playedGames", playedGames);
    }

    public static void SaveCharacterUser(int index)
    {
        indexCharacter = index;
        PlayerPrefs.SetInt("indexCharacter", indexCharacter);
    }
}
