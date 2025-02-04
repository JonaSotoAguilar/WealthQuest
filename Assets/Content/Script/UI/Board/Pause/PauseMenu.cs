using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using static UnityEngine.InputSystem.InputAction;

public class PauseMenu : MonoBehaviour
{
    private static PauseMenu instance;

    [Header("Game Data")]
    [SerializeField] private GameData gameData;

    [Header("Event System")]
    [SerializeField] private EventSystem eventSystem;

    [Header("Menus")]
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

    [Header("Input System")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction pauseAction;

    // Control Canvas
    private GameObject currentMenu;
    private GameObject currentSelected;
    private CanvasGroup canvasGroupUI;

    #region Initialization

    private void Awake()
    {
        CreateInstance();
        if (gameData.mode != 2) SetActions();
        else DisablePauseAction();
    }

    private void CreateInstance()
    {
        if (instance != null && instance != this)
        {
            instance.DisablePauseAction();
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void SetActions()
    {
        if (pauseAction != null) return;

        pauseAction = inputActions?.FindAction("Pause");
        if (pauseAction != null)
        {
            pauseAction.performed -= OnPauseAction;
            pauseAction.performed += OnPauseAction;
            pauseAction.Enable();
        }
    }

    private void OnDestroy()
    {
        DisablePauseAction();
    }

    private void DisablePauseAction()
    {
        if (pauseAction != null)
        {
            pauseAction.performed -= OnPauseAction;
            pauseAction.Disable();
            pauseAction = null;
        }
    }

    private void OnPauseAction(CallbackContext context)
    {
        if (instance == null)
        {
            return;
        }

        bool activePause = instance.ActivePauseMenu();
        if (activePause)
        {
            instance.CloseCurrentMenu();
            instance.ReturnCurrentSelect();
        }
        else
        {
            instance.SaveCurrentSelect();
            instance.OpenPauseMenu();
        }
    }

    private void SaveCurrentSelect()
    {
        GroupActive(canvasGroupUI, false);
        if (eventSystem.currentSelectedGameObject != null)
        {
            currentSelected = eventSystem.currentSelectedGameObject;
        }
    }

    private void ReturnCurrentSelect()
    {
        GroupActive(canvasGroupUI, true);
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
        if (background == null || pauseMenu == null)
        {
            return;
        }

        background.SetActive(true);
        pauseMenu.SetActive(true);
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
        if (currentMenu != null) currentMenu.SetActive(false);
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

        // Time.timeScale = 1;
    }

    #endregion

    #region Actions Menu

    public void ReturnMainMenu()
    {
        CanvasGroup canvasGroup = confirmMenuPopup.GetComponent<CanvasGroup>();
        GroupActive(canvasGroup, false);
        DisablePauseAction();

        if (gameData.mode != 3)
        {
            SceneManager.LoadScene("Menu");
        }
        else
        {
            StartCoroutine(DisconnectAndReturnToMenu());
        }
    }

    private IEnumerator DisconnectAndReturnToMenu()
    {
        if (RelayService.Instance != null)
        {
            // Inicia la desconexión del cliente
            RelayService.Instance.StopClient();

            // Espera un pequeño tiempo para garantizar la desconexión
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    public void ExitGame()
    {
        CanvasGroup canvasGroup = confirmExitPopup.GetComponent<CanvasGroup>();
        GroupActive(canvasGroup, false);
        DisablePauseAction();
        // Time.timeScale = 1;
        Application.Quit();
    }

    #endregion

    #region Canvas Group

    public static void SetCanvasGroup(CanvasGroup canvasGroup)
    {
        // Activar el CanvasGroup actual, si existe
        if (instance.canvasGroupUI != null)
        {
            instance.GroupActive(instance.canvasGroupUI, true);
        }

        // Asignar el nuevo CanvasGroup
        instance.canvasGroupUI = canvasGroup;

        // Desactivar el CanvasGroup si el menú de pausa está activo
        if (instance.ActivePauseMenu())
        {
            instance.SaveCurrentSelect();
            instance.GroupActive(canvasGroup, false);
        }
    }

    private void GroupActive(CanvasGroup canvasGroup, bool active)
    {
        if (canvasGroup == null) return;
        else if (gameData.mode <= 3) return;
        canvasGroup.interactable = active;
        canvasGroup.blocksRaycasts = active;
    }

    #endregion

}
