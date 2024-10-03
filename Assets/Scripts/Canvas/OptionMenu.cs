using UnityEngine;
using UnityEngine.Audio;

public class OptionMenu : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    public void FullScreenToggle(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    public void ChangeVolume(float volume)
    {
        audioMixer.SetFloat("Volume", volume);
    }
}
