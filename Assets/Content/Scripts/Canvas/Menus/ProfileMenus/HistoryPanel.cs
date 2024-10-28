using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class HistoryPanel : MonoBehaviour
{
    [SerializeField] private GameHistory gameHistory;
    [SerializeField] private GameObject gamePrefab;
    [SerializeField] private Transform container;

    private void OnEnable()
    {
        StartCoroutine(CreateGamePanel());
    }

    // Crea un panel para cada tópico
    private IEnumerator CreateGamePanel()
    {
        yield return gameHistory.GetGames();
        List<FinishGameData> finishGameData = gameHistory.finishGameData;
        foreach (FinishGameData game in finishGameData)
        {
            GameObject newPanel = Instantiate(gamePrefab, container);
            GameObject statsPanel = newPanel.transform.Find("Stats").gameObject;

            statsPanel.transform.Find("Years_Text").GetComponent<TextMeshProUGUI>().text = game.years.ToString();
            statsPanel.transform.Find("TimePlayed_Text").GetComponent<TextMeshProUGUI>().text = game.timePlayed.ToString();
            statsPanel.transform.Find("Topic_Text").GetComponent<TextMeshProUGUI>().text = game.bundleName;
            statsPanel.transform.Find("Score_Text").GetComponent<TextMeshProUGUI>().text = game.score.ToString();
            statsPanel.transform.Find("Date_Text").GetComponent<TextMeshProUGUI>().text = game.date;

            newPanel.SetActive(true);
        }
    }

    // Limpiar los paneles existentes del ScrollView
    public void ClearScrollView()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}
