using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Soruce")]
    [SerializeField] public AudioSource musicMenuSource;
    [SerializeField] public AudioSource sfxMenuSource;

    [Header("Audio Buttons")]
    [SerializeField] private AudioClip musicMenuClip;
    [SerializeField] private AudioClip buttonSelectClip;
    [SerializeField] private AudioClip buttonPressClip;

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
            PlayMenuMusic();
        }
        else
        {
            StopMenuMusic();
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

    private void StopMenuMusic()
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
        AudioClip clip = Instance.buttonSelectClip;
        Instance.sfxMenuSource.PlayOneShot(clip);
    }

    public static void PlaySoundButtonPress()
    {
        AudioClip clip = Instance.buttonPressClip;
        Instance.sfxMenuSource.PlayOneShot(clip);
    }

    #endregion
}

