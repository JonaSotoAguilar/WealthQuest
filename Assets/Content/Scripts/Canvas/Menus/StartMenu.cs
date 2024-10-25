using UnityEngine;

public class StartMenu : MonoBehaviour
{
    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
