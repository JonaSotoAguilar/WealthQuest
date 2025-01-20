using System;
using Firebase.Firestore;

[FirestoreData]
public class ProfileData
{
    private int level = 1;
    private int xp = 0;
    private int averageScore = 0;
    private int bestScore = 0;
    private int playedGames = 0;
    private int financeLevel = 1;
    private string birthDate = "";
    private string gender = "";
    private int age = 1;
    private string role = "player";

    [FirestoreProperty] public int Level { get => level; set => level = value; }
    [FirestoreProperty] public int Xp { get => xp; set => xp = value; }
    [FirestoreProperty] public int AverageScore { get => averageScore; set => averageScore = value; }
    [FirestoreProperty] public int BestScore { get => bestScore; set => bestScore = value; }
    [FirestoreProperty] public int PlayedGames { get => playedGames; set => playedGames = value; }
    [FirestoreProperty] public int FinanceLevel { get => financeLevel; set => financeLevel = value; }
    [FirestoreProperty] public string BirthDate { get => birthDate; set => birthDate = value; }
    [FirestoreProperty] public string Gender { get => gender; set => gender = value; }
    [FirestoreProperty] public int Age { get => age; set => age = value; }
    [FirestoreProperty] public string Role { get => role; set => role = value; }

    public ProfileData() { }

    public ProfileData(string birthDate, string gender, int age, int role = 0)
    {
        this.birthDate = birthDate;
        this.gender = gender;
        this.age = age;
        this.role = Enum.GetName(typeof(RoleType), role) ?? RoleType.Player.ToString();
    }
}