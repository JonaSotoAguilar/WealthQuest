using System;
using System.Collections.Generic;
using Mirror.Examples.Basic;
using UnityEngine;

[System.Serializable]
public static class ProfileUser
{
    private static string uid;
    private static string username;
    private static int level;
    private static int xp;
    private static int averageScore;
    private static int bestScore;
    private static int playedGames;

    [Header("bGames")]
    private static BGamesPlayer bGamesPlayer;
    public static List<BGamesAttributes> bGamesAttributes;

    #region Methods Getters & Setters

    public static string UID { get => uid; }
    public static string Username { get => username; }
    public static int Level { get => level; }
    public static int XP { get => xp; }
    public static int AverageScore { get => averageScore; }
    public static int BestScore { get => bestScore; }
    public static int PlayedGames { get => playedGames; }
    public static BGamesPlayer BGamesPlayer { get => bGamesPlayer; }

    #endregion

    #region Methods Load Profile

    public static void LoadProfile()
    {
        uid = PlayerPrefs.GetString("uid", GenerateUID());
        username = PlayerPrefs.GetString("username", "Jugador");
        level = PlayerPrefs.GetInt("levelUser", 1);
        xp = PlayerPrefs.GetInt("xpUser", 0);
        averageScore = PlayerPrefs.GetInt("averageScoreUser", 0);
        bestScore = PlayerPrefs.GetInt("bestScoreUser", 0);
        playedGames = PlayerPrefs.GetInt("playedGames", 0);

        int id = PlayerPrefs.GetInt("bGamesId", -1);
        if (id != -1) HttpService.LoginBGamesPlayer($"/players/{id}");
    }

    private static string GenerateUID()
    {
        string uid = Guid.NewGuid().ToString();
        PlayerPrefs.SetString("uid", uid);
        PlayerPrefs.Save();
        return uid;
    }

    #endregion

    #region Methods Update Profile

    public static void SaveNameUser(String name)
    {
        username = name;
        PlayerPrefs.SetString("username", username);
        PlayerPrefs.Save();
    }

    public static void SaveBGamesPlayer(BGamesPlayer player)
    {
        bGamesPlayer = player;
        if (player == null)
        {
            PlayerPrefs.SetInt("bGamesId", -1);
            return;
        };
        PlayerPrefs.SetInt("bGamesId", player.id_players);
        PlayerPrefs.Save();
    }

    public static void LoadBGamesPlayer(BGamesPlayer player)
    {
        bGamesPlayer = player;
    }

    // FIXME: Revisar guardar atributos termino partida
    public static void UpdateStats(FinishGameData data)
    {
        SaveXPUser(xp + data.score);
        SaveAverageScoreUser((averageScore * playedGames + data.score) / playedGames + 1);
        SavePlayedGames(playedGames + 1);
        if (bestScore < data.score) SaveBestScoreUser(data.score);
    }

    private static void SaveXPUser(int score)
    {
        xp = score;
        NextLevel(xp);
        PlayerPrefs.SetInt("scoreUser", xp);
        PlayerPrefs.Save();
    }

    public static int XPNextLevel()
    {
        return 100 * (level + 1) * (level + 1);
    }

    private static void NextLevel(int xp)
    {
        int xpNextLevel = XPNextLevel();
        if (xp >= xpNextLevel)
        {
            level++;
            PlayerPrefs.SetInt("levelUser", level);
            PlayerPrefs.Save();
        }
    }

    private static void SaveAverageScoreUser(int average)
    {
        averageScore = average;
        PlayerPrefs.SetInt("averageScoreUser", averageScore);
        PlayerPrefs.Save();
    }

    private static void SaveBestScoreUser(int newBestScore)
    {
        bestScore = newBestScore;
        PlayerPrefs.SetInt("bestScoreUser", bestScore);
        PlayerPrefs.Save();
    }

    private static void SavePlayedGames(int played)
    {
        playedGames = played;
        PlayerPrefs.SetInt("playedGames", playedGames);
        PlayerPrefs.Save();
    }

    #endregion
}