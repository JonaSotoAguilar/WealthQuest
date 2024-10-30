using UnityEngine;
using UnityEngine.Networking;

public static class HttpService
{
    public static string userManagement = "http://localhost:3010";

    public static void Get(string endpoint, System.Action<string, bool> callback)
    {
        UnityWebRequest request = UnityWebRequest.Get(userManagement + endpoint);
        UnityWebRequestAsyncOperation operation = request.SendWebRequest();
        operation.completed += _ =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(request.downloadHandler.text, true);
            }
            else
            {
                callback(request.error, false);
            }
        };
    }

    public static void LoginBGamesPlayer(string id)
    {
        string endpoint = $"/players/{id}";
        UnityWebRequest request = UnityWebRequest.Get(userManagement + endpoint);
        UnityWebRequestAsyncOperation operation = request.SendWebRequest();
        operation.completed += _ =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                if (!string.IsNullOrEmpty(response) && response != "No response received")
                {
                    BGamesPlayerList userDataList = JsonUtility.FromJson<BGamesPlayerList>("{\"players\":" + response + "}");
                    if (userDataList.players != null && userDataList.players.Count > 0)
                    {
                        BGamesPlayer data = userDataList.players[0];
                        ProfileUser.SaveBGamesPlayer(data);
                    }

                }
            }
        };
    }
}
