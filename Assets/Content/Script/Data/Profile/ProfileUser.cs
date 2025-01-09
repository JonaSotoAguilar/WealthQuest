using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public static class ProfileUser
{
    public static string uid;
    public static string username;
    public static int level;
    public static int xp;
    public static int averageScore;
    public static int bestScore;
    public static int playedGames;

    // BGames
    public static BGamesProfile bGamesProfile;
    private static CancellationTokenSource cancellationTokenSource;

    // History
    public static List<FinishGameData> history = new List<FinishGameData>();

    #region Load Profile

    public static void LoadProfile(string userId)
    {
        // 1. Cargar perfil de local
        LoadLocalProfile(userId);
        // 2. Sincronizar con perfil de Firebase
        FirebaseManager.Instance.UpdateProfile(userId);
        // 3. Cargar perfil de BGames
        _ = LoadBGamesProfile();
        // 4. Cargar historial
        LoadHistory();
        // 5. Logeado
        FirebaseManager.Instance.logged = true;
    }

    public static void LoadLocalProfile(string userId)
    {
        uid = userId;
        username = PlayerPrefs.GetString("username");
        level = PlayerPrefs.GetInt("levelUser");
        xp = PlayerPrefs.GetInt("xpUser");
        averageScore = PlayerPrefs.GetInt("averageScoreUser");
        bestScore = PlayerPrefs.GetInt("bestScoreUser");
        playedGames = PlayerPrefs.GetInt("playedGames");
    }

    public static void LoadFirebaseProfile(string userId, string displayName, ProfileData data)
    {
        uid = userId;
        username = displayName;
        level = data.Level;
        xp = data.Xp;
        averageScore = data.AverageScore;
        bestScore = data.BestScore;
        playedGames = data.PlayedGames;
        LogoutBGames();
        SaveProfile();
    }

    private static void SaveProfile()
    {
        PlayerPrefs.SetString("username", username);
        PlayerPrefs.SetInt("levelUser", level);
        PlayerPrefs.SetInt("xpUser", xp);
        PlayerPrefs.SetInt("averageScoreUser", averageScore);
        PlayerPrefs.SetInt("bestScoreUser", bestScore);
        PlayerPrefs.SetInt("playedGames", playedGames);
        PlayerPrefs.Save();
    }

    #endregion

    #region BGames

    public static int GetBGamesID()
    {
        return PlayerPrefs.GetInt("bGamesId", -1);
    }

    public static async Task LoadBGamesProfile()
    {
        int bGamesId = GetBGamesID();
        if (bGamesId == -1) return;
        await HttpService.Authenticator(bGamesId);
        //StartPeriodicUpdates(TimeSpan.FromMinutes(5));
    }

    public static void LoadBGamesPlayer(BGamesProfile bgames)
    {
        if (bgames == null) return;
        bGamesProfile = bgames;
    }

    public static void LoginBGames(BGamesProfile bgames)
    {
        if (bgames == null) return;
        bGamesProfile = bgames;
        PlayerPrefs.SetInt("bGamesId", bgames.id);
        PlayerPrefs.Save();
        //StartPeriodicUpdates(TimeSpan.FromMinutes(5));
    }

    public static void LogoutBGames()
    {
        PlayerPrefs.DeleteKey("bGamesId");
        //StopProfileUpdates();
        bGamesProfile = null;
    }

    private static void StartPeriodicUpdates(TimeSpan interval)
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = cancellationTokenSource.Token;

        _ = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Lógica de actualización periódica
                    await HttpService.Authenticator(GetBGamesID());
                }
                catch (Exception ex)
                {
                    Debug.Log($"Error en la actualización periódica: {ex.Message}");
                }

                // Esperar al siguiente intervalo
                try
                {
                    await Task.Delay(interval, token);
                }
                catch (TaskCanceledException)
                {
                    // Se canceló la tarea
                    break;
                }
            }
        }, token);
    }

    public static void StopProfileUpdates()
    {
        cancellationTokenSource?.Cancel();
    }

    #endregion

    #region Update Profile

    public static void SaveNameUser(string name)
    {
        username = name;
        PlayerPrefs.SetString("username", username);
        PlayerPrefs.Save();
        FirebaseManager.Instance.UpdateProfile(uid);
    }

    public static void UpdateStats(FinishGameData data)
    {
        AddXPUser(xp + data.score);

        if (playedGames == 0)
        {
            UpdateAverageScoreUser(data.score);
        }
        else
        {
            UpdateAverageScoreUser((averageScore * playedGames + data.score) / (playedGames + 1));
        }

        UpdatePlayedGames(playedGames + 1);

        if (bestScore < data.score)
        {
            UpdateBestScoreUser(data.score);
        }
        FirebaseManager.Instance.UpdateProfile(uid);
    }

    private static void AddXPUser(int score)
    {
        UpdateXp(xp + score);
        NextLevel(xp);
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

    public static int XPNextLevel()
    {
        return 100 * (level + 1) * (level + 1);
    }

    public static void UpdateXp(int newXP)
    {
        xp = newXP;
        PlayerPrefs.SetInt("xpUser", xp);
        PlayerPrefs.Save();
    }

    public static void UpdateLevel(int newLevel)
    {
        level = newLevel;
        PlayerPrefs.SetInt("levelUser", level);
        PlayerPrefs.Save();
    }

    public static void UpdateAverageScoreUser(int average)
    {
        averageScore = average;
        PlayerPrefs.SetInt("averageScoreUser", averageScore);
        PlayerPrefs.Save();
    }

    public static void UpdateBestScoreUser(int newBestScore)
    {
        bestScore = newBestScore;
        PlayerPrefs.SetInt("bestScoreUser", bestScore);
        PlayerPrefs.Save();
    }

    public static void UpdatePlayedGames(int played)
    {
        playedGames = played;
        PlayerPrefs.SetInt("playedGames", playedGames);
        PlayerPrefs.Save();
    }

    #endregion

    #region History

    public static async void SaveGame(FinishGameData data, int slotData)
    {
        // 1. Guarda local
        history.Add(data);
        await SaveSystem.SaveHistory(data, slotData);
        // 2. Guarda en Firebase
        await FirebaseManager.Instance.SaveGameHistory(uid, data);
    }

    public static async void LoadHistory()
    {
        // 1. Cargar historial local
        await SaveSystem.LoadHistory();
        // 2. Cargar historial Firebase
        List<FinishGameData> historyFirebase = await FirebaseManager.Instance.LoadGameHistory(uid);
        // 3. Actualizar historiales
        await FirebaseManager.Instance.UpdateHistory(uid, historyFirebase);
    }

    #endregion
}