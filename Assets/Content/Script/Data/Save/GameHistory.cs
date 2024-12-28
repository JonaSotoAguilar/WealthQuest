using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

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
