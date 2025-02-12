using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using static UnityEngine.InputSystem.InputAction;

public class PauseMultiMenu : MonoBehaviour
{
    [Header("Event System")]
    [SerializeField] private MultiplayerEventSystem eventSystem;
    [SerializeField] private GameObject rootUI;

    [Header("Menus")]
    [SerializeField] private CanvasGroup canvasGroupUI;
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject optionMenu;
    [SerializeField] private GameObject confirmMenuPopup;
    [SerializeField] private GameObject confirmExitPopup;

    [Header("Buttons")]
    [SerializeField] private GameObject firstPauseButton;
    [SerializeField] private GameObject firstOptionButton;
    [SerializeField] private GameObject firstConfirmMenuButton;
    [SerializeField] private GameObject firstConfirmExitButton;

    // Control Canvas
    private GameObject currentMenu;
    private GameObject currentSelected;


    #region Action


    public void OnPauseAction(CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;
        bool activePause = ActivePauseMenu();
        if (activePause)
        {
            CloseCurrentMenu();
            ReturnCurrentSelect();
        }
        else
        {
            SaveCurrentSelect();
            OpenPauseMenu();
        }
    }

    private void SaveCurrentSelect()
    {
        GroupActive(canvasGroupUI, false);
        eventSystem.playerRoot = gameObject;
        if (eventSystem.currentSelectedGameObject != null)
        {
            currentSelected = eventSystem.currentSelectedGameObject;
        }
    }

    private void ReturnCurrentSelect()
    {
        GroupActive(canvasGroupUI, true);
        eventSystem.playerRoot = rootUI;
        if (currentSelected != null)
        {
            eventSystem.SetSelectedGameObject(currentSelected);
            currentSelected = null;
        }
    }

    #endregion

    #region Menu

    public void OpenPauseMenu()
    {
        background.SetActive(true);
        pauseMenu.SetActive(true);
        eventSystem.SetSelectedGameObject(firstPauseButton);
        currentMenu = pauseMenu;

        // Time.timeScale = 0;
    }

    public void ReturnPauseMenu()
    {
        currentMenu.SetActive(false);
        pauseMenu.SetActive(true);
        eventSystem.SetSelectedGameObject(firstPauseButton);
        currentMenu = pauseMenu;
    }

    public void OpenOptionMenu()
    {
        currentMenu.SetActive(false);
        optionMenu.SetActive(true);
        eventSystem.SetSelectedGameObject(firstOptionButton);
        currentMenu = optionMenu;
    }

    public void OpenConfirmMenuPopup()
    {
        currentMenu.SetActive(false);
        confirmMenuPopup.SetActive(true);
        eventSystem.SetSelectedGameObject(firstConfirmMenuButton);
        currentMenu = confirmMenuPopup;
    }

    public void OpenConfirmExitPopup()
    {
        currentMenu.SetActive(false);
        confirmExitPopup.SetActive(true);
        eventSystem.SetSelectedGameObject(firstConfirmExitButton);
        currentMenu = confirmExitPopup;
    }

    public bool ActivePauseMenu()
    {
        return currentMenu != null;
    }

    public void CloseCurrentMenu()
    {
        background.SetActive(false);
        currentMenu.SetActive(false);
        currentMenu = null;
    }

    #endregion

    #region Actions Menu

    public void ReturnMainMenu()
    {
        CanvasGroup canvasGroup = confirmMenuPopup.GetComponent<CanvasGroup>();
        GroupActive(canvasGroup, false);
        SceneTransition.Instance.LoadScene("Menu");
    }

    public void ExitGame()
    {
        CanvasGroup canvasGroup = confirmExitPopup.GetComponent<CanvasGroup>();
        GroupActive(canvasGroup, false);
        Application.Quit();
    }

    #endregion

    #region Canvas Group

    private void GroupActive(CanvasGroup canvasGroup, bool active)
    {
        canvasGroup.interactable = active;
        canvasGroup.blocksRaycasts = active;
    }

    #endregion

}
