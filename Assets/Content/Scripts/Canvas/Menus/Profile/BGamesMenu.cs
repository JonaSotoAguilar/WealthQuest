using UnityEngine;
using TMPro;

public class BGamesMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField userNick;
    [SerializeField] private TMP_InputField passText;
    [SerializeField] private TextMeshProUGUI invalidMessage;

    public void AttemptLogin()
    {
        string endpoint = $"/player/{userNick.text}/{passText.text}";
        HttpService.Get(endpoint, HandleTryLoginResponse);
    }

    private void HandleTryLoginResponse(string response, bool success)
    {
        if (success)
        {
            if (!string.IsNullOrEmpty(response) && response != "No response received")
            {
                int id = int.Parse(response);
                FetchPlayerData(id);
            }
            else
            {
                invalidMessage.text = "Login Failed";
            }
        }
        else
        {
            invalidMessage.text = "Login Failed";
        }
    }

    private void FetchPlayerData(int playerId)
    {
        string endpoint = $"/players/{playerId}";
        HttpService.Get(endpoint, HandlePlayerDataResponse);
    }

    private void HandlePlayerDataResponse(string response, bool success)
    {
        if (success)
        {
            BGamesPlayerList userDataList = JsonUtility.FromJson<BGamesPlayerList>("{\"players\":" + response + "}");
            if (userDataList.players != null && userDataList.players.Count > 0)
            {
                BGamesPlayer data = userDataList.players[0];
                invalidMessage.text = "Login Successful!";
                ProfileUser.SaveBGamesPlayer(data);
            }
            else
            {
                invalidMessage.text = "Login Failed";
            }
        }
        else
        {
            invalidMessage.text = "Login Failed";
        }
    }

}