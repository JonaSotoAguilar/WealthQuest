using UnityEngine;
using UnityEngine.Playables;

public class TimelineSpeedController : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector; // Asigna el PlayableDirector en el Inspector
    [SerializeField] private float speedMultiplier = 2f; // Multiplicador de velocidad (2x en este caso)

    private void Start()
    {
        if (playableDirector != null)
        {
            playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(speedMultiplier);
            Debug.Log($"Velocidad del Timeline ajustada a {speedMultiplier}x.");
        }
        else
        {
            Debug.LogError("PlayableDirector no asignado.");
        }
    }
}
