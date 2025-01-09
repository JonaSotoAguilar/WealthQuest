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

    [FirestoreProperty] public int Level { get => level; set => level = value; }
    [FirestoreProperty] public int Xp { get => xp; set => xp = value; }
    [FirestoreProperty] public int AverageScore { get => averageScore; set => averageScore = value; }
    [FirestoreProperty] public int BestScore { get => bestScore; set => bestScore = value; }
    [FirestoreProperty] public int PlayedGames { get => playedGames; set => playedGames = value; }

}