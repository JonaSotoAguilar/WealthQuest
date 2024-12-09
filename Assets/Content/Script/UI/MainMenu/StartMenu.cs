using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.EventSystems;

public class StartMenu : MonoBehaviour
{
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject contentButton;
    [SerializeField] private GameObject profileButton;
    [SerializeField] private GameObject optionsButton;
    [SerializeField] private GameObject exitButton;
    [SerializeField] private TextMeshProUGUI titleText;

    private GameObject selectedButton;

    #region Initialize

    private void Awake()
    {
        AnimateTitle();
        GameObject[] buttons = new GameObject[] { playButton, contentButton, profileButton, optionsButton, exitButton };

        MenuAnimation.Instance.SubscribeButtonsToEvents(buttons);
        selectedButton = playButton;
    }

    private void OnEnable()
    {
        MenuAnimation.Instance.SelectObject(selectedButton);
    }

    private void OnDisable()
    {
        selectedButton = MenuAnimation.Instance.SelectedButton;
    }

    public void ShowPanel(bool show)
    {
        gameObject.SetActive(show);
    }

    #endregion

    #region Methods Menu

    public void ExitGame()
    {
        Application.Quit();
    }

    #endregion

    #region Animation Title

    private void AnimateTitle()
    {
        LeanTween.scale(titleText.gameObject, new Vector3(1.1f, 1.1f, 1.1f), 1.0f).setLoopPingPong();
    }

    #endregion

}
