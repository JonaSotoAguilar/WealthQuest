using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Soruce")]
    [SerializeField] public AudioSource musicMenuSource;
    [SerializeField] public AudioSource sfxMenuSource;

    [Header("Music")]
    [SerializeField] private AudioClip musicMenuClip;
    [SerializeField] private AudioClip musicGameClip;

    [Header("Audio UI")]
    [SerializeField] private AudioClip buttonSelectClip;
    [SerializeField] private AudioClip buttonPressClip;
    [SerializeField] private AudioClip arrowClip;
    [SerializeField] private AudioClip bannerNextPlayerClip;
    [SerializeField] private AudioClip bannerNextPlayerEndClip;

    [Header("Audio Cards")]
    [SerializeField] private AudioClip correctAnswerClip;
    [SerializeField] private AudioClip wrongAnswerClip;

    [Header("Audio Card")]
    [SerializeField] private AudioClip openCardClip;
    [SerializeField] private AudioClip eventCardClip;
    [SerializeField] private AudioClip expenseCardClip;
    [SerializeField] private AudioClip incomeCardClip;
    [SerializeField] private AudioClip investmentCardClip;

    [Header("Audio Player")]
    [SerializeField] private AudioClip timerClip;
    [SerializeField] private AudioClip diceClip;


    #region Initialization

    private void Awake()
    {
        CreateInstance();
    }

    private void CreateInstance()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadVolume();
        PlayMenuMusic();
    }

    private void LoadVolume()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.25f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        musicMenuSource.ignoreListenerPause = true;
        sfxMenuSource.ignoreListenerPause = true;

        SetupVolumeMusicMenu(musicVolume);
        SetupVolumeSFXMenu(sfxVolume);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Menu")
        {
            StopSoundSFX();
            StopMusic();
            PlayMenuMusic();
        }
        else if (scene.name == "LocalBoard" || scene.name == "OnlineBoard")
        {
            StopSoundSFX();
            StopMusic();
            PlayGameMusic();
        }
        else
        {
            StopSoundSFX();
            StopMusic();
        }
    }

    #endregion

    #region Music

    public static void SetupVolumeMusicMenu(float volume)
    {
        Instance.musicMenuSource.volume = volume;
    }

    private void PlayMenuMusic()
    {
        musicMenuSource.clip = musicMenuClip;
        musicMenuSource.loop = true;
        musicMenuSource.Play();
    }

    private void PlayGameMusic()
    {
        musicMenuSource.clip = musicGameClip;
        musicMenuSource.loop = true;
        musicMenuSource.Play();
    }

    private void StopMusic()
    {
        musicMenuSource.Stop();
    }

    #endregion

    #region SFX

    public static void SetupVolumeSFXMenu(float volume)
    {
        Instance.sfxMenuSource.volume = volume;
    }

    public static void PlaySoundButtonSelect()
    {
        Instance.sfxMenuSource.PlayOneShot(Instance.buttonSelectClip);
    }

    public static void PlaySoundButtonPress()
    {
        Instance.sfxMenuSource.PlayOneShot(Instance.buttonPressClip);
    }

    public static void PlaySoundCorrectAnswer()
    {
        Instance.sfxMenuSource.PlayOneShot(Instance.correctAnswerClip);
    }

    public static void PlaySoundWrongAnswer()
    {
        Instance.sfxMenuSource.PlayOneShot(Instance.wrongAnswerClip);
    }

    public static void PlayOpenCard()
    {
        Instance.sfxMenuSource.PlayOneShot(Instance.openCardClip);
    }

    public static void PlaySoundSquare(SquareType square)
    {
        switch (square)
        {
            case SquareType.Event:
                Instance.sfxMenuSource.PlayOneShot(Instance.eventCardClip);
                break;
            case SquareType.Expense:
                Instance.sfxMenuSource.PlayOneShot(Instance.expenseCardClip);
                break;
            case SquareType.Income:
                Instance.sfxMenuSource.PlayOneShot(Instance.incomeCardClip);
                break;
            case SquareType.Investment:
                Instance.sfxMenuSource.PlayOneShot(Instance.investmentCardClip);
                break;
        }
    }

    public static void PlaySoundArrow()
    {
        Instance.sfxMenuSource.clip = Instance.arrowClip;
        Instance.sfxMenuSource.loop = true;
        Instance.sfxMenuSource.Play();
    }

    public static void PlaySoundTimer()
    {
        //Sonido en bucle
        Instance.sfxMenuSource.clip = Instance.timerClip;
        Instance.sfxMenuSource.loop = true;
        Instance.sfxMenuSource.Play();
    }

    public static void PlaySoundAppear()
    {
        Instance.sfxMenuSource.PlayOneShot(Instance.bannerNextPlayerClip);
    }

    public static void PlaySoundBannerDisappear()
    {
        Instance.sfxMenuSource.PlayOneShot(Instance.bannerNextPlayerEndClip);
    }

    public static void StopSoundSFX()
    {
        Instance.sfxMenuSource.loop = false;
        Instance.sfxMenuSource.Stop();
    }

    #endregion

}

