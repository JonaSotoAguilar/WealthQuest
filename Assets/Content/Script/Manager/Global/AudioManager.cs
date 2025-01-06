using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Buttons")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip buttonSelectClip;
    [SerializeField] private AudioClip buttonPressClip;

    [Header("Background Music")]
    [SerializeField] private AudioSource musicSource;

    [Header("Option Menu")]
    [SerializeField] private OptionMenu optionMenu;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        LoadSettings();
        PlayBackgroundMusic();
    }

    private void LoadSettings()
    {
        optionMenu.LoadVolume();
        optionMenu.LoadQuality();
        optionMenu.LoadResolution();
    }

    private void PlayBackgroundMusic()
    {
        musicSource.loop = true;
        musicSource.Play();
    }

    public static void PlaySoundButtonSelect()
    {
        AudioClip clip = Instance.buttonSelectClip;
        Instance.sfxSource.PlayOneShot(clip);
    }

    public static void PlaySoundButtonPress()
    {
        AudioClip clip = Instance.buttonPressClip;
        Instance.sfxSource.PlayOneShot(clip);
    }
}

