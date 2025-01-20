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
    public static int financeLevel;
    public static string role;

    private static bool testApplied = true;

    // BGames
    public static BGamesProfile bGamesProfile;

    // History
    public static List<FinishGameData> history = new List<FinishGameData>();

    #region Load Profile

    public static void LoadProfile(string userId)
    {
        // 1. Cargar perfil de local
        LoadLocalProfile(userId);
        // 2. Sincronizar con perfil de Firebase
        FirebaseService.Instance.UpdateProfile(userId);
        // 3. Cargar perfil de BGames
        _ = LoadBGamesProfile();
        // 4. Cargar historial
        LoadHistory();
    }

    public static void LoadLocalProfile(string userId)
    {
        uid = userId;
        username = PlayerPrefs.GetString("username", "Player");
        level = PlayerPrefs.GetInt("levelUser", 1);
        xp = PlayerPrefs.GetInt("xpUser", 0);
        averageScore = PlayerPrefs.GetInt("averageScoreUser", 0);
        bestScore = PlayerPrefs.GetInt("bestScoreUser", 0);
        playedGames = PlayerPrefs.GetInt("playedGames", 0);
        financeLevel = PlayerPrefs.GetInt("financeLevel", 1);
        role = PlayerPrefs.GetString("role", "Player");

        testApplied = PlayerPrefs.GetInt("testApplied", 1) == 1;
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
        financeLevel = data.FinanceLevel;
        role = data.Role;
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
        PlayerPrefs.SetInt("financeLevel", financeLevel);
        PlayerPrefs.SetString("role", role);
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
        FirebaseService.Instance.UpdateProfile(uid);
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

        RefreshTest();
        PlayerPrefs.Save();
        FirebaseService.Instance.UpdateProfile(uid);
    }

    private static void AddXPUser(int score)
    {
        int newXP = xp + score;
        NextLevel(newXP);
    }

    private static void NextLevel(int newXP)
    {
        int xpNextLevel = XPNextLevel();
        if (newXP >= xpNextLevel)
        {
            level++;
            newXP -= xpNextLevel;
            PlayerPrefs.SetInt("levelUser", level);
        }
        UpdateXp(newXP);
    }

    public static int XPNextLevel()
    {
        return 100 * (level + 1) * (level + 1);
    }

    public static void UpdateXp(int newXP)
    {
        xp = newXP;
        PlayerPrefs.SetInt("xpUser", xp);
    }

    public static void UpdateLevel(int newLevel)
    {
        level = newLevel;
        PlayerPrefs.SetInt("levelUser", level);
    }

    public static void UpdateAverageScoreUser(int average)
    {
        averageScore = average;
        PlayerPrefs.SetInt("averageScoreUser", averageScore);
    }

    public static void UpdateBestScoreUser(int newBestScore)
    {
        bestScore = newBestScore;
        PlayerPrefs.SetInt("bestScoreUser", bestScore);
    }

    public static void UpdatePlayedGames(int played)
    {
        playedGames = played;
        PlayerPrefs.SetInt("playedGames", playedGames);
    }

    public static void UpdateFinanceLevel(int newFinanceLevel)
    {
        financeLevel = newFinanceLevel;
        PlayerPrefs.SetInt("financeLevel", financeLevel);
    }

    public static void UpdateRole(RoleType newRole)
    {
        role = newRole.ToString();
        PlayerPrefs.SetString("role", role);
    }

    #endregion

    #region History

    public static async void SaveGame(FinishGameData data, int slotData)
    {
        // 1. Guarda local
        history.Add(data);
        await SaveService.SaveHistory(data, slotData);
        // 2. Guarda en Firebase
        await FirebaseService.Instance.SaveGameHistory(uid, data);
    }

    public static async void LoadHistory()
    {
        // 1. Cargar historial local
        await SaveService.LoadHistory();
        // 2. Cargar historial Firebase
        List<FinishGameData> historyFirebase = await FirebaseService.Instance.LoadGameHistory(uid);
        // 3. Actualizar historiales
        await FirebaseService.Instance.UpdateHistory(uid, historyFirebase);
    }

    #endregion

    #region Test

    public static bool ApplyTest()
    {
        return testApplied;
    }

    private static void RefreshTest()
    {
        // Se aplica cada 3 partidas sino es nivel 3 (máximo)
        if (financeLevel >= 3 || playedGames % 3 != 0)
        {
            UpdateTestApplied(false);
        }
        else
        {
            UpdateTestApplied(true);
        }
    }

    public static void UpdateTestApplied(bool applied)
    {
        testApplied = applied;
        PlayerPrefs.SetInt("testApplied", testApplied ? 1 : 0);
        PlayerPrefs.Save();
    }

    #endregion
}