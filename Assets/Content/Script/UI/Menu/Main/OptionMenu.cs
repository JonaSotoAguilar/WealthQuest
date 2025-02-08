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

    [Header("Vsync")]
    [SerializeField] private GameObject activeVsync;
    [SerializeField] private GameObject inactiveVsync;

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
        LoadVSync();
        LoadQuality();
        GetScreen();
    }

    #endregion

    #region Volume

    public void LoadVolume()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.10f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.25f);

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
        // Cargar la opción guardada (1 = Fullscreen, 0 = Windowed)
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        SetFullscreen(isFullscreen);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        // Aplicar el modo de pantalla completa
        Screen.fullScreen = isFullscreen;

        // Guardar en PlayerPrefs (1 = Fullscreen, 0 = Windowed)
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        // Actualizar la UI
        activeFullscreen.SetActive(isFullscreen);
        inactiveFullscreen.SetActive(!isFullscreen);
    }

    #endregion

    #region Vsync

    private void LoadVSync()
    {
        int vSyncEnabled = PlayerPrefs.GetInt("VSync", 1); // 1 = Activado, 0 = Desactivado
        SetVSync(vSyncEnabled == 1);
    }

    public void SetVSync(bool isEnabled)
    {
        QualitySettings.vSyncCount = isEnabled ? 1 : 0; // 1 = Activado (1 sincronización por frame), 0 = Desactivado

        // Guardar en PlayerPrefs
        PlayerPrefs.SetInt("VSync", isEnabled ? 1 : 0);
        PlayerPrefs.Save();

        // Activar/Desactivar los indicadores en la UI
        activeVsync.SetActive(isEnabled);
        inactiveVsync.SetActive(!isEnabled);
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
