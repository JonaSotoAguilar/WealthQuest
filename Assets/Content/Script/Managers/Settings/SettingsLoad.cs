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
        SetFullscreen();
        LoadResolution();
        LoadQuality();
    }

    private void LoadLocalContent()
    {
        ContentDatabase.InitializateLocalContent();
    }

    private void SetFullscreen()
    {
        bool isFullscreen = Screen.fullScreen;
        Screen.fullScreen = isFullscreen;
    }

    private void LoadResolution()
    {
        // Filtrar resoluciones desde 1920x1080 en adelante
        Resolution[] resolutions = FilterResolutions(Screen.resolutions, 1920, 1080);

        if (resolutions.Length == 0)
        {
            Debug.LogError("No se encontraron resoluciones iguales o superiores a 1920x1080.");
            return;
        }

        int resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", -1);
        if (resolutionIndex == -1 || resolutionIndex >= resolutions.Length)
        {
            resolutionIndex = resolutions.Length - 1; // Seleccionar la más alta disponible
            PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        }

        // Establecer la resolución
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    private Resolution[] FilterResolutions(Resolution[] allResolutions, int minWidth, int minHeight)
    {
        // Filtrar resoluciones que cumplan con los requisitos mínimos
        System.Collections.Generic.List<Resolution> filtered = new System.Collections.Generic.List<Resolution>();

        foreach (Resolution res in allResolutions)
        {
            if (res.width >= minWidth && res.height >= minHeight)
            {
                filtered.Add(res);
            }
        }

        return filtered.ToArray();
    }

    private void LoadQuality()
    {
        int qualityIndex = PlayerPrefs.GetInt("QualityIndex", 2);
        QualitySettings.SetQualityLevel(qualityIndex);
    }
}