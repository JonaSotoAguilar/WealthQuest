using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    private static GameUIManager instance;

    [Space, Header("Data")]
    [SerializeField] private TextMeshProUGUI yearText;

    [Space, Header("HUDs")]
    [SerializeField] private Transform HUDParent;
    [SerializeField] private HUD HUDPrefab;
    private Dictionary<string, HUD> HUDs = new Dictionary<string, HUD>();

    [Space, Header("Actions")]
    [SerializeField] private GameObject navKeyboards;
    [SerializeField] private GameObject navGamepads;
    [SerializeField] private GameObject selectKeyboards;
    [SerializeField] private GameObject selectGamepads;
    [SerializeField] private GameObject throwKeyboards;
    [SerializeField] private GameObject throwGamepads;
    [SerializeField] private GameObject menuKeyboards;
    [SerializeField] private GameObject menuGamepads;
    private bool activeThrow = false;
    private bool activeUI = false;
    private bool isGamepad = false;

    [Space, Header("Banner NextPlayer")]
    [SerializeField] private CharactersDatabase characterDB;
    [SerializeField] private GameObject bannerNextPlayer;
    [SerializeField] private Image characterNextPlayer;
    [SerializeField] private TextMeshProUGUI nicknameNextPlayer;
    private Vector3 offScreenPosition;
    private Vector3 centerPosition;

    [Space, Header("Next Year")]
    [SerializeField] private GameObject nextYearBackground;
    [SerializeField] private GameObject nextYearClouds;
    [SerializeField] private GameObject nextYearClock;
    [SerializeField] private GameObject nextYearSign;

    [Space, Header("Finish Game")]
    [SerializeField] private GameObject finishBackground;
    [SerializeField] private GameObject cloudsPanel;
    [SerializeField] private GameObject wavesPanel;
    [SerializeField] private GameObject finishText;

    [Space, Header("Results")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private GameObject resultParent;
    [SerializeField] private GameObject resultPrefab;
    private List<Result> resultsPlayers = new List<Result>();
    [SerializeField] private Button returnButton;

    #region Getters

    public static List<Transform> HUDsList()
    {
        List<Transform> list = new List<Transform>();
        foreach (var hud in instance.HUDs)
        {
            list.Add(hud.Value.transform);
        }
        return list;
    }

    public static string GetCurrentNickname(string clientID, bool isLocal = true)
    {
        if (isLocal) return GameLocalManager.GetPlayer(clientID).Data.Nickname;
        else return GameNetManager.GetPlayer(clientID).Data.Nickname;
    }

    public static int GetCurrentCharacterID(string clientID, bool isLocal = true)
    {
        if (isLocal) return GameLocalManager.GetPlayer(clientID).Data.CharacterID;
        else return GameNetManager.GetPlayer(clientID).Data.CharacterID;
    }

    #endregion

    #region Initialization

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        SetBannerPlayer();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void SetBannerPlayer()
    {
        // Definir la posición fuera de la pantalla (arriba: 800) y en el centro (Y = 0) usando localPosition
        offScreenPosition = new Vector3(bannerNextPlayer.transform.localPosition.x, 800, bannerNextPlayer.transform.localPosition.z);
        centerPosition = new Vector3(bannerNextPlayer.transform.localPosition.x, 0, bannerNextPlayer.transform.localPosition.z);

        // Asegurar que el banner inicie fuera de la pantalla
        bannerNextPlayer.transform.localPosition = offScreenPosition;
        bannerNextPlayer.SetActive(false);
    }


    private void OnEnable()
    {
        // Escuchar cambios en el dispositivo de entrada
        InputUser.onChange += OnInputDeviceChange;
    }

    private void OnDisable()
    {
        // Dejar de escuchar cambios en el dispositivo de entrada
        InputUser.onChange -= OnInputDeviceChange;
    }

    public static void ShowPanel(bool show)
    {
        if (instance == null) return;
        instance.gameObject.SetActive(show);
    }

    #endregion

    #region Year

    public static void ChangeYear(int newYear)
    {
        if (instance == null) return;

        instance.yearText.text = "Año " + newYear.ToString();
    }

    #endregion

    #region HUD

    public static void InitializeHUD(string clientID, bool isLocal = true)
    {
        if (instance == null) return;

        HUD newHUD = Instantiate(instance.HUDPrefab, instance.HUDParent);
        instance.HUDs.Add(clientID, newHUD);
        newHUD.name = "HUD [" + clientID + "]";

        if (isLocal) newHUD.InitializeUILocal(clientID);
        else newHUD.InitializeUINet(clientID);
    }

    public static HUD GetHUD(string clientID)
    {
        if (instance == null) return null;

        if (!instance.HUDs.ContainsKey(clientID))
        {
            return null;
        }
        return instance.HUDs[clientID];
    }

    public static void ShowsHUDs(bool show)
    {
        instance.HUDParent.gameObject.SetActive(show);
    }

    #endregion

    #region Actions

    public static void ChangeScheme(bool gamepad)
    {
        instance.isGamepad = gamepad;
    }

    private void OnInputDeviceChange(InputUser user, InputUserChange change, InputDevice device)
    {
        if (user == null || device == null || change == InputUserChange.DeviceLost || change == InputUserChange.DeviceUnpaired) return;
        HandleDeviceChange(device);
    }

    private void HandleDeviceChange(InputDevice device)
    {
        if (device is Gamepad)
        {
            ChangeScheme(true);
            if (activeThrow)
            {
                ActiveThrow();
            }
            else if (activeUI)
            {
                ActiveUI();
            }
            ActiveMenu();
        }
        else
        {
            ChangeScheme(false);
            if (activeThrow)
            {
                ActiveThrow();
            }
            else if (activeUI)
            {
                ActiveUI();
            }
            ActiveMenu();
        }
    }


    public static void ActiveThrowActions(bool active)
    {
        instance.activeThrow = active;
        if (active == true) ActiveThrow();
        else ResetActions();
    }

    private static void ActiveThrow()
    {
        if (instance.isGamepad)
        {
            instance.throwKeyboards.SetActive(false);
            instance.throwGamepads.SetActive(true);
        }
        else
        {
            instance.throwGamepads.SetActive(false);
            instance.throwKeyboards.SetActive(true);
        }
    }

    public static void ActiveUIActions(bool active)
    {
        instance.activeUI = active;
        if (active == true) ActiveUI();
        else ResetActions();
    }

    private static void ActiveUI()
    {
        if (instance.isGamepad)
        {
            instance.navKeyboards.SetActive(false);
            instance.selectKeyboards.SetActive(false);
            instance.navGamepads.SetActive(true);
            instance.selectGamepads.SetActive(true);
        }
        else
        {
            instance.navGamepads.SetActive(false);
            instance.selectGamepads.SetActive(false);
            instance.navKeyboards.SetActive(true);
            instance.selectKeyboards.SetActive(true);
        }
    }

    public static void ActiveMenu()
    {
        if (instance.isGamepad)
        {
            instance.menuKeyboards.SetActive(false);
            instance.menuGamepads.SetActive(true);
        }
        else
        {
            instance.menuGamepads.SetActive(false);
            instance.menuKeyboards.SetActive(true);
        }
    }

    private static void ResetActions()
    {
        instance.navKeyboards.SetActive(false);
        instance.navGamepads.SetActive(false);
        instance.selectKeyboards.SetActive(false);
        instance.selectGamepads.SetActive(false);
        instance.throwKeyboards.SetActive(false);
        instance.throwGamepads.SetActive(false);
    }

    #endregion

    #region Next Player

    public static async Task SetPlayerTurn(string clientID, bool animation = true, bool isLocal = true)
    {
        if (animation) await instance.BannerNextPlayer(clientID, isLocal);

        HUD currentHUD = GetHUD(clientID);
        ActiveTurn(clientID, true);

        foreach (Transform child in currentHUD.transform)
        {
            child.position -= new Vector3(0, 15, 0);
        }
    }

    public async Task BannerNextPlayer(string clientID, bool isLocal = true)
    {
        ShowsHUDs(false);

        characterNextPlayer.sprite = characterDB.GetCharacter(GetCurrentCharacterID(clientID, isLocal)).characterIcon;
        nicknameNextPlayer.text = GetCurrentNickname(clientID, isLocal);

        bannerNextPlayer.SetActive(true);
        bannerNextPlayer.transform.localPosition = offScreenPosition;

        AudioManager.PlaySoundAppear();
        await MoveLocalYAsync(bannerNextPlayer, centerPosition.y, 0.6f, LeanTweenType.easeOutBack);

        await Task.Delay(600);
        AudioManager.PlaySoundBannerDisappear();
        await MoveLocalYAsync(bannerNextPlayer, offScreenPosition.y, 0.4f, LeanTweenType.easeInBack);

        bannerNextPlayer.SetActive(false);
        ShowsHUDs(true);
    }

    public static void ActiveTurn(string clientID, bool active)
    {
        if (instance == null) return;

        HUD currentHUD = GetHUD(clientID);
        currentHUD.SetActiveTurn(active);
    }

    public static void ResetPlayerTurn(string clientID)
    {
        HUD currentHUD = GetHUD(clientID);
        foreach (Transform child in currentHUD.transform)
        {
            child.position += new Vector3(0, 15, 0);
        }
        currentHUD.SetActiveTurn(false);
    }

    #endregion

    #region Next Year

    public static async Task ShowNextYear()
    {
        instance.ActiveNextYear(true);
        await instance.AnimateNextYear();
        instance.ActiveNextYear(false);
    }

    private async Task AnimateNextYear()
    {
        // Posicionar el fondo en Y=0
        AudioManager.PlaySoundAppear();
        await MoveLocalYAsync(nextYearBackground, 0, 0.5f, LeanTweenType.easeOutCubic);

        // Mover los otros 3 elementos en paralelo a sus posiciones iniciales
        AudioManager.PlaySoundAppear();
        var cloudsTask = MoveLocalYAsync(nextYearClouds, 0, 0.5f, LeanTweenType.easeOutCubic);
        var clockTask = MoveLocalYAsync(nextYearClock, 540, 0.5f, LeanTweenType.easeOutCubic);
        var signTask = MoveLocalYAsync(nextYearSign, -260, 0.5f, LeanTweenType.easeOutCubic);

        await Task.WhenAll(cloudsTask, clockTask, signTask);

        // Animación de rotación del reloj (entre -15 y 15 grados, repetitiva)
        var clockRotationTween = LeanTween.rotateZ(nextYearClock, -15, 0.5f)
            .setLoopPingPong()
            .setEase(LeanTweenType.easeInOutSine);

        // Animación de movimiento cíclico del letrero (entre Y=180 y Y=150)
        var signBounceTween = LeanTween.moveLocalY(nextYearSign, -280, 0.75f)
            .setLoopPingPong()
            .setEase(LeanTweenType.easeInOutSine);

        // Esperar unos segundos con animaciones activas
        await Task.Delay(2000);

        // Detener animaciones cíclicas
        LeanTween.cancel(nextYearClock);
        LeanTween.cancel(nextYearSign);

        // Animación de salida en paralelo
        AudioManager.PlaySoundBannerDisappear();
        var clockExitTask = MoveLocalYAsync(nextYearClock, 1080, 0.3f, LeanTweenType.easeInCubic);
        var signExitTask = MoveLocalYAsync(nextYearSign, -800, 0.3f, LeanTweenType.easeInCubic);

        await Task.WhenAll(clockExitTask, signExitTask);

        var cloudsExitTask = MoveLocalYAsync(nextYearClouds, 1080, 0.3f, LeanTweenType.easeInCubic);
        var backgroundExitTask = MoveLocalYAsync(nextYearBackground, 1080, 0.3f, LeanTweenType.easeInCubic);

        await Task.WhenAll(cloudsExitTask, clockExitTask, signExitTask, backgroundExitTask);
        nextYearClock.transform.localRotation = Quaternion.Euler(0, 0, 15);
    }

    private void ActiveNextYear(bool active)
    {
        nextYearBackground.SetActive(active);
        nextYearClouds.SetActive(active);
        nextYearClock.SetActive(active);
        nextYearSign.SetActive(active);
    }

    #endregion

    #region Finish Game

    public static async Task ShowFinishGame(bool isLocal = true)
    {
        await instance.AnimateFinishSequence();
        instance.SetResults(isLocal);
    }

    private async Task AnimateFinishSequence()
    {
        // Background animation
        finishBackground.SetActive(true);
        await MoveLocalYAsync(finishBackground, 0, 0.5f, LeanTweenType.easeOutCubic);

        // Clouds and waves animation
        cloudsPanel.SetActive(true);
        wavesPanel.SetActive(true);
        AudioManager.PlaySoundAppear();
        var cloudsTask = MoveLocalYAsync(cloudsPanel, 0, 0.5f, LeanTweenType.easeOutCubic);
        var wavesTask = MoveLocalYAsync(wavesPanel, 0, 0.5f, LeanTweenType.easeOutCubic);
        await Task.WhenAll(cloudsTask, wavesTask);

        // Finish text animation
        finishText.SetActive(true);
        AudioManager.PlaySoundAppear();
        await MoveLocalYAsync(finishText, 0, 0.5f, LeanTweenType.easeOutCubic);

        // Wait for 2 seconds
        await Task.Delay(2000);

        // Move finish text out of the screen
        AudioManager.PlaySoundBannerDisappear();
        await MoveLocalYAsync(finishText, 1080, 0.5f, LeanTweenType.easeInCubic);
    }

    private void SetResults(bool isLocal = true)
    {
        if (isLocal)
        {
            SetResultsLocal();
        }
        else
        {
            SetResultsNet();
        }
    }

    private void SetResultsLocal(bool isLocal = true)
    {
        foreach (var player in GameLocalManager.Players)
        {
            Result newResult = Instantiate(resultPrefab, resultParent.transform).GetComponent<Result>();
            newResult.InitializeResult(player.Data.Nickname, characterDB.GetCharacter(player.Data.CharacterID).characterIcon);
            resultsPlayers.Add(newResult);
        }
        ActiveResultPanel(isLocal);
    }

    private void SetResultsNet(bool isLocal = true)
    {
        foreach (var player in GameNetManager.Players)
        {
            Result newResult = Instantiate(resultPrefab, resultParent.transform).GetComponent<Result>();
            newResult.InitializeResult(player.Data.Nickname, characterDB.GetCharacter(player.Data.CharacterID).characterIcon);
            resultsPlayers.Add(newResult);
        }
        ActiveResultPanel(isLocal);
    }

    private void ActiveResultPanel(bool isLocal = true)
    {
        // Configurar la escala y opacidad inicial del panel
        resultsPanel.transform.localScale = Vector3.zero;
        CanvasGroup canvasGroup = resultsPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = resultsPanel.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0;

        // Activar el panel y animarlo
        resultsPanel.SetActive(true);
        AnimateResultsPanel(canvasGroup, isLocal);
    }

    private void AnimateResultsPanel(CanvasGroup canvasGroup, bool isLocal = true)
    {
        AudioManager.PlaySoundAppear();
        // Animar escala de 0 a 1
        LeanTween.scale(resultsPanel, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBack);

        // Animar opacidad de 0 a 1
        LeanTween.value(resultsPanel, 0, 1, 0.5f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float alpha) =>
            {
                canvasGroup.alpha = alpha;
            })
            .setOnComplete(() =>
            {
                if (isLocal)
                {
                    UpdateResultsLocal();
                }
                else
                {
                    UpdateResultsNet();
                }
            });
    }

    private void UpdateResultsLocal()
    {
        for (int i = 0; i < GameLocalManager.Players.Count; i++)
        {
            // Acceder directamente al resultado y jugador correspondiente por índice
            Result result = resultsPlayers[i];
            var player = GameLocalManager.Players[i];

            // Actualizar el resultado con animación
            result.UpdateResultLocal(player.Data.UID);
        }
        LeanTween.delayedCall(1.5f, () =>
            {
                returnButton.gameObject.SetActive(true);
            });
    }

    private void UpdateResultsNet()
    {
        for (int i = 0; i < GameLocalManager.Players.Count; i++)
        {
            // Acceder directamente al resultado y jugador correspondiente por índice
            Result result = resultsPlayers[i];
            var player = GameLocalManager.Players[i];

            // Actualizar el resultado con animación
            result.UpdateResultLocal(player.Data.UID);
        }
        LeanTween.delayedCall(1.5f, () =>
            {
                returnButton.gameObject.SetActive(true);
            });
    }

    #endregion

    #region Utils

    private Task MoveLocalYAsync(GameObject target, float targetY, float duration, LeanTweenType easeType)
    {
        var tcs = new TaskCompletionSource<bool>();

        LeanTween.moveLocalY(target, targetY, duration)
            .setEase(easeType)
            .setOnComplete(() => tcs.SetResult(true));

        return tcs.Task;
    }

    #endregion

}
