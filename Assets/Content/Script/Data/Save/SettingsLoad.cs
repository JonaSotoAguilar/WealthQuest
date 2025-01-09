using UnityEngine;

public class SettingsLoad : MonoBehaviour
{
    [Header("Game Data")]
    [SerializeField] private Content content;

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
        content.InitializateLocalContent();
    }

    private void SetFullscreen()
    {
        bool isFullscreen = Screen.fullScreen;
        Screen.fullScreen = isFullscreen;
    }

    private void LoadResolution()
    {
        int resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
        Resolution[] resolutions = Screen.resolutions;
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    private void LoadQuality()
    {
        int qualityIndex = PlayerPrefs.GetInt("QualityIndex", 2);
        QualitySettings.SetQualityLevel(qualityIndex);
    }
}