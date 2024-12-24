using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class StartMenu : MonoBehaviour
{
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject contentButton;
    [SerializeField] private GameObject profileButton;
    [SerializeField] private GameObject optionsButton;
    [SerializeField] private GameObject exitButton;

    [Header("Event System")]
    [SerializeField] private EventSystem system;

    [Header("Content")]
    [SerializeField] private Content content;

    #region Initialize

    private async void Awake()
    {
        await ProfileUser.LoadProfile();
        StartCoroutine(content.InitializeContent());
    }

    private void Start()
    {
        GameObject[] buttons = new GameObject[] { playButton, contentButton, profileButton,
            optionsButton, exitButton };
        MenuAnimation.Instance.SubscribeButtonsToEvents(buttons);
    }

    private void OnEnable()
    {
        system.SetSelectedGameObject(playButton);
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

}
