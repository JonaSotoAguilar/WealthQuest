using System.Collections.Generic;
using UnityEngine;

public class SettingsLoad : MonoBehaviour
{
    private void Start()
    {
        LoadDataGame();
    }

    private void LoadDataGame()
    {
        LoadLocalContent();
        LoadResolution();
        SetFullscreen();
        LoadQuality();
        LoadVSync();
    }

    private void LoadLocalContent()
    {
        ContentDatabase.InitializateLocalContent();
    }

    private void SetFullscreen()
    {
        // Cargar la opción guardada (1 = Fullscreen, 0 = Windowed), por defecto Fullscreen activado
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        // Aplicar la configuración
        Screen.fullScreen = isFullscreen;

        // Guardar la opción en PlayerPrefs para futuras sesiones
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadResolution()
    {
        // Obtener todas las resoluciones disponibles
        Resolution[] resolutions = Screen.resolutions;

        if (resolutions.Length == 0)
        {
            Debug.LogError("No se encontraron resoluciones disponibles.");
            return;
        }

        // Filtrar resoluciones que no superen 1920x1080
        List<Resolution> validResolutions = new List<Resolution>();
        foreach (var res in resolutions)
        {
            if (res.width <= 1920 && res.height <= 1080)
            {
                validResolutions.Add(res);
            }
        }

        if (validResolutions.Count == 0)
        {
            Debug.LogError("No se encontraron resoluciones dentro del límite de 1920x1080.");
            return;
        }

        // Seleccionar la resolución más alta dentro del límite
        Resolution bestResolution = validResolutions[validResolutions.Count - 1];

        // Aplicar la resolución
        Screen.SetResolution(bestResolution.width, bestResolution.height, Screen.fullScreen);
    }

    private void LoadQuality()
    {
        int qualityIndex = PlayerPrefs.GetInt("QualityIndex", 2);
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    private void LoadVSync()
    {
        int vSyncEnabled = PlayerPrefs.GetInt("VSync", 1); // 1 = Activado, 0 = Desactivado
        SetVSync(vSyncEnabled == 1);
    }

    public void SetVSync(bool isEnabled)
    {
        QualitySettings.vSyncCount = isEnabled ? 1 : 0; // 1 = Activado (1 sincronización por frame), 0 = Desactivado
    }
}