using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mirror;
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

    public static void InitializeHUD(string clientID, bool isLocal = true)
    {
        if (instance == null) return;

        HUD newHUD = Instantiate(instance.HUDPrefab, instance.HUDParent);
        instance.HUDs.Add(clientID, newHUD);
        newHUD.name = "HUD [" + clientID + "]";

        if (isLocal) newHUD.InitializeUILocal(clientID);
        else newHUD.InitializeUINet(clientID);
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

    #region NextPlayer

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

        AudioManager.PlaySoundBannerNextPlayer();
        await MoveBanner(bannerNextPlayer, centerPosition.y, 0.4f, LeanTweenType.easeOutBack);

        await Task.Delay(600);
        AudioManager.PlaySoundBannerNextPlayerEnd();
        await MoveBanner(bannerNextPlayer, offScreenPosition.y, 0.4f, LeanTweenType.easeInBack);

        bannerNextPlayer.SetActive(false);
        ShowsHUDs(true);
    }

    // Modificar la función MoveBanner para usar localPosition
    private Task MoveBanner(GameObject banner, float targetY, float duration, LeanTweenType easeType)
    {
        var tcs = new TaskCompletionSource<bool>();

        LeanTween.moveLocalY(banner, targetY, duration)
            .setEase(easeType)
            .setOnComplete(() => tcs.SetResult(true));

        return tcs.Task;
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

}
