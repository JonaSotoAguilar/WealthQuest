using System.Collections.Generic;
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

    // History
    public static List<FinishGameData> history = new List<FinishGameData>();

    #region Initialization

    public static void LoadProfile(string userId)
    {
        // 1. Cargar perfil de local
        LoadLocalProfile(userId);
        // 2. Sincronizar con perfil de Firebase
        FirebaseManager.Instance.UpdateProfile(userId);
        // 3. Cargar perfil de BGames
        _ = LoadBGamesProfile(PlayerPrefs.GetInt("bGamesId", -1));
        // 4. Cargar historial
        LoadHistory();
    }

    #endregion

    #region Load Profile

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

    public static async Task LoadFirebaseProfile(string userId, string displayName, ProfileData data)
    {
        uid = userId;
        username = displayName;
        level = data.Level;
        xp = data.Xp;
        averageScore = data.AverageScore;
        bestScore = data.BestScore;
        playedGames = data.PlayedGames;
        bGamesProfile = null;
        SaveProfile();

        await LoadBGamesProfile(data.BGamesId);
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

    public static async Task LoadBGamesProfile(int bGamesId)
    {
        if (bGamesId == -1) return;
        bool success = await HttpService.Authenticator(bGamesId);
        if (success) Debug.Log("Perfil de BGames cargado exitosamente.");
        else Debug.Log("Error al cargar perfil de bGames.");
    }

    public static void LoadBGamesPlayer(BGamesProfile bgames)
    {
        if (bgames == null) return;
        bGamesProfile = bgames;
    }

    public static void LogoutBGames()
    {
        PlayerPrefs.DeleteKey("bGamesId");
        bGamesProfile = null;
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

    public static void SaveBGamesPlayer(BGamesProfile bgames)
    {
        if (bgames == null) return;
        bGamesProfile = bgames;
        PlayerPrefs.SetInt("bGamesId", bgames.id);
        PlayerPrefs.Save();
        FirebaseManager.Instance.UpdateProfile(uid);
    }

    public static void UpdateStats(FinishGameData data)
    {
        SaveXPUser(xp + data.score);

        if (playedGames == 0)
        {
            SaveAverageScoreUser(data.score);
        }
        else
        {
            SaveAverageScoreUser((averageScore * playedGames + data.score) / (playedGames + 1));
        }

        SavePlayedGames(playedGames + 1);

        if (bestScore < data.score)
        {
            SaveBestScoreUser(data.score);
        }
        FirebaseManager.Instance.UpdateProfile(uid);
    }

    private static void SaveXPUser(int score)
    {
        xp = xp + score;
        NextLevel(xp);
        PlayerPrefs.SetInt("xpUser", xp);
        PlayerPrefs.Save();
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