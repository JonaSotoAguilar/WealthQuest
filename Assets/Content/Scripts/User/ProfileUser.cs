using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ProfileUser", menuName = "User/Profile")]
public class ProfileUser : ScriptableObject
{
    [SerializeField] private string nameUser;
    [SerializeField] private int scoreUser;
    [SerializeField] private int bestScoreUser;
    [SerializeField] private int playedGames;
    [SerializeField] private int indexIcon;
    [SerializeField] private int indexCharacter;

    [Header("bGames")]
    //[SerializeField] private string user;
    //[SerializeField] private string password;
    //[SerializeField] private string pointsBGames;

    public string NameUser { get => nameUser; set => nameUser = value; }
    public int ScoreUser { get => scoreUser; set => scoreUser = value; }
    public int BestScoreUser { get => bestScoreUser; set => bestScoreUser = value; }
    public int PlayedGames { get => playedGames; set => playedGames = value; }
    public int IndexIcon { get => indexIcon; set => indexIcon = value; }
    public int IndexCharacter { get => indexCharacter; set => indexCharacter = value; }

    private void OnEnable()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        nameUser = PlayerPrefs.GetString("nameUser");
        scoreUser = PlayerPrefs.GetInt("scoreUser");
        bestScoreUser = PlayerPrefs.GetInt("bestScoreUser");
        playedGames = PlayerPrefs.GetInt("playedGames");
        indexIcon = PlayerPrefs.GetInt("indexIcon");
    }

    public void SaveNameUser(String name)
    {
        nameUser = name;
        PlayerPrefs.SetString("nameUser", nameUser);
    }

    public void SavePointsUser(int score)
    {
        scoreUser = score;
        PlayerPrefs.SetInt("scoreUser", scoreUser);
    }

    public void SaveBestScoreUser(int bestScore)
    {
        bestScoreUser = bestScore;
        PlayerPrefs.SetInt("bestScoreUser", bestScoreUser);
    }

    public void SavePlayedGames(int played)
    {
        playedGames = played;
        PlayerPrefs.SetInt("playedGames", playedGames);
    }

    public void SaveIconUser(int index)
    {
        indexIcon = index;
        PlayerPrefs.SetInt("indexIcon", indexIcon);
    }

    public void SaveCharacterUser(int index)
    {
        indexCharacter = index;
        PlayerPrefs.SetInt("indexCharacter", indexCharacter);
    }
}
