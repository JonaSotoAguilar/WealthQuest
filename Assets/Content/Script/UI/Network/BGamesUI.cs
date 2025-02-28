using UnityEngine;

public class BGamesUI : MonoBehaviour
{
    public static BGamesUI Instance { get; private set; }

    [Header("bGames")]
    [SerializeField] private GameObject bGamesLoginIcon;

    private static string bGamesLink = "bGames.interaction-lab.info";

    #region bGames

    private void Awake()
    {
        CreateInstance();
        BGamesLogged();
    }

    private void CreateInstance()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void BGamesLogged()
    {
        if (ProfileUser.bGamesProfile != null)
        {
            ActiveBGamesLoginIcon(true);
        }
        else
        {
            ActiveBGamesLoginIcon(false);
        }
    }

    public void ActiveBGamesLoginIcon(bool active)
    {
        CanvasGroup canvasGroup = bGamesLoginIcon.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            return;
        }
        else if (active)
        {
            canvasGroup.alpha = 1;
        }
        else
        {
            canvasGroup.alpha = 0.25f;
        }
    }

    public void OpenBGames()
    {
        Debug.Log("Open bGames");
        Application.OpenURL(bGamesLink);
    }

    #endregion

}