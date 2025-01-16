using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionMenu : MonoBehaviour
{
    [Header("Audio")]
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

    #region Initialization

    private void Awake()
    {
        LoadSettings();
    }

    public void LoadSettings()
    {
        LoadVolume();
        LoadQuality();
        LoadResolution();
        GetScreen();
    }

    #endregion

    #region Volume

    public void LoadVolume()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        musicSlider.value = musicVolume;
        musicSlider.onValueChanged.AddListener(SetMusicVolume);

        sfxSlider.value = sfxVolume;
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float volume)
    {
        AudioManager.SetupVolumeMusicMenu(volume);

        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        AudioManager.SetupVolumeSFXMenu(volume);

        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    #endregion

    #region Fullscreen

    private void GetScreen()
    {
        bool isFullscreen = Screen.fullScreen;
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
        // Filtrar resoluciones para incluir solo las de 1920x1080 o superiores
        resolutions = FilterResolutions(Screen.resolutions, 1920, 1080);

        resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", GetCurrentResolutionIndex());
        SetResolution(false); 
        UpdateResolutionText();
    }

    private Resolution[] FilterResolutions(Resolution[] allResolutions, int minWidth, int minHeight)
    {
        List<Resolution> filtered = new List<Resolution>();

        foreach (Resolution res in allResolutions)
        {
            if (res.width >= minWidth && res.height >= minHeight)
            {
                filtered.Add(res);
            }
        }

        return filtered.ToArray();
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

        // Si no se encuentra una coincidencia, usar la última resolución (más alta disponible)
        return resolutions.Length - 1;
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
