using System.Threading.Tasks;
using UnityEngine;

public class JumpSound : StateMachineBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;

    private float jumpSoundDelay = 0.5f;
    private float landSoundDelay = 1.2f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Obtener el AudioSource desde el personaje o sus hijos
        audioSource = animator.GetComponentInChildren<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("No se encontró un AudioSource en el personaje o sus hijos.");
            return;
        }

        // Llamar a la función async que maneja ambos sonidos con delays
        PlayJumpAndLandSounds();
    }

    private async void PlayJumpAndLandSounds()
    {
        // Esperar el delay para el sonido de salto
        await Task.Delay((int)(jumpSoundDelay * 1000));

        if (audioSource != null && jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }

        // Esperar el delay para el sonido de aterrizaje (desde el inicio de la animación)
        int landDelayFromNow = (int)((landSoundDelay - jumpSoundDelay) * 1000);
        if (landDelayFromNow > 0)
        {
            await Task.Delay(landDelayFromNow);
        }

        if (audioSource != null && landSound != null)
        {
            audioSource.PlayOneShot(landSound);
        }
    }
}
