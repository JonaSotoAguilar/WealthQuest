using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Fullscreen")]
    [SerializeField] private GameObject activeFullscreen;
    [SerializeField] private GameObject inactiveFullscreen;

    [Header("Resolution")]
    [SerializeField] private TextMeshProUGUI resolutionText;
    [SerializeField] private Resolution[] resolutions;
    private int resolutionIndex;

    [Header("Quality")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    private int qualityIndex;

    #region Volume

    public void LoadVolume()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;

        musicSlider.value = musicVolume;
        musicSlider.onValueChanged.AddListener(SetMusicVolume);

        sfxSlider.value = sfxVolume;
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;

        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;

        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    #endregion

    #region Fullscreen

    private void GetScreen()
    {
        bool isFullscreen = false;
        if (Screen.fullScreen) isFullscreen = true;
        SetFullscreen(isFullscreen);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        if (isFullscreen)
        {
            activeFullscreen.SetActive(true);
            inactiveFullscreen.SetActive(false);
        }
        else
        {
            activeFullscreen.SetActive(false);
            inactiveFullscreen.SetActive(true);
        }
    }

    #endregion

    #region Resolution

    public void LoadResolution()
    {
        resolutions = Screen.resolutions;
        resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", GetCurrentResolutionIndex());
        SetResolution(false);
        UpdateResolutionText();
    }

    private int GetCurrentResolutionIndex()
    {
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                return i;
            }
        }
        return 0;
    }

    public void NextResolution()
    {
        resolutionIndex = (resolutionIndex + 1) % resolutions.Length;
        SetResolution();
    }

    public void PreviousResolution()
    {
        resolutionIndex = (resolutionIndex - 1 + resolutions.Length) % resolutions.Length;
        SetResolution();
    }

    private void SetResolution(bool save = true)
    {
        Resolution selectedResolution = resolutions[resolutionIndex];
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen);
        if (save)
        {
            PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
            PlayerPrefs.Save();
        }
        UpdateResolutionText();
    }
    private void UpdateResolutionText()
    {
        Resolution currentResolution = resolutions[resolutionIndex];
        resolutionText.text = $"{currentResolution.width}x{currentResolution.height}";
    }

    #endregion

    #region Quality

    public void LoadQuality()
    {
        qualityIndex = PlayerPrefs.GetInt("QualityIndex", 3);
        qualityDropdown.value = qualityIndex;
        SetQuality();
    }

    public void SetQuality()
    {
        QualitySettings.SetQualityLevel(qualityDropdown.value);
        PlayerPrefs.SetInt("QualityIndex", qualityDropdown.value);
        PlayerPrefs.Save();
        qualityIndex = qualityDropdown.value;
    }

    #endregion

}
