using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Result : MonoBehaviour
{
    private static CultureInfo chileanCulture = new CultureInfo("es-CL");

    [Header("Winner")]
    [SerializeField] private GameObject winnerIcon;
    [SerializeField] private TextMeshProUGUI position;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI nickname;
    [SerializeField] private TextMeshProUGUI points;
    [SerializeField] private TextMeshProUGUI money;
    [SerializeField] private TextMeshProUGUI finalScore;
    [SerializeField] private TextMeshProUGUI grade;

    [Header("Character")]
    [SerializeField] private Image characterSprite;

    public void InitializeResult(string nickname, Sprite icon)
    {
        this.nickname.text = nickname;
        characterSprite.sprite = icon;

        //Default values
        points.text = "0";
        money.text = Mathf.RoundToInt(0).ToString("C0", chileanCulture);
        finalScore.text = "0";
        grade.text = " ";

        position.gameObject.SetActive(false);
        winnerIcon.SetActive(false);
    }

    public void UpdateResultLocal(string clientID)
    {
        var data = GameLocalManager.GetPlayer(clientID).Data;

        UpdatePoints(data.Points);
        UpdateMoney(data.Money);
        UpdateScore(data.FinalScore);
        UpdateGrade(data.Level);

        UpdatePosition(data.ResultPosition);
        SetWinner(data.ResultPosition);
    }

    public void UpdateResultNet(string clientID)
    {
        var data = GameNetManager.GetPlayer(clientID).Data;

        UpdatePoints(data.Points);
        UpdateMoney(data.Money);
        UpdateScore(data.FinalScore);
        UpdateGrade(data.Level);

        UpdatePosition(data.ResultPosition);
        SetWinner(data.ResultPosition);
    }

    public void UpdatePoints(int newPoints)
    {
        int oldPoints = int.Parse(points.text);
        LeanTween.value(oldPoints, newPoints, 1.5f).setOnUpdate((float val) =>
        {
            points.text = Mathf.RoundToInt(val).ToString();
        }).setEaseOutQuad();
    }

    public void UpdateMoney(int newMoney)
    {
        int oldMoney = int.Parse(money.text, NumberStyles.Currency, chileanCulture);
        LeanTween.value(oldMoney, newMoney, 1.5f).setOnUpdate((float val) =>
        {
            money.text = Mathf.RoundToInt(val).ToString("C0", chileanCulture);
        }).setEaseOutQuad();
    }

    public void UpdateScore(int newScore)
    {
        int oldScore = int.Parse(finalScore.text);
        LeanTween.value(oldScore, newScore, 1.5f).setOnUpdate((float val) =>
        {
            finalScore.text = Mathf.RoundToInt(val).ToString();
        }).setEaseOutQuad();
    }

    public void UpdateGrade(int level)
    {
        string newGrade = GetGrade(level);
        LeanTween.value(0, 1, 1.5f).setOnUpdate((float val) =>
        {
            grade.text = newGrade;
        }).setEaseOutQuad();
    }

    public void UpdatePosition(int pos)
    {
        string posText = pos switch
        {
            1 => "1ro",
            2 => "2do",
            3 => "3ro",
            4 => "4to",
            _ => "1ro"
        };
        position.text = posText;
        position.gameObject.SetActive(true);
        LeanTween.scale(position.gameObject, Vector3.one, 1.5f).setEaseOutBounce();
    }

    public void SetWinner(int resultPosition)
    {
        bool active = resultPosition == 1;
        winnerIcon.SetActive(active);
    }

    private string GetGrade(int level)
    {
        switch (level)
        {
            case 1:
                return "Principiante";
            case 2:
                return "Intermedio Bajo";
            case 3:
                return "Intermedio Alto";
            case 4:
                return "Avanzado";
            default:
                return "Principiante";
        }
    }

}