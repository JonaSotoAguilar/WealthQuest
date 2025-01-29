using UnityEngine;

public class MovementSound : StateMachineBehaviour
{
    [Header("Footstep")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float stepInterval = 1.2f; // Intervalo entre pasos

    private AudioSource audioSource;
    private float stepTimer = 0f;

    // Se ejecuta cuando la animación comienza
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Obtener el AudioSource desde el GameObject del personaje
        audioSource = animator.GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("No se encontró AudioSource en el personaje.");
        }

        PlayFootstepSound();
        stepTimer = 0f; // Reiniciar el temporizador
    }

    // Se ejecuta en cada frame mientras la animación está activa
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (audioSource == null || footstepSounds.Length == 0)
            return;

        stepTimer += Time.deltaTime;

        if (stepTimer >= stepInterval)
        {
            PlayFootstepSound();
            stepTimer = 0f;
        }
    }

    // Se ejecuta cuando la animación termina o cambia a otro estado
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (audioSource != null)
        {
            audioSource.Stop(); // Detener cualquier sonido en curso
        }

        stepTimer = 0f; // Reiniciar el temporizador para la próxima animación
    }

    private void PlayFootstepSound()
    {
        if (footstepSounds.Length > 0 && audioSource != null)
        {
            AudioClip stepSound = footstepSounds[Random.Range(0, footstepSounds.Length)];
            audioSource.PlayOneShot(stepSound);
        }
    }
}
