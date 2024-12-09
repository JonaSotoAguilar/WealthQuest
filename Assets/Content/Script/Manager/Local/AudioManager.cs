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

    public void PlayBackgroundMusic()
    {
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySoundButtonSelect()
    {
        AudioClip clip = buttonSelectClip;
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySoundButtonPress()
    {
        AudioClip clip = buttonPressClip;
        sfxSource.PlayOneShot(clip);
    }
}

